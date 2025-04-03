using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using System.Reflection;

using CeManualPatcher.Saveable.Ammo;

namespace CeManualPatcher.Saveable
{
    struct SecondaryDamageExpo
    {
        public string defName;
        public int damageAmount;
    }

    struct ThingDefCountClassExpo
    {
        public string defName;
        public int count;
    }
    internal class ProjectileDefSaveable : SaveableBase
    {
        //字段
        private string damageDefString;
        public DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value.defName;
        }

        public int damageAmountBase;
        public float armorPenetrationSharp;
        public float armorPenetrationBlunt;
        public float explosionRadius;
        public float suppressionFactor;
        public GasType? postExplosionGasType;
        public SecondaryExplosionSaveable secondaryExplosion;

        private List<SecondaryDamageExpo> secondaryDamagesExpo = new List<SecondaryDamageExpo>();
        public List<SecondaryDamage> secondaryDamages = new List<SecondaryDamage>();

        private List<ThingDefCountClassExpo> fragmentsExpo = new List<ThingDefCountClassExpo>();
        public List<ThingDefCountClass> fragments = new List<ThingDefCountClass>();

        //public SecondaryDamageSaveable secondaryDamage;

        //public ThingDefCountClassSaveable fragments;
        private ProjectileDefSaveable Original => originalData as ProjectileDefSaveable;

        public ProjectileDefSaveable()
        {
        }

        public ProjectileDefSaveable(ThingDef thingDef, bool isOriginal = false)
        {
            if(thingDef == null || 
                thingDef.projectile == null ||
                !(thingDef.projectile is ProjectilePropertiesCE))
            {
                return;
            }

            ProjectilePropertiesCE projectilePropertiesCE = thingDef.projectile as ProjectilePropertiesCE;

            this.damageDef = projectilePropertiesCE.damageDef;
            this.damageAmountBase  = typeof(ProjectileProperties).GetField("damageAmountBase" , BindingFlags.NonPublic | BindingFlags.Instance).GetValue(projectilePropertiesCE) as int? ?? 0;
            this.armorPenetrationSharp = projectilePropertiesCE.armorPenetrationSharp;
            this.armorPenetrationBlunt = projectilePropertiesCE.armorPenetrationBlunt;
            this.explosionRadius = projectilePropertiesCE.explosionRadius;
            this.suppressionFactor = projectilePropertiesCE.suppressionFactor;
            this.postExplosionGasType = projectilePropertiesCE.postExplosionGasType;

            this.secondaryDamages.Clear();
            this.secondaryDamages.AddRange(projectilePropertiesCE.secondaryDamage);

            if (thingDef.HasComp<CompFragments>())
            {
                this.fragments.Clear();
                this.fragments.AddRange(thingDef.GetCompProperties<CompProperties_Fragments>().fragments);
            }

            if(thingDef.HasComp<CompExplosiveCE>())
            {
                this.secondaryExplosion = new SecondaryExplosionSaveable(thingDef);
            }

            //this.secondaryDamage = new SecondaryDamageSaveable(thingDef);
            //this.fragments = new ThingDefCountClassSaveable(thingDef);

            if(!isOriginal)
            {
                this.originalData = new ProjectileDefSaveable(thingDef, true);
            }
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null ||
              thingDef.projectile == null ||
              !(thingDef.projectile is ProjectilePropertiesCE))
            {
                return;
            }

            ProjectilePropertiesCE projectilePropertiesCE = thingDef.projectile as ProjectilePropertiesCE;

            projectilePropertiesCE.damageDef = this.damageDef;
            typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(projectilePropertiesCE, this.damageAmountBase);
            projectilePropertiesCE.armorPenetrationSharp = this.armorPenetrationSharp;
            projectilePropertiesCE.armorPenetrationBlunt = this.armorPenetrationBlunt;
            projectilePropertiesCE.explosionRadius = this.explosionRadius;
            projectilePropertiesCE.suppressionFactor = this.suppressionFactor;
            projectilePropertiesCE.postExplosionGasType = this.postExplosionGasType;

            projectilePropertiesCE.secondaryDamage.Clear();
            projectilePropertiesCE.secondaryDamage.AddRange(this.secondaryDamages);

            if(thingDef.HasComp<CompFragments>())
            {
                CompProperties_Fragments compProps = thingDef.GetCompProperties<CompProperties_Fragments>();
                compProps.fragments.Clear();
                compProps.fragments.AddRange(this.fragments);
            }

            if(thingDef.HasComp<CompExplosiveCE>())
            {
                this.secondaryExplosion?.Apply(thingDef);
            }

            //this.secondaryDamage?.Apply(thingDef);
            //this.fragments?.Apply(thingDef);
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

                fragmentsExpo.Clear();
                foreach (var fragment in fragments)
                {
                    fragmentsExpo.Add(new ThingDefCountClassExpo
                    {
                        defName = fragment.thingDef.defName,
                        count = fragment.count
                    });
                }
            }

            Scribe_Values.Look(ref damageDefString, "damageDefString");
            Scribe_Values.Look(ref damageAmountBase, "damageAmountBase");
            Scribe_Values.Look(ref armorPenetrationSharp, "armorPenetrationSharp");
            Scribe_Values.Look(ref armorPenetrationBlunt, "armorPenetrationBlunt");
            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
            Scribe_Values.Look(ref suppressionFactor, "suppressionFactor");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");

            Scribe_Collections.Look(ref secondaryDamagesExpo, "secondaryDamages", LookMode.Value);
            if(Scribe.mode == LoadSaveMode.LoadingVars && secondaryDamagesExpo == null)
            {
                secondaryDamagesExpo = new List<SecondaryDamageExpo>();
            }

            Scribe_Collections.Look(ref fragmentsExpo, "fragments", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars && fragmentsExpo == null)
            {
                fragmentsExpo = new List<ThingDefCountClassExpo>();
            }

            Scribe_Deep.Look(ref secondaryExplosion, "secondaryExplosion");
            Scribe_Deep.Look(ref fragments, "fragments");
        }

        public override void Reset(ThingDef thingDef)
        {
            if (Original == null)
            {
                return;
            }

            this.secondaryExplosion?.Reset(thingDef);

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            if(!secondaryDamagesExpo.NullOrEmpty())
            {
                secondaryDamages.Clear();
                foreach (var damage in secondaryDamagesExpo)
                {
                    DamageDef damageDef = DefDatabase<DamageDef>.GetNamed(damage.defName, false);
                    if (damageDef != null)
                    {
                        secondaryDamages.Add(new SecondaryDamage()
                        {
                            def = damageDef,
                            amount = damage.damageAmount
                        });
                    }
                }
            }

            if(!fragmentsExpo.NullOrEmpty())
            {
                fragments.Clear();
                foreach (var fragment in fragmentsExpo)
                {
                    ThingDef thing = DefDatabase<ThingDef>.GetNamed(fragment.defName, false);
                    if (thing != null)
                    {
                        fragments.Add(new ThingDefCountClass()
                        {
                            thingDef = thing,
                            count = fragment.count
                        });
                    }
                }
            }

            this.originalData = new ProjectileDefSaveable(thingDef, true);
        }
    }
}
