using CeManualPatcher.Manager;
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

namespace CeManualPatcher.Misc
{
    public static class XMLUtility
    {
        private static WeaponManager weaponManager => WeaponManager.instance;


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

        public static void Replace_StatBase(XmlDocument xmlDoc, XmlElement rootElement, string defName, List<StatModifier> stats)
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

            XmlElement partialArmorExtElement = xmlDoc.CreateElement("li");
            partialArmorExtElement.SetAttribute("Class", "CombatExtended.PartialArmorExt");
            valueElement.AppendChild(partialArmorExtElement);

            XmlElement statsElement = xmlDoc.CreateElement("stats");
            partialArmorExtElement.AppendChild(statsElement);
            foreach (var stat in partialArmorExt.stats ?? new List<ApparelPartialStat>())
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
        public static void Replace_Tools(XmlDocument xmlDoc, XmlElement rootElement, string defName, List<Tool> tools)
        {
            if (tools.NullOrEmpty())
                return;

            XmlElement liElement = xmlDoc.CreateElement("li");
            liElement.SetAttribute("Class", "PatchOperationReplace");
            rootElement.AppendChild(liElement);
            AddChildElement(xmlDoc, liElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]/tools");
            XmlElement valueElement = xmlDoc.CreateElement("value");
            liElement.AppendChild(valueElement);

            XmlElement toolsElement = xmlDoc.CreateElement("tools");
            valueElement.AppendChild(toolsElement);
            foreach (var tool in tools)
            {
                if (tool is ToolCE)
                {
                    XmlElement toolElement = xmlDoc.CreateElement("li");
                    toolElement.SetAttribute("Class", "CombatExtended.ToolCE");
                    toolsElement.AppendChild(toolElement);

                    ToolCE defaultTool = new ToolCE();

                    AddChildElement(xmlDoc, toolElement, "label", tool.label);

                    //capacities
                    XmlElement capacityElement = xmlDoc.CreateElement("capacities");
                    foreach (var capacity in tool.capacities)
                    {
                        AddChildElement(xmlDoc, capacityElement, "li", capacity.defName);
                    }
                    toolElement.AppendChild(capacityElement);


                    //surprise attack
                    if (tool.surpriseAttack != null && tool.surpriseAttack.extraMeleeDamages != null)
                    {
                        XmlElement surpriseAttackElement = xmlDoc.CreateElement("surpriseAttack");
                        toolElement.AppendChild(surpriseAttackElement);

                        XmlElement extraMeleeDamageElement = xmlDoc.CreateElement("extraMeleeDamages");
                        surpriseAttackElement.AppendChild(extraMeleeDamageElement);

                        foreach (var damage in tool.surpriseAttack.extraMeleeDamages)
                        {
                            XmlElement listElement = xmlDoc.CreateElement("li");

                            AddChildElement(xmlDoc, listElement, "def", damage.def.defName);
                            AddChildElement(xmlDoc, listElement, "amount", damage.amount.ToString());
                            AddChildElement(xmlDoc, listElement, "chance", damage.chance.ToString());

                            extraMeleeDamageElement.AppendChild(listElement);
                        }
                        toolElement.AppendChild(surpriseAttackElement);
                    }
                    //extra damage
                    if (tool.extraMeleeDamages != null)
                    {
                        XmlElement extraDamageElement = xmlDoc.CreateElement("extraMeleeDamages");
                        foreach (var damage in tool.surpriseAttack.extraMeleeDamages)
                        {
                            XmlElement listElement = xmlDoc.CreateElement("li");

                            AddChildElement(xmlDoc, listElement, "def", damage.def.defName);
                            AddChildElement(xmlDoc, listElement, "amount", damage.amount.ToString());
                            AddChildElement(xmlDoc, listElement, "chance", damage.chance.ToString());

                            extraDamageElement.AppendChild(listElement);
                        }
                        toolElement.AppendChild(extraDamageElement);
                    }

                    foreach (var propName in ToolCESaveable.propNames)
                    {
                        if (PropUtility.GetPropValue(defaultTool, propName).ToString() != PropUtility.GetPropValue(tool, propName).ToString())
                        {
                            AddChildElement(xmlDoc, toolElement, propName, PropUtility.GetPropValue(tool, propName).ToString());
                        }
                    }

                    if (tool.linkedBodyPartsGroup != null)
                        AddChildElement(xmlDoc, toolElement, "linkedBodyPartsGroup", tool.linkedBodyPartsGroup.defName);
                }
            }
        }

