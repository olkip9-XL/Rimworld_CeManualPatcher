using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal abstract class CompSaveableBase<TComp> : SaveableBase<ThingDef> where TComp : CompProperties
    {

        protected TComp originalData;

        public bool compIsNull = true;

        protected abstract TComp compProps { get; }

        public abstract bool CompChanged { get; }
        public CompSaveableBase() { }

        public CompSaveableBase(ThingDef thingDef) : base(thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (def != null && Scribe.mode == LoadSaveMode.Saving && def.GetCompProperties<TComp>() != null)
            {
                compIsNull = false;
                SaveData();
            }

            Scribe_Values.Look(ref compIsNull, "compIsNull", true);
        }
        public override void Reset()
        {
            if (def == null)
            {
                return;
            }

            def.comps.RemoveAll(x => x is TComp);
            if (originalData != null)
            {
                def.comps.Add(originalData);
            }
        }

        protected override void Apply()
        {
            if (def == null || this.compIsNull)
            {
                return;
            }

            def.comps.RemoveAll(x => x is TComp);

            TComp newComp = ReadData();
            if(newComp != null)
            {
                def.comps.Add(newComp);
            }
        }

        protected override void InitOriginalData()
        {
            if (def == null)
            {
                return;
            }

            if (compProps == null)
            {
                originalData = null;
            }
            else
            {
                originalData = MakeCopy(compProps);
            }
        }

        protected abstract void SaveData();
        protected abstract TComp ReadData();
        protected abstract TComp MakeCopy(TComp original);
    }
}
