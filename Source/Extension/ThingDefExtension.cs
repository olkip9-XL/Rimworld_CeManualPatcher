using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Extension
{
    public static class ThingDefExtension
    {
        public static List<DamageDef> ExistDamages(this ThingDef thingDef)
        {
            List<DamageDef> list = new List<DamageDef>();

            ProjectilePropertiesCE props = thingDef.projectile as ProjectilePropertiesCE;
            if (props == null)
            {
                return null;
            }

            list.Add(props.damageDef);

            foreach (var item in props.secondaryDamage)
            {
                list.Add(item.def);
            }

            if (thingDef.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE compProps = thingDef.GetCompProperties<CompProperties_ExplosiveCE>();
                list.Add(compProps.explosiveDamageType);
            }

            return list;
        }
    }
}
