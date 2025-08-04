using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class CompChargeSaveable : CompSaveableBase<CompProperties_Charges>
    {
        public override bool CompChanged
        {
            get
            {
                if (originalData == null || compProps == null)
                {
                    return false;
                }

                if (!originalData.chargeSpeeds.NullOrEmpty() && !compProps.chargeSpeeds.NullOrEmpty() && originalData.chargeSpeeds.Count == compProps.chargeSpeeds.Count)
                {
                    for (int i = 0; i < originalData.chargeSpeeds.Count; i++)
                    {
                        if (originalData.chargeSpeeds[i] != compProps.chargeSpeeds[i])
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

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

        //save
        private List<int> charges_save = new List<int>();

        public CompChargeSaveable() { }

        public CompChargeSaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //old save
            if(Scribe.mode == LoadSaveMode.LoadingVars && base.compIsNull)
            {
                Scribe_Collections.Look(ref this.charges_save, "chargeSpeeds", LookMode.Value);
                if(!this.charges_save.NullOrEmpty())
                {
                    base.compIsNull = false;
                }
            }

            if (!base.compIsNull)
            {
                Scribe_Collections.Look(ref this.charges_save, "chargeSpeeds", LookMode.Value);
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (this.charges_save == null)
                    {
                        this.charges_save = new List<int>();
                    }
                }
            }
        }

        protected override CompProperties_Charges MakeCopy(CompProperties_Charges original)
        {
            CompProperties_Charges newComp = new CompProperties_Charges();

            newComp.chargeSpeeds = new List<int>(compProps.chargeSpeeds);

            return newComp;
        }

        protected override void SaveData()
        {
            charges_save = new List<int>(compProps.chargeSpeeds);
        }

        protected override CompProperties_Charges ReadData()
        {
            CompProperties_Charges newComp = new CompProperties_Charges();

            newComp.chargeSpeeds = new List<int>(this.charges_save);

            return newComp;
        }

    }
}
