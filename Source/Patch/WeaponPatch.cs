using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;
using Verse.AI;

namespace CeManualPatcher.Patch
{
    public class WeaponPatch : PatchBase<ThingDef>
    {
        //字段
        internal StatSaveable statBase;

        internal StatOffsetSaveable statOffsets;

        internal VerbPropertiesCESaveable verbProperties;

        internal List<ToolCESaveable> tools = new List<ToolCESaveable>();

        internal CompAmmoUserSaveable ammoUser;

        internal CompFireModesSaveable fireMode;

        internal CompChargeSaveable charges;

        internal WeaponTagsSaveable weaponTags;

        //private
        List<Tool> originalTools = new List<Tool>();
        public override string PatchName => "Weapon";

        public WeaponPatch() { }
        public WeaponPatch(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            targetDef = thingDef;

            statBase = new StatSaveable(thingDef);
            statOffsets = new StatOffsetSaveable(thingDef);

            verbProperties = new VerbPropertiesCESaveable(thingDef);

            //if (!thingDef.Verbs.NullOrEmpty() && thingDef.Verbs[0] is VerbPropertiesCE)
            //    verbProperties = new VerbPropertiesCESaveable(thingDef);

            if (thingDef.tools != null)
            {
                InitTools();
                foreach (var tool in thingDef.tools)
                {
                    if (tool is ToolCE)
                    {
                        tools.Add(new ToolCESaveable(thingDef, tool.id));
                    }
                }
            }

            //comps
            if (thingDef.comps != null)
            {
                ammoUser = new CompAmmoUserSaveable(thingDef);

                fireMode = new CompFireModesSaveable(thingDef);

                charges = new CompChargeSaveable(thingDef);
            }

            //weapon tags
            if (thingDef.weaponTags != null)
                weaponTags = new WeaponTagsSaveable(thingDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving && targetDef != null)
            {
                this.tools.Clear();
                if (targetDef.tools != null)
                    foreach (var item in targetDef.tools)
                    {
                        this.tools.Add(new ToolCESaveable(targetDef, item.id));
                    }
            }

            //Scribe_Values.Look(ref weaponDefString, "defName");

            Scribe_Deep.Look(ref statBase, "statBase");
            Scribe_Deep.Look(ref statOffsets, "statOffsets");
            Scribe_Deep.Look(ref verbProperties, "verbProperties");

            //comps
            Scribe_Deep.Look(ref ammoUser, "ammoUser");
            Scribe_Deep.Look(ref fireMode, "fireMode");
            Scribe_Deep.Look(ref charges, "charges");

            Scribe_Collections.Look(ref tools, "tools", LookMode.Deep);
            Scribe_Deep.Look(ref weaponTags, "weaponTags");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (tools == null)
                {
                    tools = new List<ToolCESaveable>();
                }

                //old save compatibility
                if (ammoUser == null)
                {
                    ammoUser = new CompAmmoUserSaveable(targetDef);
                }

                if (fireMode == null)
                {
                    fireMode = new CompFireModesSaveable(targetDef);
                }

                if (charges == null)
                {
                    charges = new CompChargeSaveable(targetDef);
                }
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string oldDefString = "";
                Scribe_Values.Look(ref oldDefString, "defName");
                if (!oldDefString.NullOrEmpty())
                {
                    targetDefString = oldDefString;
                }
            }

        }
        public override void Reset()
        {
            statBase?.Reset();
            statOffsets?.Reset();
            verbProperties?.Reset();

            if (targetDef.tools != null)
            {
                targetDef.tools = this.originalTools;
                InitTools();
            }

            //comps
            ammoUser?.Reset();
            fireMode?.Reset();
            charges?.Reset();

            //weapontags
            weaponTags?.Reset();
        }

