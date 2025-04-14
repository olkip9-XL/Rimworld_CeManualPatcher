using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
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
    internal class SecondaryDamageSaveable : SaveableBase
    {
        private List<SecondaryDamageExpo> secondaryDamagesExpo = new List<SecondaryDamageExpo>();


        //private
        private List<SecondaryDamage> originalData = new List<SecondaryDamage>();

        private List<SecondaryDamage> secondaryDamages
        {
            get
            {
                if (thingDef == null || thingDef.projectile == null || !(thingDef.projectile is ProjectilePropertiesCE))
                {
                    return null;
                }

                return (thingDef.projectile as ProjectilePropertiesCE).secondaryDamage;
            }
        }

        public SecondaryDamageSaveable() : base()
        {
        }

        public SecondaryDamageSaveable(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (secondaryDamages.NullOrEmpty())
            {
                return;
            }

            InitOriginalData();
        }
        protected override void Apply()
        {
            if(secondaryDamages == null)
            {
                return;
            }

            secondaryDamages.Clear();
            foreach(var item in secondaryDamagesExpo)
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
            if (Scribe.mode == LoadSaveMode.Saving)
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
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (secondaryDamagesExpo == null)
                {
                    secondaryDamagesExpo = new List<SecondaryDamageExpo>();
                }
            }
        }

        public override void Reset()
        {
            if(secondaryDamages == null)
            {
                return;
            }

            ProjectilePropertiesCE projectile = thingDef.projectile as ProjectilePropertiesCE;

            //projectile.secondaryDamage = this.originalData;
            projectile.secondaryDamage.Clear();
            projectile.secondaryDamage.AddRange(originalData);

            InitOriginalData();
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
           this.thingDef = thingDef;
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
    }
}
