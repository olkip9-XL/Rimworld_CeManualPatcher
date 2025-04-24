using CeManualPatcher.Misc;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Weapon
{
    internal class StatOffsetSaveable : SaveableBase
    {
        private Dictionary<string, float> modifierDic = new Dictionary<string, float>();

        //private
        private List<StatModifier> statOffsets
        {
            get
            {
                if (thingDef == null)
                {
                    return null;
                }
                return thingDef.equippedStatOffsets;
            }
        }

        private List<StatModifier> originalData = new List<StatModifier>();
        public List<StatModifier> OriginalStats => originalData;

        public StatOffsetSaveable() { }
        public StatOffsetSaveable(ThingDef thingDef) : base(thingDef)
        {
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving &&
               this.statOffsets != null)
            {
                modifierDic.Clear();
                foreach (var statModifier in this.statOffsets)
                {
                    if (statModifier.stat != null)
                    {
                        modifierDic[statModifier.stat.defName] = statModifier.value;
                    }
                }
            }

            Scribe_Collections.Look(ref modifierDic, "statOffsets", LookMode.Value, LookMode.Value);
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
            if (this.thingDef == null)
                return;

            thingDef.equippedStatOffsets = this.originalData;
            InitOriginalData();
        }

        protected override void Apply()
        {
            if (thingDef == null)
                return;

            if(!modifierDic.NullOrEmpty() && thingDef.equippedStatOffsets == null)
                thingDef.equippedStatOffsets = new List<StatModifier>();

            thingDef.equippedStatOffsets?.Clear();
            foreach (var item in modifierDic)
            {
                StatModifier statModifier = new StatModifier()
                {
                    stat = DefDatabase<StatDef>.GetNamed(item.Key, false),
                    value = item.Value,
                };
                if (statModifier.stat != null)
                    thingDef.equippedStatOffsets.Add(statModifier);
            }

        }

        protected override void InitOriginalData()
        {
            if(this.thingDef == null)
                return;

            if(thingDef.equippedStatOffsets.NullOrEmpty())
            {
                this.originalData = null;
            }
            else
            {
                originalData = CopyUtility.CopyStats(thingDef.equippedStatOffsets);
            }

        }
    }
}
