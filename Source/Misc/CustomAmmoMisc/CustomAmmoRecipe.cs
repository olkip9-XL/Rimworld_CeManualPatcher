using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Misc.CustomAmmoMisc
{
    internal class CustomAmmoRecipe : IExposable
    {
        public AmmoProjectilePair parentPair;

        //字段
        public string parentRecipeClass = "AmmoRecipeBase";
        public string label;
        public string description;
        public string jobString;
        public int workAmount = 2000;
        public int productAmount = 500;

        public List<MP_ThingDefCountClass_Save> ingredients = new List<MP_ThingDefCountClass_Save>();

        public CustomAmmoRecipe() { }
        public CustomAmmoRecipe(AmmoProjectilePair _parentPair)
        {
            parentPair = _parentPair;

            try
            {
                label = "MP_DefaultAmmoRecipeLabel".Translate(parentPair.ammo.label, productAmount.ToString());
                description = "MP_DefaultAmmoRecipeDescription".Translate(productAmount.ToString(), parentPair.ammo.label);
                jobString = "MP_DefaultAmmoRecipeJobString".Translate(parentPair.ammo.label);
            }
            catch
            {
                //load new 
                label = "default recipe label";
                description = "default recipe description";
                jobString = "default recipe job string";
            }
            
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref parentRecipeClass, "parentClass", "AmmoRecipeBase");

            Scribe_Values.Look(ref label, "label", "default recipe label");
            Scribe_Values.Look(ref description, "description", "default recipe description");
            Scribe_Values.Look(ref jobString, "jobString", "default recipe job string");

            Scribe_Values.Look(ref workAmount, "workAmount", 2000);
            Scribe_Values.Look(ref productAmount, "productAmount", 500);

            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ingredients == null)
                {
                    ingredients = new List<MP_ThingDefCountClass_Save>();
                }
            }
        }

        public void Export(XmlDocument doc, XmlElement root)
        {
            XmlElement recipeElement = doc.CreateElement("RecipeDef");
            recipeElement.SetAttribute("ParentName", parentRecipeClass);
            root.AppendChild(recipeElement);

            XmlUtility.AddChildElement(doc, recipeElement, "defName", $"Make{parentPair.ammo.DefName}");
            XmlUtility.AddChildElement(doc, recipeElement, "label", label);
            XmlUtility.AddChildElement(doc, recipeElement, "description", description);
            XmlUtility.AddChildElement(doc, recipeElement, "jobString", jobString);

            //add ingredients
            XmlElement ingredientsElement = doc.CreateElement("ingredients");
            recipeElement.AppendChild(ingredientsElement);
            foreach (var item in ingredients)
            {
                if (item == null)
                    continue;

                XmlElement liElement = doc.CreateElement("li");
                ingredientsElement.AppendChild(liElement);

                //thing
                XmlElement filterElement = doc.CreateElement("filter");
                liElement.AppendChild(filterElement);

                XmlElement thingDefsElement = doc.CreateElement("thingDefs");
                filterElement.AppendChild(thingDefsElement);

                XmlUtility.AddChildElement(doc, thingDefsElement, "li", item.thingDef.defName);

                //count
                XmlUtility.AddChildElement(doc, liElement, "count", item.count.ToString());
            }

            XmlElement fixedIndustriesElement = doc.CreateElement("fixedIngredientFilter");
            recipeElement.AppendChild(fixedIndustriesElement);

            XmlUtility.AddChildElementList(doc, fixedIndustriesElement, "thingDefs", ingredients.Select(x=>x.thingDef.defName).ToList());

            //products
            XmlElement productElement = doc.CreateElement("products");
            recipeElement.AppendChild(productElement);
            XmlUtility.AddChildElement(doc, productElement, parentPair.ammo.DefName, productAmount.ToString());

            //work amount
            XmlUtility.AddChildElement(doc, recipeElement, "workAmount", workAmount.ToString());
        }

    }
}