        public static void MakeGunCECompatible(XmlDocument xmlDoc, XmlElement rootElement, ThingDef thingDef)
        {
            if (thingDef == null)
                return;

            //frame
            XmlElement cePatchElement = xmlDoc.CreateElement("li");
            cePatchElement.SetAttribute("Class", "CombatExtended.PatchOperationMakeGunCECompatible");
            rootElement.AppendChild(cePatchElement);

            AddChildElement(xmlDoc, cePatchElement, "defName", thingDef.defName);

            MakeStat();
            MakeVerb();
            MakeAmmoUser();
            MakeFireMode();
            MakeWeaponTags();

            void MakeStat()
            {
                //stats
                XmlElement statBases = xmlDoc.CreateElement("statBases");
                cePatchElement.AppendChild(statBases);

                if (thingDef.statBases != null)
                {
                    List<StatModifier> originalStats = weaponManager.GetWeaponPatch(thingDef).statBase.OriginalStats;
                    foreach (var item in thingDef.statBases)
                    {
                        StatModifier originalStatModifier = originalStats.FirstOrDefault(x => x.stat == item.stat);

                        if (originalStatModifier == null || Math.Abs(originalStatModifier.value - item.value) > float.Epsilon)
                        {
                            AddChildElement(xmlDoc, statBases, item.stat.defName, item.value.ToString());
                        }
                    }
                }
            }

            void MakeVerb()
            {
                //verb
                if (!thingDef.Verbs.NullOrEmpty())
                {
                    XmlElement verbPropertiesElement = xmlDoc.CreateElement("Properties");
                    cePatchElement.AppendChild(verbPropertiesElement);


                    VerbPropertiesCE defaultVerb = new VerbPropertiesCE();
                    VerbPropertiesCE currentVerb = thingDef.Verbs[0] as VerbPropertiesCE;

                    AddChildElement(xmlDoc, verbPropertiesElement, "verbClass", currentVerb.verbClass.ToString());

                    foreach (var propName in VerbPropertiesCESaveable.propNames)
                    {
                        if (PropUtility.GetPropValue(defaultVerb, propName).ToString() != PropUtility.GetPropValue(currentVerb, propName).ToString())
                        {
                            AddChildElement(xmlDoc, verbPropertiesElement, propName, PropUtility.GetPropValue(currentVerb, propName).ToString());
                        }
                    }

                    if (currentVerb.recoilPattern != defaultVerb.recoilPattern)
                    {
                        AddChildElement(xmlDoc, verbPropertiesElement, "recoilPattern", currentVerb.recoilPattern.ToString());
                    }

                    if (currentVerb.defaultProjectile != defaultVerb.defaultProjectile)
                    {
                        AddChildElement(xmlDoc, verbPropertiesElement, "defaultProjectile", currentVerb.defaultProjectile.defName);
                    }
                }
            }

            void MakeAmmoUser()
            {
                //ammo user
                if (thingDef.HasComp<CompAmmoUser>())
                {
                    XmlElement ammoUserElement = xmlDoc.CreateElement("AmmoUser");
                    cePatchElement.AppendChild(ammoUserElement);

                    CompProperties_AmmoUser defaultAmmoUser = new CompProperties_AmmoUser();
                    CompProperties_AmmoUser currentAmmoUser = thingDef.GetCompProperties<CompProperties_AmmoUser>();

                    foreach (var propName in CompAmmoUserSaveable.propNames)
                    {
                        if (PropUtility.GetPropValue(defaultAmmoUser, propName).ToString() != PropUtility.GetPropValue(currentAmmoUser, propName).ToString())
                        {
                            AddChildElement(xmlDoc, ammoUserElement, propName, PropUtility.GetPropValue(currentAmmoUser, propName).ToString());
                        }
                    }

                    if (currentAmmoUser.ammoSet != defaultAmmoUser.ammoSet)
                    {
                        AddChildElement(xmlDoc, ammoUserElement, "ammoSet", currentAmmoUser.ammoSet.defName);
                    }
                }
            }

            void MakeFireMode()
            {

                //fire mode
                if (thingDef.HasComp<CompFireModes>())
                {
                    XmlElement fireModeElement = xmlDoc.CreateElement("FireModes");
                    cePatchElement.AppendChild(fireModeElement);

                    CompProperties_FireModes defaultFireMode = new CompProperties_FireModes();
                    CompProperties_FireModes currentFireMode = thingDef.GetCompProperties<CompProperties_FireModes>();

                    foreach (var propName in CompFireModesSaveable.propNames)
                    {
                        if (PropUtility.GetPropValue(defaultFireMode, propName).ToString() != PropUtility.GetPropValue(currentFireMode, propName).ToString())
                        {
                            AddChildElement(xmlDoc, fireModeElement, propName, PropUtility.GetPropValue(currentFireMode, propName).ToString());
                        }
                    }

                    if (currentFireMode.aiAimMode != defaultFireMode.aiAimMode)
                    {
                        AddChildElement(xmlDoc, fireModeElement, "aiAimMode", currentFireMode.aiAimMode.ToString());
                    }
                }
            }

            void MakeWeaponTags()
            {
                //weapontags
                if (thingDef.weaponTags != null)
                {
                    XmlElement weaponTagsElement = xmlDoc.CreateElement("weaponTags");
                    cePatchElement.AppendChild(weaponTagsElement);
                    foreach (var tag in thingDef.weaponTags)
                    {
                        AddChildElement(xmlDoc, weaponTagsElement, "li", tag);
                    }
                }
            }
        }