        public override void PostLoadInit()
        {
            if (targetDef != null)
            {
                this.statBase?.PostLoadInit(targetDef);

                this.statOffsets?.PostLoadInit(targetDef);

                this.verbProperties?.PostLoadInit(targetDef);

                if (targetDef.tools != null)
                {
                    InitTools();
                    targetDef.tools.Clear();
                    this.tools.ForEach(x => x?.PostLoadInit(targetDef));
                }

                this.ammoUser?.PostLoadInit(targetDef);
                this.fireMode?.PostLoadInit(targetDef);
                this.charges?.PostLoadInit(targetDef);

                this.weaponTags?.PostLoadInit(targetDef);

                //new add 
                if (weaponTags == null && targetDef.weaponTags != null)
                {
                    weaponTags = new WeaponTagsSaveable(targetDef);
                }

                if (statOffsets == null)
                {
                    statOffsets = new StatOffsetSaveable(targetDef);
                }
            }
        }
        private void InitTools()
        {
            if (targetDef == null || targetDef.tools == null)
            {
                return;
            }
            originalTools = new List<Tool>();
            foreach (var item in targetDef.tools)
            {
                Tool tool = new Tool();

                if (item is ToolCE)
                {
                    tool = new ToolCE();
                    PropUtility.CopyPropValue<ToolCE>(item as ToolCE, tool as ToolCE);
                }
                else
                {
                    PropUtility.CopyPropValue<Tool>(item, tool);
                }

                List<ToolCapacityDef> toolCapacityDefs = new List<ToolCapacityDef>(item.capacities);
                tool.capacities = toolCapacityDefs;

                this.originalTools.Add(tool);
            }
        }


        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            bool needCEPatch = verbProperties.NeedCEPatch;

            XmlUtility.Replace_Tools(xmlDoc, root, targetDef.defName, targetDef.tools);
            if (needCEPatch)
            {
                XmlUtility.MakeGunCECompatible(xmlDoc, root, targetDef);
            }
            XmlUtility.Replace_StatOffsets(xmlDoc, root, targetDef.defName, targetDef.equippedStatOffsets);

            //comps
            root.AppendChild(XmlUtility.PatchAddCompRoot(xmlDoc, targetDef.defName));

            if (!needCEPatch)
            {
                //todo: normal patch stat, verb, weapontags
                XmlUtility.Replace_StatBase(xmlDoc, root, targetDef.defName, targetDef.statBases);

                ReplaceVerb();

                ReplaceWeaponTags();
            }

            MakeCompPatch_Charges();

            if (!needCEPatch)
            {
                MakeCompPatch_AmmoUser();
                MakeCompPatch_FireModes();
            }

