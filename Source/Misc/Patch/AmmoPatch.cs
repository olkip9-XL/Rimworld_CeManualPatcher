using CeManualPatcher.Misc;
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
        private string ammoDefString = "";
        public ThingDef ammoDef
        {
            get => DefDatabase<ThingDef>.GetNamed(ammoDefString ?? "null", false);
            set => ammoDefString = value?.defName ?? "null";
        }

        private RecipeDef recipeDefInt = null;
        public RecipeDef recipeDef
        {
            get
            {

                if (recipeDefInt == null && ammoDef != null)
                {
                    RecipeDef _recipe = DefDatabase<RecipeDef>.GetNamed("Make" + ammoDefString);
                    if (_recipe != null)
                    {
                        recipeDefInt = _recipe;
                        return recipeDefInt;
                    }

                    _recipe = DefDatabase<RecipeDef>.AllDefsListForReading
                        .FirstOrDefault(x => x.products.Any(y => y.thingDef == ammoDef));
                    if (_recipe != null)
                    {
                        recipeDefInt = _recipe;
                        return recipeDefInt;
                    }

                    MP_Log.Error("Can't Find Recipe for ammo ", def: ammoDef);
                }
                return recipeDefInt;
            }
        }

        private ProjectileDefSaveable projectile;
        private AmmoDefSaveable ammo;
        private AmmoRecipeDefSaveable recipe;

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

            if(recipeDef != null)
            {
                this.recipe = new AmmoRecipeDefSaveable(recipeDef);
            }

            this.sourceModName = projectileDef.modContentPack?.Name ?? "Unknown";
        }

        public override void Reset()
        {
            projectile?.Reset();
            ammo?.Reset();
            recipe?.Reset();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Values.Look(ref thingDefString, "thingDefString");
            Scribe_Deep.Look(ref projectile, "projectile");

            Scribe_Values.Look(ref ammoDefString, "ammoDefString");
            Scribe_Deep.Look(ref ammo, "ammo");
            Scribe_Deep.Look(ref recipe, "recipe");

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

            if (recipeDef != null)
            {
                if(recipe == null)
                {
                    recipe = new AmmoRecipeDefSaveable(recipeDef);
                }
                else
                {
                    recipe?.PostLoadInit(recipeDef);
                }
            }
        }

        //skip
        public override void ExportPatch(string dirPath)
        {
            throw new NotImplementedException();
        }
    }
}
