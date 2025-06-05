using CeManualPatcher.Misc.CustomAmmoMisc;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    internal class AmmoRecipeDefSaveable : SaveableBase<RecipeDef>
    {

        //save data
        private int productCount_save;
        private int workAmount_save;
        private List<MP_ThingDefCountClass_Save> ingredients_save;

        //originalData
        private int productCount;
        private int workAmount;
        private List<ThingDefCountClass> ingredients;

        public int OriginalProductCount => this.productCount;
        public int OriginalWorkAmount => this.workAmount;
        public List<ThingDefCountClass> OriginalIngredients => this.ingredients;


        private RecipeDef recipe => base.def as RecipeDef;

        public AmmoRecipeDefSaveable() { }
        public AmmoRecipeDefSaveable(RecipeDef def) : base(def)
        {
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && recipe != null)
            {
                this.productCount_save = recipe.products.FirstOrDefault()?.count ?? 0;

                this.workAmount_save = (int)recipe.workAmount;

                this.ingredients_save = new List<MP_ThingDefCountClass_Save>();
                foreach (var item in recipe.ingredients)
                {
                    ThingDef thingDef = item.filter.AllowedThingDefs.FirstOrDefault();
                    int count = (int)item.GetBaseCount();
                    this.ingredients_save.Add(new MP_ThingDefCountClass_Save(thingDef, count));
                }
            }

            Scribe_Values.Look(ref productCount_save, "productCount");
            Scribe_Values.Look(ref workAmount_save, "workAmount");
            Scribe_Collections.Look(ref ingredients_save, "ingerents", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ingredients_save == null)
                {
                    ingredients_save = new List<MP_ThingDefCountClass_Save>();
                }
            }

        }

        public override void Reset()
        {
            if (recipe == null)
                return;

            recipe.workAmount = this.workAmount;

            recipe.products.FirstOrDefault().count = this.productCount;

            recipe.ingredients.Clear();
            recipe.fixedIngredientFilter.SetDisallowAll();
            foreach (var item in this.ingredients)
            {
                ThingFilter filter = new ThingFilter();
                filter.SetAllow(item.thingDef, true);

                IngredientCount ingredientCount = new IngredientCount();
                ingredientCount.filter = filter;
                ingredientCount.SetBaseCount(item.count);

                recipe.ingredients.Add(ingredientCount);
                recipe.fixedIngredientFilter.SetAllow(item.thingDef, true);
            }
        }

        protected override void Apply()
        {
            if (recipe == null)
                return;

            recipe.workAmount = this.workAmount_save;

            recipe.products.FirstOrDefault().count = this.productCount_save;

            recipe.ingredients.Clear();
            recipe.fixedIngredientFilter.SetDisallowAll();
            foreach (var item in this.ingredients_save)
            {
                ThingFilter filter = new ThingFilter();
                filter.SetAllow(item.thingDef, true);

                IngredientCount ingredientCount = new IngredientCount();
                ingredientCount.filter = filter;
                ingredientCount.SetBaseCount(item.count);

                recipe.ingredients.Add(ingredientCount);

                recipe.fixedIngredientFilter.SetAllow(item.thingDef, true);
            }

        }

        protected override void InitOriginalData()
        {
            if (recipe == null)
                return;

            this.workAmount = (int)recipe.workAmount;

            this.productCount = recipe.products.FirstOrDefault()?.count ?? 0;

            this.ingredients = new List<ThingDefCountClass>();
            foreach (var item in recipe.ingredients)
            {
                ThingDef thingDef = item.filter.AllowedThingDefs.FirstOrDefault();
                int count = (int)item.GetBaseCount();

                this.ingredients.Add(new ThingDefCountClass(thingDef, count));
            }
        }

        public bool CompareOriginalIngredients()
        {
            List<IngredientCount> ingredientsA = this.recipe?.ingredients ?? null;
            List<ThingDefCountClass> ingredientsB = this.ingredients;

            Dictionary<ThingDef, int> thingCountDicA = new Dictionary<ThingDef, int>();
            Dictionary<ThingDef, int> thingCountDicB = new Dictionary<ThingDef, int>();

            if (ingredientsA == null || ingredientsB == null)
            {
                return false;
            }

            //collect thing counts
            foreach (var ingredient in ingredientsA)
            {
                foreach (var thingDef in ingredient.filter.AllowedThingDefs)
                {
                    if (!thingCountDicA.ContainsKey(thingDef))
                    {
                        thingCountDicA.Add(thingDef, (int)ingredient.GetBaseCount());
                    }
                }
            }

            foreach (var item in ingredientsB)
            {
                if (!thingCountDicB.ContainsKey(item.thingDef))
                {
                    thingCountDicB.Add(item.thingDef, item.count);
                }
            }

            //compare
            if (thingCountDicA.Count != thingCountDicB.Count)
            {
                return false;
            }

            foreach (var kvp in thingCountDicA)
            {
                if (!thingCountDicB.TryGetValue(kvp.Key, out int countB) || countB != kvp.Value)
                {
                    return false;
                }
            }

            return true;



        }

        public bool CompareOriginalFixedIngredientFilter()
        {
            IEnumerable<ThingDef> allowedDefsA = this.recipe?.fixedIngredientFilter?.AllowedThingDefs ?? Enumerable.Empty<ThingDef>();
            IEnumerable<ThingDef> allowedDefsB = this.ingredients?.Select(x => x.thingDef) ?? Enumerable.Empty<ThingDef>();

            return allowedDefsA.OrderBy(x => x.defName).SequenceEqual(allowedDefsB.OrderBy(x => x.defName));
        }
    }
}
