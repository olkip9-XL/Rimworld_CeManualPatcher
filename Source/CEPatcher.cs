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

namespace CeManualPatcher
{
    public class CEPatcher : IExposable
    {
        private string thingDefString;
        public ThingDef thingDef
        {
            get => DefDatabase<ThingDef>.GetNamed(thingDefString, false);
            set => thingDefString = value?.defName ?? "null";
        }

        private CEPatchManager patchManager => CEPatchManager.instance;
        private WeaponManager weaponManager => WeaponManager.instance;

        //patch data
        internal List<StatModifier> stats = new List<StatModifier>();
        internal VerbPropertiesCE verbProperties;
        internal List<Tool> tools = new List<Tool>();
        internal CompProperties_AmmoUser ammoUser;
        internal CompProperties_FireModes fireMode;


        public CEPatcher() { }
        public CEPatcher(ThingDef thingDef)
        {
            this.thingDef = thingDef;

            if (thingDef == null)
                return;

            weaponManager.Reset(thingDef);
            weaponManager.GetWeaponPatch(thingDef);

            InitNewData();
        }
        public void ApplyCEPatch()
        {
            if (thingDef == null)
            {
                return;
            }

            //add
            if (verbProperties != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddVerb();
            }

            if (ammoUser != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddAmmoUser();
            }

            if (fireMode != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddFireMode();
            }

            ReplaceData();
        }

        private void ReplaceData()
        {
            //statBases

            if (thingDef.statBases != null)
            {
                thingDef.statBases = this.stats;
                HandleStats();
            }

            //verbs
            if (!thingDef.Verbs.NullOrEmpty())
            {
                thingDef.Verbs[0] = this.verbProperties;
            }

            //tools
            if (thingDef.tools != null)
            {
                thingDef.tools = this.tools;
            }

            //comps
            if (thingDef.comps != null)
            {
                if (!thingDef.HasComp<CompAmmoUser>() && ammoUser != null)
                {
                    thingDef.comps.Add(ammoUser);
                }
                if (!thingDef.HasComp<CompFireModes>() && fireMode != null)
                {
                    thingDef.comps.Add(fireMode);
                }
            }
        }

