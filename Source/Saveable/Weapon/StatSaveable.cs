using CeManualPatcher.Misc;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class StatSaveable : SaveableBase<ThingDef>
    {
        private Dictionary<string, float> modifierDic = new Dictionary<string, float>();

        //private
        private List<StatModifier> statBases
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                return def.statBases;
            }
        }

        private List<StatModifier> originalData = new List<StatModifier>();
        public List<StatModifier> OriginalStats => originalData;

        public StatSaveable() { }
        public StatSaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        protected override bool NullCheck()
        {
            return statBases.NullOrEmpty();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving &&
                this.def != null)
            {
                modifierDic.Clear();
                foreach (var statModifier in this.def.statBases)
                {
                    if (statModifier.stat != null)
                    {
                        modifierDic[statModifier.stat.defName] = statModifier.value;
                    }
                }
            }

            Scribe_Collections.Look(ref modifierDic, "stats", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (modifierDic == null)
                {
                    modifierDic = new Dictionary<string, float>();
                }
            }
        }

        public override void Reset()
        {
            if (def == null)
                return;

            def.statBases = this.originalData;
            InitOriginalData();
        }

        protected override void Apply()
        {
            if (def == null)
                return;
            def.statBases.Clear();
            foreach (var item in modifierDic)
            {
                StatModifier statModifier = new StatModifier()
                {
                    stat = DefDatabase<StatDef>.GetNamed(item.Key, false),
                    value = item.Value,
                };
                if (statModifier.stat != null)
                    def.statBases.Add(statModifier);
            }

        }


        protected override void InitOriginalData()
        {
            if (this.statBases == null)
                return;

            originalData = CopyUtility.CopyStats(def.statBases);
        }


    }
}
