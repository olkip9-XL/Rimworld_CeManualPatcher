using CeManualPatcher.Saveable;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    public class WeaponPatch : PatchBase
    {
        private string _defName;
        public ThingDef Def
        {
            get => DefDatabase<ThingDef>.GetNamed(_defName);
            set => _defName = value.defName;
        }

        //字段
        internal List<StatSaveable> statBase = new List<StatSaveable>();

        internal VerbPropertiesCESaveable verbProperties;

        internal List<ToolCESaveable> tools = new List<ToolCESaveable>();

        internal CompAmmoUserSaveable ammoUser;

        internal CompFireModesSaveable fireMode;

        public WeaponPatch() { }
        public WeaponPatch(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            Def = thingDef;

            foreach (var statModifier in thingDef.statBases)
            {
                statBase.Add(new StatSaveable(statModifier));
            }

            if (thingDef.Verbs != null && thingDef.Verbs.Count > 0 && thingDef.Verbs[0] is VerbPropertiesCE)
                verbProperties = new VerbPropertiesCESaveable(thingDef.Verbs[0] as VerbPropertiesCE);
            if (thingDef.tools != null)
            {
                foreach (var tool in thingDef.tools)
                {
                    if (tool is ToolCE)
                        tools.Add(new ToolCESaveable(tool as ToolCE));
                }
            }

            //comps
            if (thingDef.comps != null)
            {
                if (thingDef.HasComp<CompAmmoUser>())
                {
                    ammoUser = new CompAmmoUserSaveable(thingDef);
                }

                if(thingDef.HasComp<CompFireModes>())
                {
                    fireMode = new CompFireModesSaveable(thingDef);
                }
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _defName, "defName");

            Scribe_Collections.Look(ref statBase, "statBase", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && statBase == null)
            {
                statBase = new List<StatSaveable>();
            }

            Scribe_Deep.Look(ref verbProperties, "verbProperties");

            Scribe_Collections.Look(ref tools, "tools", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && tools == null)
            {
                tools = new List<ToolCESaveable>();
            }

            Scribe_Deep.Look(ref ammoUser, "ammoUser");
            Scribe_Deep.Look(ref fireMode, "fireMode");
        }

        public override void Apply()
        {
            statBase.ForEach(x => x?.Apply(Def));
            verbProperties?.Apply(Def);
            tools.ForEach(x => x?.Apply(Def));

            //comps
            ammoUser?.Apply(Def);
            fireMode?.Apply(Def);
        }

        public override void Reset()
        {
            statBase.ForEach(x => x?.Reset(Def));
            verbProperties?.Reset(Def);
            tools.ForEach(x => x?.Reset(Def));

            //sort
            //Def.statBases = Def.statBases.OrderBy(x => x.stat.defName).ToList();
            Def.tools = Def.tools.OrderBy(x => x.id).ToList();

            //comps
            ammoUser?.Reset(Def);
            fireMode?.Reset(Def);
        }

        public override void PostLoadInit()
        {
            if (Def != null)
            {
                this.statBase.ForEach(x => x?.PostLoadInit(Def));
                this.verbProperties?.PostLoadInit(Def);
                this.tools.ForEach(x => x?.PostLoadInit(Def));
                this.ammoUser?.PostLoadInit(Def);
                this.fireMode?.PostLoadInit(Def);
            }
        }
    }
}
