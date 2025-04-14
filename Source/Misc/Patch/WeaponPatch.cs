using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;

using Verse;
using Verse.AI;

namespace CeManualPatcher
{
    public class WeaponPatch : PatchBase
    {
        private string weaponDefString;
        public ThingDef weaponDef
        {
            get => DefDatabase<ThingDef>.GetNamed(weaponDefString, false);
            set => weaponDefString = value?.defName ?? "null";
        }

        //字段
        internal StatSaveable statBase;

        internal VerbPropertiesCESaveable verbProperties;

        internal List<ToolCESaveable> tools = new List<ToolCESaveable>();

        internal CompAmmoUserSaveable ammoUser;

        internal CompFireModesSaveable fireMode;

        //private
        List<Tool> originalTools = new List<Tool>();

        public WeaponPatch() { }
        public WeaponPatch(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            weaponDef = thingDef;

            statBase = new StatSaveable(thingDef);

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
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && weaponDef != null)
            {
                this.tools.Clear();
                foreach (var item in weaponDef.tools)
                {
                    this.tools.Add(new ToolCESaveable(weaponDef, item.id));
                }
            }

            Scribe_Values.Look(ref weaponDefString, "defName");

            Scribe_Deep.Look(ref statBase, "statBase");
            Scribe_Deep.Look(ref verbProperties, "verbProperties");
            Scribe_Deep.Look(ref ammoUser, "ammoUser");
            Scribe_Deep.Look(ref fireMode, "fireMode");
            Scribe_Collections.Look(ref tools, "tools", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (statBase == null)
                {
                    statBase = new StatSaveable(weaponDef);
                }

                if (tools == null)
                {
                    tools = new List<ToolCESaveable>();
                }
            }

        }
        public override void Reset()
        {
            statBase?.Reset();
            verbProperties?.Reset();

            weaponDef.tools = this.originalTools;
            InitTools();

            //comps
            ammoUser?.Reset();
            fireMode?.Reset();
        }

        public override void PostLoadInit()
        {
            if (weaponDef != null)
            {
                this.statBase?.PostLoadInit(weaponDef);

                this.verbProperties?.PostLoadInit(weaponDef);

                InitTools();
                weaponDef.tools.Clear();
                this.tools.ForEach(x => x?.PostLoadInit(weaponDef));


                this.ammoUser?.PostLoadInit(weaponDef);
                this.fireMode?.PostLoadInit(weaponDef);
            }
        }
        private void InitTools()
        {
            if (weaponDef == null || weaponDef.tools == null)
            {
                return;
            }
            originalTools = new List<Tool>();
            foreach (var item in weaponDef.tools)
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
            this.verbProperties = new VerbPropertiesCESaveable(weaponDef, true);
        }

        public void AddAmmoUser()
        {
            this.ammoUser = new CompAmmoUserSaveable(weaponDef, true);
        }

        public void AddFireMode()
        {
            this.fireMode = new CompFireModesSaveable(weaponDef, true);
        }

    }
}
