using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class StatSaveable : SaveableBase
    {

        private string _statDefName;
        public StatDef StatDef
        {
            get => DefDatabase<StatDef>.GetNamed(_statDefName);
            set => _statDefName = value.defName;
        }

        public float value;

        public bool needDelete = false;
        private StatSaveable Original => base.originalData as StatSaveable;

        public StatSaveable() { }

        public StatSaveable(StatModifier statModifier, bool isOriginal = false)
        {
            if (statModifier == null)
            {
                return;
            }
            StatDef = statModifier.stat;
            value = statModifier.value;

            if (!isOriginal)
                originalData = new StatSaveable(statModifier, true);
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            var statModifier = thingDef.statBases.Find(x => x.stat == StatDef);
            if (statModifier == null)
            {
                if (needDelete)
                {
                    return;
                }
                statModifier = new StatModifier();
                statModifier.stat = StatDef;
                thingDef.statBases.Add(statModifier);
                if (Original != null)
                    Original.needDelete = true;
            }

            if (needDelete)
            {
                thingDef.statBases.Remove(statModifier);
                return;
            }
            else
            {
                statModifier.value = value;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref _statDefName, "statDefName");
            Scribe_Values.Look(ref value, "value");
            Scribe_Values.Look(ref needDelete, "needDelete");
        }

        public override void Reset(ThingDef thingDef)
        {
            if (thingDef == null || Original == null)
            {
                return;
            }
            this.value = Original.value;

            this.needDelete = Original.needDelete;

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            StatModifier statModifier = thingDef.statBases.First(x => x.stat == this.StatDef);
            if(statModifier != null)
            {
                this.originalData = new StatSaveable(statModifier, true);
            }
        }
    }
}
