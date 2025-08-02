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
    class SecondaryDamageExpo : IExposable
    {
        public string defName;
        public int damageAmount;

        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref damageAmount, "damageAmount");
        }
    }
    internal class SecondaryDamageSaveable : SaveableBase<ThingDef>
    {
        private List<SecondaryDamageExpo> secondaryDamagesExpo = new List<SecondaryDamageExpo>();


        //private
        private List<SecondaryDamage> originalData = new List<SecondaryDamage>();

        private List<SecondaryDamage> secondaryDamages
        {
            get
            {
                if (def == null || def.projectile == null || !(def.projectile is ProjectilePropertiesCE))
                {
                    return null;
                }

                return (def.projectile as ProjectilePropertiesCE).secondaryDamage;
            }
        }

        public SecondaryDamageSaveable() : base()
        {
        }

        public SecondaryDamageSaveable(ThingDef thingDef)
        {
            this.def = thingDef;
            if (secondaryDamages.NullOrEmpty())
            {
                return;
            }

            InitOriginalData();
        }
        protected override void Apply()
        {
            if (secondaryDamages == null)
            {
                return;
            }

            secondaryDamages.Clear();
            foreach (var item in secondaryDamagesExpo)
            {
                DamageDef damageDef = DefDatabase<DamageDef>.GetNamed(item.defName);
                if (damageDef != null)
                {
                    secondaryDamages.Add(new SecondaryDamage()
                    {
                        def = damageDef,
                        amount = item.damageAmount
                    });
                }
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && secondaryDamages != null)
            {
                secondaryDamagesExpo.Clear();
                foreach (var damage in secondaryDamages)
                {
                    secondaryDamagesExpo.Add(new SecondaryDamageExpo
                    {
                        defName = damage.def.defName,
                        damageAmount = damage.amount
                    });
                }
            }

            Scribe_Collections.Look(ref secondaryDamagesExpo, "secondaryDamagesExpo", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (secondaryDamagesExpo == null)
                {
                    secondaryDamagesExpo = new List<SecondaryDamageExpo>();
                }
            }
        }

        public override void Reset()
        {
            if (secondaryDamages == null)
            {
                return;
            }

            ProjectilePropertiesCE projectile = def.projectile as ProjectilePropertiesCE;

            //projectile.secondaryDamage = this.originalData;
            projectile.secondaryDamage.Clear();
            projectile.secondaryDamage.AddRange(originalData);

            InitOriginalData();
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            if (secondaryDamages == null)
            {
                return;
            }
            InitOriginalData();
            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (secondaryDamages == null)
            {
                return;
            }
            originalData.Clear();
            originalData = new List<SecondaryDamage>();
            foreach (var item in secondaryDamages)
            {
                SecondaryDamage temp = new SecondaryDamage();

                PropUtility.CopyPropValue(item, temp);

                originalData.Add(temp);
            }
        }

        public bool CompareOriginalData()
        {
            List<SecondaryDamage> currentData = (this.def.projectile as ProjectilePropertiesCE)?.secondaryDamage;

            if ((currentData?.Count ?? -1) != (originalData?.Count ?? -1))
            {
                return false;
            }

            if (currentData == null || originalData == null)
            {
                return true;
            }

            currentData.SortBy(x => x.def.defName);
            originalData.SortBy(x => x.def.defName);

            for (int i = 0; i < currentData.Count; i++)
            {
                if (currentData[i].def != originalData[i].def ||
                    currentData[i].amount != originalData[i].amount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