        private void HandleStats()
        {
            StatModifier statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.Recoil);
            if(statModifier != null && this.verbProperties!= null)
            {
                this.verbProperties.recoilAmount = statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.TicksBetweenBurstShots);
            if(statModifier != null && this.verbProperties != null)
            {
                this.verbProperties.ticksBetweenBurstShots = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.BurstShotCount);
            if (statModifier != null && this.verbProperties != null)
            {
                this.verbProperties.burstShotCount = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.MagazineCapacity);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.magazineSize = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.ReloadSpeed);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.reloadTime = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.AmmoGenPerMagOverride);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.AmmoGenPerMagOverride = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.BipodStats);
            if(statModifier != null && this.verbProperties != null)
            {
                thingDef.weaponTags.Add("Bipod_LMG");
            }

        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref thingDefString, "thingDef");
        }
        public void PostLoadInit()
        {
            if (thingDef == null)
            {
                return;
            }
        }

        private void InitNewData()
        {
            //statBases
            if (thingDef.statBases != null)
            {
                this.stats = new List<StatModifier>();
                foreach (var stat in thingDef.statBases)
                {
                    if (MP_Options.exceptStatDefs.Contains(stat.stat))
                    {
                        continue;
                    }

                    StatModifier statModifier = new StatModifier();
                    PropUtility.CopyPropValue(stat, statModifier);
                    this.stats.Add(statModifier);
                }

                // Add CE stats, default AR
                stats.Add(new StatModifier() { stat = CE_StatDefOf.Bulk, value =  10.03f});

                if (!thingDef.Verbs.NullOrEmpty())
                {
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.TicksBetweenBurstShots, value =4 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.BurstShotCount, value = 6 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SightsEfficiency, value =1 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SwayFactor, value = 1.33f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ShotSpread, value = 0.07f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.Recoil, value = 1.5f });

                    stats.Add(new StatModifier() { stat = CE_StatDefOf.MagazineCapacity , value=30f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ReloadSpeed, value =4f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.AmmoGenPerMagOverride });
                }
            }

            //verbs
            if (!thingDef.Verbs.NullOrEmpty())
            {
                this.verbProperties = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(thingDef.Verbs[0]);

                if (this.verbProperties.verbClass == typeof(Verb_Shoot))
                    this.verbProperties.verbClass = typeof(Verb_ShootCE);

                if (this.verbProperties.verbClass == typeof(Verb_ShootOneUse))
                    this.verbProperties.verbClass = typeof(Verb_ShootCEOneUse);

                if (this.verbProperties.verbClass == typeof(Verb_LaunchProjectile))
                    this.verbProperties.verbClass = typeof(Verb_LaunchProjectileCE);

                this.verbProperties.defaultProjectile = MP_ProjectileDefOf.Bullet_556x45mmNATO_FMJ;
            }

            //tools
            if (thingDef.tools != null)
            {
                this.tools = new List<Tool>();
                foreach (var tool in thingDef.tools)
                {
                    ToolCE toolCopy = PropUtility.ConvertToChild<Tool, ToolCE>(tool);

                    List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();
                    capacities.AddRange(tool.capacities);
                    toolCopy.capacities = capacities;

                    this.tools.Add(toolCopy);
                }
            }

            //comps
            if (thingDef.comps != null && !thingDef.Verbs.NullOrEmpty())
            {
                this.ammoUser = new CompProperties_AmmoUser();
                if (thingDef.HasComp<CompAmmoUser>())
                {
                    PropUtility.CopyPropValue(thingDef.GetCompProperties<CompProperties_AmmoUser>(), this.ammoUser);
                }
                this.ammoUser.ammoSet = MP_AmmoSetDefOf.AmmoSet_556x45mmNATO;

                this.fireMode = new CompProperties_FireModes();
                if (thingDef.HasComp<CompFireModes>())
                {
                    PropUtility.CopyPropValue(thingDef.GetCompProperties<CompProperties_FireModes>(), this.fireMode);
                }
                if (fireMode.aimedBurstShotCount <= 0)
                {
                    fireMode.aimedBurstShotCount = 3;
                }
            }
        }

        public void ExportToFile(string dirPath)
        {
            string folderPath = Path.Combine(dirPath, thingDef.modContentPack.Name);
            folderPath = Path.Combine(folderPath, "Weapon");
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath,thingDef.defName + ".xml");

            // 创建XML文档
            XmlDocument xmlDoc = new XmlDocument();

            // 声明XML头部
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDeclaration);

            XmlElement patchElement = xmlDoc.CreateElement("Patch");
            xmlDoc.AppendChild(patchElement);

            XmlElement operationElement = xmlDoc.CreateElement("Operation");
            operationElement.SetAttribute("Class", "PatchOperationSequence");
            patchElement.AppendChild(operationElement);

            XmlElement operationsElement = xmlDoc.CreateElement("operations");
            operationElement.AppendChild(operationsElement);

            XmlElement toolsLiElement = xmlDoc.CreateElement("li");
            toolsLiElement.SetAttribute("Class", "PatchOperationReplace");
            operationsElement.AppendChild(toolsLiElement);

            XmlElement toolsXpath = xmlDoc.CreateElement("xpath");
            toolsXpath.InnerText = $"/Defs/ThingDef[defName=\"{thingDefString}\"]/tools";
            toolsLiElement.AppendChild(toolsXpath);

            XmlElement toolsValue = xmlDoc.CreateElement("value");
            toolsLiElement.AppendChild(toolsValue);

            XmlElement tools = xmlDoc.CreateElement("tools");
            toolsValue.AppendChild(tools);

            foreach (var item in thingDef.tools)
            {
                if (item is ToolCE tool)
                {
                    XmlElement toolElement = xmlDoc.CreateElement("li");
                    toolElement.SetAttribute("Class", "CombatExtended.ToolCE");
                    tools.AppendChild(toolElement);

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
                        if (PropUtility.GetPropValue(defaultTool, propName).ToString() != PropUtility.GetPropValue(item, propName).ToString())
                        {
                            AddChildElement(xmlDoc, toolElement, propName, PropUtility.GetPropValue(item, propName).ToString());
                        }
                    }

                    if (tool.linkedBodyPartsGroup != null)
                        AddChildElement(xmlDoc, toolElement, "linkedBodyPartsGroup", tool.linkedBodyPartsGroup.defName);
                }
            }

            XmlElement cePatchElement = xmlDoc.CreateElement("li");
            cePatchElement.SetAttribute("Class", "CombatExtended.PatchOperationMakeGunCECompatible");
            operationsElement.AppendChild(cePatchElement);

            AddChildElement(xmlDoc, cePatchElement, "defName", thingDef.defName);

            //stat
            XmlElement statBases = xmlDoc.CreateElement("statBases");
            cePatchElement.AppendChild(statBases);

            List<StatModifier> originalStats = weaponManager.GetWeaponPatch(thingDef).statBase.OriginalStats;
            foreach (var item in thingDef.statBases)
            {
                StatModifier originalStatModifier = originalStats.FirstOrDefault(x => x.stat == item.stat);

                if (originalStatModifier == null || Math.Abs(originalStatModifier.value - item.value) > float.Epsilon)
                {
                    AddChildElement(xmlDoc, statBases, item.stat.defName, item.value.ToString());
                }
            }

            //verbProperties

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

            // 保存XML文件
            xmlDoc.Save(filePath);

        }

        // 辅助方法：添加子节点
        static void AddChildElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            XmlElement element = doc.CreateElement(name);
            element.InnerText = value;
            parent.AppendChild(element);
        }

    }
}
