using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
 

    internal class SecondaryDamageSaveable : SaveableBase
    {
        private List<SecondaryDamageExpo> secondaryDamagesExpo = new List<SecondaryDamageExpo>();
        public List<SecondaryDamage> secondaryDamages = new List<SecondaryDamage>();

        private SecondaryDamageSaveable Original => base.originalData as SecondaryDamageSaveable;
        public SecondaryDamageSaveable() : base()
        {
        }

        public SecondaryDamageSaveable(ThingDef thingDef, bool isOriginal = false) : base(thingDef, isOriginal)
        {
            if (IsNull(thingDef))
            {
                return;
            }
            ProjectilePropertiesCE projectilePropertiesCE = thingDef.projectile as ProjectilePropertiesCE;
            
            this.secondaryDamages.Clear();
            this.secondaryDamages.AddRange(projectilePropertiesCE.secondaryDamage);

            if (!isOriginal)
            {
                originalData = new SecondaryDamageSaveable(thingDef, true);
            }
        }

        private bool IsNull(ThingDef thingDef)
        {
            return thingDef?.projectile == null || !(thingDef?.projectile is ProjectilePropertiesCE);
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null || thingDef.projectile == null || !(thingDef.projectile is ProjectilePropertiesCE))
            {
                return;
            }
            ProjectilePropertiesCE projectilePropertiesCE = thingDef.projectile as ProjectilePropertiesCE;
            projectilePropertiesCE.secondaryDamage.Clear();
            projectilePropertiesCE.secondaryDamage.AddRange(this.secondaryDamages);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
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

            Scribe_Collections.Look(ref secondaryDamagesExpo, "secondaryDamagesExpo", LookMode.Value);
        }

        public override void Reset(ThingDef thingDef)
        {
            if (Original == null)
            {
                return;
            }

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            if(this.secondaryDamagesExpo != null)
            {
                this.secondaryDamages.Clear();
                foreach (var damage in this.secondaryDamagesExpo)
                {
                    DamageDef damageDef = DefDatabase<DamageDef>.GetNamed(damage.defName);
                    if (damageDef != null)
                    {
                        this.secondaryDamages.Add(new SecondaryDamage()
                        {
                            def = damageDef,
                            amount = damage.damageAmount
                        });
                    }
                }
            }

            this.originalData = new SecondaryDamageSaveable(thingDef, true);
        }
    }
}