            void MakeCompPatch_Charges()
            {
                if (!this.charges.CompChanged)
                    return;

                CompProperties_Charges compProps = targetDef.GetCompProperties<CompProperties_Charges>();
                if (compProps == null)
                    return;

                XmlElement valueElement = xmlDoc.CreateElement("li");
                valueElement.SetAttribute("Class", typeof(CompProperties_Charges).ToString());

                XmlUtility.AddChildElementList(xmlDoc, valueElement, "chargeSpeeds", compProps.chargeSpeeds.Select(x => x.ToString()));

                XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_Charges).ToString());
            }

            void MakeCompPatch_AmmoUser()
            {
                if (!targetDef.HasComp<CompAmmoUser>())
                {
                    return;
                }

                CompProperties_AmmoUser defaultAmmoUser = new CompProperties_AmmoUser();
                CompProperties_AmmoUser currentAmmoUser = targetDef.GetCompProperties<CompProperties_AmmoUser>();

                if (currentAmmoUser.ammoSet == null)
                    return;

                XmlElement valueElement = xmlDoc.CreateElement("li");
                valueElement.SetAttribute("Class", typeof(CompProperties_AmmoUser).ToString());

                foreach (var propName in CompAmmoUserSaveable.propNames)
                {
                    if (PropUtility.GetPropValue(defaultAmmoUser, propName).ToString() != PropUtility.GetPropValue(currentAmmoUser, propName).ToString())
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement, propName, PropUtility.GetPropValue(currentAmmoUser, propName).ToString());
                    }
                }

                if (currentAmmoUser.ammoSet != defaultAmmoUser.ammoSet)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElement, "ammoSet", currentAmmoUser.ammoSet.defName);
                }

                XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_AmmoUser).ToString());
            }

            void MakeCompPatch_FireModes()
            {
                if (!targetDef.HasComp<CompFireModes>())
                {
                    return;
                }

                XmlElement valueElement = xmlDoc.CreateElement("li");
                valueElement.SetAttribute("Class", typeof(CompProperties_FireModes).ToString());

                CompProperties_FireModes defaultFireMode = new CompProperties_FireModes();
                CompProperties_FireModes currentFireMode = targetDef.GetCompProperties<CompProperties_FireModes>();

                foreach (var propName in CompFireModesSaveable.propNames)
                {
                    if (PropUtility.GetPropValue(defaultFireMode, propName).ToString() != PropUtility.GetPropValue(currentFireMode, propName).ToString())
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement, propName, PropUtility.GetPropValue(currentFireMode, propName).ToString());
                    }
                }

                if (currentFireMode.aiAimMode != defaultFireMode.aiAimMode)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElement, "aiAimMode", currentFireMode.aiAimMode.ToString());
                }

                XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_FireModes).ToString());
            }

            void ReplaceVerb()
            {
                if (targetDef.Verbs.NullOrEmpty() || !(targetDef.Verbs[0] is VerbPropertiesCE))
                {
                    return;
                }

                //add if not exist
                XmlElement nomatchElement = XmlUtility.PatchAdd(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]", xmlDoc.CreateElement("verbs"), "nomatch");

                root.AppendChild(XmlUtility.PatchConditional(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/verbs", null, nomatchElement));

                //replace
                XmlElement valueElement = xmlDoc.CreateElement("verbs");
                XmlElement valueElementLi = xmlDoc.CreateElement("li");
                valueElementLi.SetAttribute("Class", typeof(VerbPropertiesCE).ToString());
                valueElement.AppendChild(valueElementLi);

                VerbPropertiesCE defaultVerb = new VerbPropertiesCE();
                VerbPropertiesCE currentVerb = targetDef.Verbs[0] as VerbPropertiesCE;

                XmlUtility.AddChildElement(xmlDoc, valueElementLi, "verbClass", currentVerb.verbClass.ToString());

                foreach (var propName in VerbPropertiesCESaveable.propNames)
                {
                    if (PropUtility.GetPropValue(defaultVerb, propName).ToString() != PropUtility.GetPropValue(currentVerb, propName).ToString())
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElementLi, propName, PropUtility.GetPropValue(currentVerb, propName).ToString());
                    }
                }

                if (currentVerb.recoilPattern != defaultVerb.recoilPattern)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElementLi, "recoilPattern", currentVerb.recoilPattern.ToString());
                }

                if (currentVerb.defaultProjectile != defaultVerb.defaultProjectile)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElementLi, "defaultProjectile", currentVerb.defaultProjectile.defName);
                }

                if (currentVerb.soundCast != defaultVerb.soundCast)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElementLi, "soundCast", currentVerb.soundCast.defName);
                }

                if (currentVerb.soundCastTail != defaultVerb.soundCastTail)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElementLi, "soundCastTail", currentVerb.soundCastTail.defName);
                }

                root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/verbs", valueElement));
            }

            void ReplaceWeaponTags()
            {
                if (targetDef.weaponTags.NullOrEmpty())
                {
                    return;
                }

                //add if not exist
                XmlElement nomatchElement = XmlUtility.PatchAdd(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]", xmlDoc.CreateElement("weaponTags"), "nomatch");
                root.AppendChild(XmlUtility.PatchConditional(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/weaponTags", null, nomatchElement));

                //replace
                XmlElement valueElement = xmlDoc.CreateElement("weaponTags");
                foreach (var tag in targetDef.weaponTags)
                {
                    XmlUtility.AddChildElement(xmlDoc, valueElement, "li", tag);
                }

                root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/weaponTags", valueElement));
            }
        }
    }
}
