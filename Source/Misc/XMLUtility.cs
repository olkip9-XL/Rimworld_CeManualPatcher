using CeManualPatcher.Manager;
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

namespace CeManualPatcher.Misc
{
    public static class XMLUtility
    {
        private static string rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches");
        private static string loadFoldersPath = Path.Combine(rootPath, "LoadFolders.xml");
        private static string ceFoldersPath = Path.Combine(rootPath, "CE\\Patches");

        public static void CreateBasicFolders()
        {
            if (!Directory.Exists(ceFoldersPath))
            {
                Directory.CreateDirectory(ceFoldersPath);
            }

            CreateLoadFolders();
            CreateAboutFolders();

        }
        private static void CreateLoadFolders()
        {
            // 创建 XML 文件
            using (StreamWriter writer = new StreamWriter(loadFoldersPath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<loadFolders>");
                writer.WriteLine("    <v1.5>");
                writer.WriteLine("        <li>/</li>");
                writer.WriteLine("        <li IfModActive=\"CETeam.CombatExtended\">CE</li>");
                writer.WriteLine("        <li IfModActive=\"CETeam.CombatExtended_copy\">CE</li>");
                writer.WriteLine("    </v1.5>");
                writer.WriteLine("</loadFolders>");
            }
        }
        private static void CreateAboutFolders()
        {
            string aboutFolderPath = Path.Combine(rootPath, "About");
            if (!Directory.Exists(aboutFolderPath))
            {
                Directory.CreateDirectory(aboutFolderPath);
            }
            string filePath = Path.Combine(aboutFolderPath, "About.xml");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<ModMetaData>");
                writer.WriteLine("	<name>Custom CE Patch</name>");
                writer.WriteLine("	<author>LotusLand</author>");
                writer.WriteLine("	<packageId>LotusLand.CustomCEPatch</packageId>");
                writer.WriteLine("	<supportedVersions>");
                writer.WriteLine("		<li>1.5</li>");
                writer.WriteLine("	</supportedVersions>");
                writer.WriteLine("	<description>");
                writer.WriteLine("	Custom CE Patch");
                writer.WriteLine("	</description>");
                writer.WriteLine("	<loadAfter>");
                writer.WriteLine("		<li>CETeam.CombatExtended</li>");
                writer.WriteLine("	</loadAfter>");
                writer.WriteLine("</ModMetaData>");
            }
        }

        //public static void CreateCEPatch(ThingDef thingDef)
        //{
        //    string folderPath = Path.Combine(ceFoldersPath, thingDef.modContentPack.Name);

        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }

        //    string filePath = Path.Combine(folderPath, thingDef.defName + ".xml");

        //    CEPatchManager.instance.GetPatcher(thingDef)?.ExportToFile(filePath);
        //}

        public static void AddChildElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            XmlElement element = doc.CreateElement(name);
            element.InnerText = value;
            parent.AppendChild(element);
        }


        public static XmlDocument CreateBasePatchDoc(ref XmlElement rootElement)
        {
            // 创建XML文档
            XmlDocument xmlDoc = new XmlDocument();

            // 声明XML头部
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            //Patch根元素
            XmlElement patchElement = xmlDoc.CreateElement("Patch");
            xmlDoc.AppendChild(patchElement);

            //Operations
            XmlElement operationElement = xmlDoc.CreateElement("Operation");
            operationElement.SetAttribute("Class", "PatchOperationSequence");
            patchElement.AppendChild(operationElement);

            rootElement = xmlDoc.CreateElement("operations");
            operationElement.AppendChild(rootElement);

            return xmlDoc;
        }

        public static void Replace_StatBase(XmlDocument xmlDoc,XmlElement rootElement, string defName, List<StatModifier> stats)
        {
            if (stats == null)
                return;

            XmlElement liElement = xmlDoc.CreateElement("li");
            liElement.SetAttribute("Class", "PatchOperationReplace");
            rootElement.AppendChild(liElement);

            AddChildElement(xmlDoc, liElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]/statBases");

            XmlElement valueElement = xmlDoc.CreateElement("value");
            liElement.AppendChild(valueElement);

            XmlElement statElement = xmlDoc.CreateElement("statBases");
            valueElement.AppendChild(statElement);
            foreach (var stat in stats)
            {
               AddChildElement(xmlDoc, statElement, stat.stat.defName, stat.value.ToString());
            }
        }

        public static void AddModExt_PartialArmor(XmlDocument xmlDoc, XmlElement rootElement, string defName, PartialArmorExt partialArmorExt)
        {
            if (partialArmorExt == null)
                return;

            XmlElement liElement = xmlDoc.CreateElement("li");
            liElement.SetAttribute("Class", "PatchOperationAddModExtension");
            rootElement.AppendChild(liElement);

            AddChildElement(xmlDoc, liElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]");

            XmlElement valueElement = xmlDoc.CreateElement("value");
            liElement.AppendChild(valueElement);

            XmlElement partialArmorExtElement = xmlDoc.CreateElement("CombatExtended.PartialArmorExt");
            valueElement.AppendChild(partialArmorExtElement);

            XmlElement statsElement = xmlDoc.CreateElement("stats");
            partialArmorExtElement.AppendChild(statsElement);
            foreach (var stat in partialArmorExt.stats)
            {
                XmlElement statElement = xmlDoc.CreateElement("li");
                statsElement.AppendChild(statElement);

                if (stat.useStatic)
                {
                    AddChildElement(xmlDoc, statElement, stat.stat.defName, stat.staticValue.ToString());
                    AddChildElement(xmlDoc, statElement, "useStatic", "true");
                }
                else
                {
                    AddChildElement(xmlDoc, statElement, stat.stat.defName, stat.mult.ToString());
                }

                XmlElement partsElement = xmlDoc.CreateElement("parts");
                statElement.AppendChild(partsElement);
                foreach (var part in stat.parts)
                {
                    AddChildElement(xmlDoc, partsElement, "li", part.defName);
                }
            }

        }

    }
}
