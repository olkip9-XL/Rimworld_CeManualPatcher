using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    internal class SecondaryExplosionSaveable : SaveableBase
    {
        private string damageDefString;
        public DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value.defName;
        }

        public float explosionRadius;
        public int damageAmountBase;
        public GasType? postExplosionGasType;

        private SecondaryExplosionSaveable Original => base.originalData as SecondaryExplosionSaveable;

        public SecondaryExplosionSaveable(ThingDef thingDef, bool isOriginal = false)
        {
            if(thingDef == null || !thingDef.HasComp<CompExplosiveCE>())
            {
                return;
            }

            CompProperties_ExplosiveCE compProps = thingDef.GetCompProperties<CompProperties_ExplosiveCE>();

            this.damageDef = compProps.explosiveDamageType;
            this.explosionRadius = compProps.explosiveRadius;
            this.damageAmountBase = (int)compProps.damageAmountBase;
            this.postExplosionGasType = compProps.postExplosionGasType;

            if (isOriginal)
            {
                this.originalData = new SecondaryExplosionSaveable(thingDef, true);
            }
        }    
        public override void Apply(ThingDef thingDef)
        {
            if(thingDef == null || !thingDef.HasComp<CompExplosiveCE>())
            {
                return;
            }

            CompProperties_ExplosiveCE compProps = thingDef.GetCompProperties<CompProperties_ExplosiveCE>();

            compProps.explosiveDamageType = this.damageDef;
            compProps.explosiveRadius = this.explosionRadius;
            compProps.damageAmountBase = this.damageAmountBase;
            compProps.postExplosionGasType = this.postExplosionGasType;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref damageDefString, "damageDef");
            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
            Scribe_Values.Look(ref damageAmountBase, "damageAmountBase");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");
        }

        public override void Reset(ThingDef thingDef)
        {
            if (thingDef == null || !thingDef.HasComp<CompExplosiveCE>() || Original == null)
            {
                return;
            }

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {

            this.originalData = new SecondaryExplosionSaveable(thingDef, true);
        }
    }
}