        public static void Replace_StatOffsets(XmlDocument xmlDoc, XmlElement rootElement, string defName, List<StatModifier> statOffsets)
        {
            if (statOffsets == null)
                return;


            XmlElement liElement = xmlDoc.CreateElement("li");
            liElement.SetAttribute("Class", "PatchOperationConditional");
            AddChildElement(xmlDoc, liElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]");

            XmlElement matchElement = xmlDoc.CreateElement("match");
            liElement.AppendChild(matchElement);
            AddChildElement(xmlDoc, matchElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]/equippedStatOffsets");


            //success
            XmlElement successElement = xmlDoc.CreateElement("success");
            XmlElement xmlElement = xmlDoc.CreateElement("Operation");
            xmlElement.SetAttribute("Class", "PatchOperationReplace");
            AddChildElement(xmlDoc, xmlElement, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]/equippedStatOffsets");
            CreateValue(xmlElement);

            //fail
            XmlElement failElement = xmlDoc.CreateElement("fail");
            XmlElement xmlElement2 = xmlDoc.CreateElement("Operation");
            xmlElement2.SetAttribute("Class", "PatchOperationAdd");
            AddChildElement(xmlDoc, xmlElement2, "xpath", $"Defs/ThingDef[defName=\"{defName}\"]");
            CreateValue(xmlElement2);

            void CreateValue(XmlElement operationElement)
            {
                XmlElement valueElement = xmlDoc.CreateElement("value");
                operationElement.AppendChild(valueElement);

                XmlElement statElement = xmlDoc.CreateElement("equippedStatOffsets");
                valueElement.AppendChild(statElement);
                foreach (var stat in statOffsets)
                {
                    AddChildElement(xmlDoc, statElement, stat.stat.defName, stat.value.ToString());
                }
            }

        }
    
    
    
    
        // Defs
        public static XmlDocument CreateBaseDefDoc(ref XmlElement rootElement)
        {
            // 创建XML文档
            XmlDocument xmlDoc = new XmlDocument();
            // 声明XML头部
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDeclaration);
            //Defs根元素
            rootElement = xmlDoc.CreateElement("Defs");
            xmlDoc.AppendChild(rootElement);
           
            return xmlDoc;
        }

        public static void AddChildElementList(XmlDocument doc, XmlElement root, string listName, List<string> list)
        {
            if (list == null)
                return;

            XmlElement liElement = doc.CreateElement(listName);
            root.AppendChild(liElement);
            foreach (var item in list)
            {
                AddChildElement(doc, liElement, "li", item);
            }
        }
    
    
    }
}
