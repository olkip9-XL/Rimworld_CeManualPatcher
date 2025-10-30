using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Patch
{
    internal class ApparelPatch : PatchBase<ThingDef>
    {
        private StatSaveable stats;
        private StatOffsetSaveable statOffsets;
        private PartialArmorExtSaveable partialArmorExtSaveable;
        private ApparelPropertiesSaveable apparel;

        public override string PatchName => "Apparel";

        public ApparelPatch() { }
        public ApparelPatch(ThingDef apparelDef)
        {
            if (apparelDef == null)
            {
                return;
            }

            this.targetDef = apparelDef;

            stats = new StatSaveable(apparelDef);
            statOffsets = new StatOffsetSaveable(apparelDef);
            partialArmorExtSaveable = new PartialArmorExtSaveable(apparelDef);
            apparel = new ApparelPropertiesSaveable(apparelDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref apparelDefString, "apparelDef");
            Scribe_Deep.Look(ref stats, "statBase");
            Scribe_Deep.Look(ref statOffsets, "statOffsets");
            Scribe_Deep.Look(ref partialArmorExtSaveable, "partialArmorExt");
            Scribe_Deep.Look(ref apparel, "apparelProperties");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string oldDefString = "";
                Scribe_Values.Look(ref oldDefString, "apparelDef");
                if (!oldDefString.NullOrEmpty())
                {
                    targetDefString = oldDefString;
                }
            }
        }

        public override void PostLoadInit()
        {
            if (targetDef == null)
            {
                return;
            }

            stats?.PostLoadInit(targetDef);
            statOffsets?.PostLoadInit(targetDef);
            partialArmorExtSaveable?.PostLoadInit(targetDef);
            apparel?.PostLoadInit(targetDef);

            //new add
            if (apparel == null && targetDef.apparel != null)
            {
                apparel = new ApparelPropertiesSaveable(targetDef);
            }
        }

        public override void Reset()
        {
            stats?.Reset();
            statOffsets?.Reset();
            partialArmorExtSaveable?.Reset();
            apparel?.Reset();
        }

        //public override void ExportPatch(string dirPath)
        //{
        //    string folderPath = Path.Combine(dirPath, targetDef.modContentPack.PackageId);
        //    folderPath = Path.Combine(folderPath, "Apparel");
        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }

        //    string filePath = Path.Combine(folderPath, targetDef.defName + ".xml");

        //    // 创建XML文档
        //    XmlElement rootPatchElement = null;
        //    XmlDocument xmlDoc = XmlUtility.CreateBasePatchDoc(ref rootPatchElement, targetDef.modContentPack.Name);


        //    xmlDoc.Save(filePath);
        //}

        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            XmlUtility.Replace_StatBase(xmlDoc, root, targetDef.defName, targetDef.statBases);
            XmlUtility.AddModExt_PartialArmor(xmlDoc, root, targetDef.defName, targetDef.GetModExtension<PartialArmorExt>());
            XmlUtility.Replace_StatOffsets(xmlDoc, root, targetDef.defName, targetDef.equippedStatOffsets);

            ReplaceApparelProperties();

            void ReplaceApparelProperties()
            {
                if (apparel == null || targetDef.apparel == null)
                {
                    return;
                }

                string xpathBase = $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/apparel";

                root.AppendChild(XmlUtility.PatchConditional(xmlDoc, xpathBase, null,
                      XmlUtility.PatchAdd(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]",
                          xmlDoc.CreateElement("apparel"), "nomatch")
                      ));

                if (apparel.BodyPartGroupsChanged)
                {
                    root.AppendChild(XmlUtility.PatchConditional(xmlDoc, $"{xpathBase}/bodyPartGroups", null,
                        XmlUtility.PatchAdd(xmlDoc, xpathBase,
                            xmlDoc.CreateElement("bodyPartGroups"), "nomatch")
                        ));

                    XmlElement valueElement = xmlDoc.CreateElement("bodyPartGroups");
                    foreach (var item in apparel.Apparel.bodyPartGroups)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement, "li", item.defName);
                    }

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"{xpathBase}/bodyPartGroups", valueElement));
                }

                if (apparel.LayersChanged)
                {
                    root.AppendChild(XmlUtility.PatchConditional(xmlDoc, $"{xpathBase}/layers", null,
                        XmlUtility.PatchAdd(xmlDoc, xpathBase,
                            xmlDoc.CreateElement("layers"), "nomatch")
                        ));

                    XmlElement valueElement = xmlDoc.CreateElement("layers");
                    foreach (var item in apparel.Apparel.layers)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement, "li", item.defName);
                    }
                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"{xpathBase}/layers", valueElement));
                }


            }
        }
    }
}
