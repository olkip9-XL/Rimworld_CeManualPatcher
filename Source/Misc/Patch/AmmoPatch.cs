using CeManualPatcher.Misc.Patch;
using CeManualPatcher.Saveable;
using CeManualPatcher.Saveable.Ammo;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace CeManualPatcher.Patch
{
    internal class AmmoPatch : PatchBase<ThingDef>
    {
        //private string thingDefString = "";
        //public ThingDef projectileDef
        //{
        //    get => DefDatabase<ThingDef>.GetNamed(thingDefString, false);
        //    set => thingDefString = value?.defName ?? "null";
        //}
        //old

        private string ammoDefString = "";
        public ThingDef ammoDef
        {
            get => DefDatabase<ThingDef>.GetNamed(ammoDefString ?? "null", false);
            set => ammoDefString = value?.defName ?? "null";
        }

        private ProjectileDefSaveable projectile;
        private AmmoDefSaveable ammo;

        public string sourceModName;
        public AmmoPatch() { }
        public AmmoPatch(ThingDef projectileDef, ThingDef ammoDef)
        {
            if (projectileDef == null ||
                projectileDef.projectile == null ||
                !(projectileDef.projectile is ProjectilePropertiesCE))
            {
                return;
            }

            if (projectileDef != null)
            {
                this.targetDef = projectileDef;
                this.projectile = new ProjectileDefSaveable(projectileDef);
            }

            if (ammoDef != null)
            {
                this.ammoDef = ammoDef;
                this.ammo = new AmmoDefSaveable(ammoDef);
            }

            this.sourceModName = projectileDef.modContentPack?.Name ?? "Unknown";
        }

        public override void Reset()
        {
            projectile?.Reset();
            ammo?.Reset();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Values.Look(ref thingDefString, "thingDefString");
            Scribe_Deep.Look(ref projectile, "projectile");

            Scribe_Values.Look(ref ammoDefString, "ammoDefString");
            Scribe_Deep.Look(ref ammo, "ammo");

            Scribe_Values.Look(ref sourceModName, "sourceModName");

            //old save
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string oldDefString = "null";
                Scribe_Values.Look(ref oldDefString, "thingDefString");
                if (!oldDefString.NullOrEmpty())
                {
                    Log.Warning($"thingDefString is {oldDefString}");
                    targetDefString = oldDefString;
                }
            }

        }

        public override void PostLoadInit()
        {
            if (targetDef != null)
            {
                projectile?.PostLoadInit(targetDef);
            }

            if (ammoDef != null)
            {
                ammo?.PostLoadInit(ammoDef);
            }
        }

        public override void ExportPatch(string dirPath)
        {
            throw new NotImplementedException();
        }
    }
}
