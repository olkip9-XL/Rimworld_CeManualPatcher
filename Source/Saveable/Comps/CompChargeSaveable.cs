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
        //save
        private List<int> charges_save;
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

        public CompChargeSaveable() { }

        public CompChargeSaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref this.charges_save, "chargeSpeeds", LookMode.Value);
        }

        //public override void Reset()
        //{
        //    if (compProps == null)
        //    {
        //        return;
        //    }

        //    if(this.charges_original == null)
        //    {
        //        def.comps.RemoveWhere(x => x is CompProperties_Charges);
        //        return;
        //    }

        //    compProps.chargeSpeeds = new List<int>(this.charges_original);
        //}

        //protected override void Apply()
        //{
        //    if (compProps == null)
        //    {
        //        return;
        //    }

        //    compProps.chargeSpeeds = new List<int>(this.charges_save);
        //}

        //protected override void InitOriginalData()
        //{
        //    if (compProps == null)
        //    {
        //        return;
        //    }

        //    this.charges_original = new List<int>(compProps.chargeSpeeds);
        //}

        //public override void PostLoadInit(ThingDef thingDef)
        //{
        //    this.def = thingDef;
        //    InitOriginalData();

        //    if (!thingDef.HasComp<CompCharges>())
        //    {
        //        thingDef.comps.Add(new CompProperties_Charges());
        //    }
        //    this.Apply();
        //}

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

        protected override CompProperties_Charges MakeCopy(CompProperties_Charges original)
        {
            CompProperties_Charges newComp = new CompProperties_Charges();

            newComp.chargeSpeeds = new List<int>(compProps.chargeSpeeds);

            return newComp;
        }
    }
}
