using CeManualPatcher.Saveable.Body;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Misc.Patch
{
    internal class BodyDefPatch : PatchBase<BodyDef>
    {

        //字段
        List<BodyPartRecordSaveable> records = new List<BodyPartRecordSaveable>();

        public BodyDefPatch() { }
        public BodyDefPatch(BodyDef bodyDef)
        {
            targetDef = bodyDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref records, "records", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (records == null)
                {
                    records = new List<BodyPartRecordSaveable>();
                }
            }
        }

        public override void PostLoadInit()
        {
            foreach (BodyPartRecordSaveable record in records)
            {
                try
                {
                    record?.PostLoadInit(targetDef);
                }
                catch (Exception e)
                {
                    Log.Error($"[CE Manual Patcher] BodyPatch PostLoadInit error: {e}");
                }
            }

        }

        public override void Reset()
        {
            foreach (var record in records)
            {
                try
                {
                    record?.Reset();
                }
                catch (Exception e)
                {
                    Log.Error($"[CE Manual Patcher] BodyPatch Reset error: {e}");
                }
            }
            records.Clear();
        }

        public void AddBodyPart(List<int> path)
        {
            if (GetRecordPatch(path) != null)
            {
                return;
            }

            BodyPartRecordSaveable record = new BodyPartRecordSaveable(targetDef, path);
            if (record != null)
            {
                records.Add(record);
            }
            else
            {
                Log.Error($"[CE Manual Patcher] BodyPatch AddBodyPart error: {path}");
            }
        }

        public override void ExportPatch(string dirPath)
        {
            string folderPath = Path.Combine(dirPath, targetDef.modContentPack.Name);
            folderPath = Path.Combine(folderPath, "Body");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, targetDef.defName + ".xml");

            XmlElement root = null;
            XmlDocument xmlDocument = XMLUtility.CreateBasePatchDoc(ref root);

            CreateGroupElement(targetDef.corePart, new List<string>());
            CreateArmorCoverage(targetDef.corePart, new List<int>());
            CreateDataReplace(targetDef.corePart);

            xmlDocument.Save(filePath);

            void CreateGroupElement(BodyPartRecord record, List<string> defNames)
            {
                XmlElement liElement = xmlDocument.CreateElement("li");
                liElement.SetAttribute("Class", "PatchOperationConditional");
                root.AppendChild(liElement);

                string xpath = $"Defs/BodyDef[defName=\"{targetDef.defName}\"]/corePart";
                foreach (var item in defNames)
                {
                    xpath += $"/parts/li[def=\"{item}\"]";
                }

                XMLUtility.AddChildElement(xmlDocument, liElement, "xpath", xpath + "/groups");

                XmlElement nomatchElement = xmlDocument.CreateElement("nomatch");
                nomatchElement.SetAttribute("Class", "PatchOperationAdd");
                liElement.AppendChild(nomatchElement);

                XMLUtility.AddChildElement(xmlDocument, nomatchElement, "xpath", xpath);

                XmlElement valueElement = xmlDocument.CreateElement("value");
                nomatchElement.AppendChild(valueElement);

                XMLUtility.AddChildElement(xmlDocument, valueElement, "groups", "");

                if (!record.parts.NullOrEmpty())
                {
                    foreach (var part in record.parts)
                    {
                        defNames.Add(part.def.defName);
                        CreateGroupElement(part, defNames);
                        defNames.Pop();
                    }
                }
            }

            void CreateArmorCoverage(BodyPartRecord record, List<int> path)
            {
                BodyPartRecordSaveable recordPatch = GetRecordPatch(path);

                if (recordPatch != null)
                {
                    if (BodyPartRecordSaveable.GetCoverByArmor(record) && !recordPatch.OriginalDataCoverByArmor)
                    {
                        XmlElement liElement = xmlDocument.CreateElement("li");
                        liElement.SetAttribute("Class", "PatchOperationAdd");
                        root.AppendChild(liElement);

                        string xpath = GetXpath(path);

                        XMLUtility.AddChildElement(xmlDocument, liElement, "xpath", xpath + "/groups");

                        XmlElement valueElement = xmlDocument.CreateElement("value");
                        liElement.AppendChild(valueElement);

                        XMLUtility.AddChildElement(xmlDocument, valueElement, "li", "CoveredByNaturalArmor");
                    }

                    if (!BodyPartRecordSaveable.GetCoverByArmor(record) && recordPatch.OriginalDataCoverByArmor)
                    {
                        XmlElement liElement = xmlDocument.CreateElement("li");
                        liElement.SetAttribute("Class", "PatchOperationRemove");
                        root.AppendChild(liElement);
                        string xpath = GetXpath(path);
                        XMLUtility.AddChildElement(xmlDocument, liElement, "xpath", xpath + "/groups/li[text()='CoveredByNaturalArmor']");
                    }
                }

                if (!record.parts.NullOrEmpty())
                {
                    for (int i = 0; i < record.parts.Count; i++)
                    {
                        path.Add(i);
                        CreateArmorCoverage(record.parts[i], path);
                        path.Pop();
                    }
                }
            }

            void CreateDataReplace(BodyPartRecord record)
            {
                foreach (var item in this.records)
                {
                    string xpath = GetXpath(item.path);

                    if (Math.Abs(record.coverage - item.OriginalData.coverage) > float.Epsilon)
                    {
                        root.AppendChild(PatchReplace(xmlDocument, xpath, "coverage", record.coverage.ToString()));
                    }
                    if (record.height != item.OriginalData.height)
                    {
                        root.AppendChild(PatchReplace(xmlDocument, xpath, "height", record.height.ToString()));
                    }
                    if (record.depth != item.OriginalData.depth)
                    {
                        root.AppendChild(PatchReplace(xmlDocument, xpath, "depth", record.depth.ToString()));
                    }
                    if (record.customLabel != item.OriginalData.customLabel)
                    {
                        root.AppendChild(PatchReplace(xmlDocument, xpath, "customLabel", record.customLabel.ToString()));
                    }
                }


            }

            string GetXpath(List<int> path)
            {
                string xpath = $"Defs/BodyDef[defName=\"{targetDef.defName}\"]/corePart";

                BodyPartRecord curRecord = targetDef.corePart;
                foreach (var item in path)
                {
                    curRecord = curRecord.parts[item];
                    xpath += $"/parts/li[def=\"{curRecord.def.defName}\"]";
                }

                return xpath;
            }

            XmlElement PatchReplace(XmlDocument doc, string xpath, string propName, string value)
            {

                XmlElement liElement = doc.CreateElement("li");
                liElement.SetAttribute("Class", "PatchOperationReplace");

                XMLUtility.AddChildElement(doc, liElement, "xpath", xpath + "/" + propName);

                XmlElement valueElement = doc.CreateElement("value");
                liElement.AppendChild(valueElement);
                XMLUtility.AddChildElement(doc, valueElement, propName, value);
                return liElement;
            }
        }

        private BodyPartRecordSaveable GetRecordPatch(List<int> path)
        {
            if (path == null)
                return null;

            return records.FirstOrDefault(x => x.path.SequenceEqual(path));
        }


    }
}
