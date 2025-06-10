using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Comps
{
    internal class CompChargeSaveable : CompSaveableBase<CompProperties_Charges>
    {
        //save
        private List<int> charges_save;

        //original
        private List<int> charges_original;

        protected override CompProperties_Charges compProps
        {
            get
            {
                if (base.def == null || base.def.comps == null)
                {
                    return null;
                }

                return base.def.GetCompProperties<CompProperties_Charges>();
            }
        }

        public CompChargeSaveable() { }

        public CompChargeSaveable(ThingDef thingDef, bool forceAdd = false)
        {
            this.def = thingDef;
            if (compProps == null && !forceAdd)
            {
                return;
            }

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.charges_save = new List<int>(compProps.chargeSpeeds);
            }

            Scribe_Collections.Look(ref this.charges_save, "chargeSpeeds", LookMode.Value);
        }

        public override void Reset()
        {
            if (compProps == null)
            {
                return;
            }

            if(this.charges_original == null)
            {
                def.comps.RemoveWhere(x => x is CompProperties_Charges);
                return;
            }

            compProps.chargeSpeeds = new List<int>(this.charges_original);
        }

        protected override void Apply()
        {
            if (compProps == null)
            {
                return;
            }

            compProps.chargeSpeeds = new List<int>(this.charges_save);
        }

        protected override void InitOriginalData()
        {
            if (compProps == null)
            {
                return;
            }

            this.charges_original = new List<int>(compProps.chargeSpeeds);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            InitOriginalData();

            if (!thingDef.HasComp<CompCharges>())
            {
                thingDef.comps.Add(new CompProperties_Charges());
            }
            this.Apply();
        }

    }
}
