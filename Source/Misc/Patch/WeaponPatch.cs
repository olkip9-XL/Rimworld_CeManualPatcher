﻿using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Patch;
using CeManualPatcher.Saveable;
using CeManualPatcher.Saveable.Comps;
using CeManualPatcher.Saveable.Weapon;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;

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

            if (!thingDef.Verbs.NullOrEmpty() && thingDef.Verbs[0] is VerbPropertiesCE)
                verbProperties = new VerbPropertiesCESaveable(thingDef);

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
                if (thingDef.HasComp<CompAmmoUser>())
                {
                    ammoUser = new CompAmmoUserSaveable(thingDef);
                }

                if (thingDef.HasComp<CompFireModes>())
                {
                    fireMode = new CompFireModesSaveable(thingDef);
                }

                //comp charges
                if (thingDef.HasComp<CompCharges>())
                {
                    charges = new CompChargeSaveable(thingDef);
                }

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
                
                if(statOffsets == null)
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
                }

                PropUtility.CopyPropValue(item, tool);

                List<ToolCapacityDef> toolCapacityDefs = new List<ToolCapacityDef>(item.capacities);
                tool.capacities = toolCapacityDefs;

                this.originalTools.Add(tool);
            }
        }

        public void AddVerb()
        {
            this.verbProperties = new VerbPropertiesCESaveable(targetDef, true);
        }

        public void AddAmmoUser()
        {
            this.ammoUser = new CompAmmoUserSaveable(targetDef, true);
        }

        public void AddFireMode()
        {
            this.fireMode = new CompFireModesSaveable(targetDef, true);
        }

        public void AddCharges()
        {
            this.charges = new CompChargeSaveable(targetDef, true);
        }

        public override void ExportPatch(string dirPath)
        {
            throw new NotImplementedException();
        }
    }
}
