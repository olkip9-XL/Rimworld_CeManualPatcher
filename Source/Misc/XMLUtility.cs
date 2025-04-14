using CeManualPatcher.Manager;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static void CreateCEPatch(ThingDef thingDef)
        {
            string folderPath = Path.Combine(ceFoldersPath, thingDef.modContentPack.Name);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, thingDef.defName + ".xml");

            CEPatchManager.instance.GetPatcher(thingDef)?.ExportToFile(filePath);
        }

    }
}
