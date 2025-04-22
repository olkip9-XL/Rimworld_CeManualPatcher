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
    internal class StatSaveable : SaveableBase
    {
        private Dictionary<string, float> modifierDic = new Dictionary<string, float>();

        //private
        private List<StatModifier> statBases
        {
            get
            {
                if (thingDef == null)
                {
                    return null;
                }
                return thingDef.statBases;
            }
        }

        private List<StatModifier> originalData = new List<StatModifier>();
        public List<StatModifier> OriginalStats => originalData;

        public StatSaveable() { }
        public StatSaveable(ThingDef thingDef) : base(thingDef)
        {
            //this.thingDef = thingDef;
            //if (statBases.NullOrEmpty())
            //{
            //    return;
            //}

            //InitOriginalData();
        }

        protected override bool NullCheck()
        {
           return statBases.NullOrEmpty();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving &&
                this.thingDef != null)
            {
                foreach (var statModifier in this.thingDef.statBases)
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
            if (thingDef == null)
                return;

            thingDef.statBases = this.originalData;
            InitOriginalData();
        }

        protected override void Apply()
        {
            if (thingDef == null)
                return;
            thingDef.statBases.Clear();
            foreach (var item in modifierDic)
            {
                StatModifier statModifier = new StatModifier()
                {
                    stat = DefDatabase<StatDef>.GetNamed(item.Key, false),
                    value = item.Value,
                };
                if (statModifier.stat != null)
                    thingDef.statBases.Add(statModifier);
            }

        }

        //public override void PostLoadInit(ThingDef thingDef)
        //{
        //    this.thingDef = thingDef;
        //    if (statBases.NullOrEmpty())
        //    {
        //        return;
        //    }

        //    InitOriginalData();
        //    this.Apply();
        //}

        protected override void InitOriginalData()
        {
            this.originalData = new List<StatModifier>();
            foreach (var statModifier in statBases)
            {
                this.originalData.Add(new StatModifier()
                {
                    stat = statModifier.stat,
                    value = statModifier.value,
                });
            }
        }


    }
}
