using CeManualPatcher.Saveable;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace CeManualPatcher.Patch
{
    internal class AmmoPatch : PatchBase
    {
        public string thingDefString;
        public ThingDef projectileDef
        {
            get => DefDatabase<ThingDef>.GetNamed(thingDefString, false);
            set => thingDefString = value.defName;
        }

        public ProjectileDefSaveable projectile;

        public AmmoPatch()
        {
        }
        public AmmoPatch(ThingDef projectileDef)
        {
            if(projectileDef == null || 
                projectileDef.projectile == null ||
                !(projectileDef.projectile is ProjectilePropertiesCE))
            {
                return;
            }

            this.projectileDef = projectileDef;
            this.projectile = new ProjectileDefSaveable(projectileDef);
        }

        public override void Apply()
        {
            projectile?.Apply(projectileDef);
        }

        public override void Reset()
        {
            projectile?.Reset(projectileDef);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref thingDefString, "thingDefString");
            Scribe_Deep.Look(ref projectile, "projectile");
        }

        public override void PostLoadInit()
        {
            if (projectileDef != null)
            {
                projectile.PostLoadInit(projectileDef);
            }
        }
    }
}
