using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        public override string PatchName => "Ammo";

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

            if (recipeDef != null)
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
                if (recipe == null)
                {
                    recipe = new AmmoRecipeDefSaveable(recipeDef);
                }
                else
                {
                    recipe?.PostLoadInit(recipeDef);
                }
            }
        }

        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            //replace projectile
            if (targetDef != null)
            {
                ReplaceProjectile();
            }
            if (ammoDef != null)
            {
                ReplaceAmmo();
            }
            if (recipeDef != null)
            {
                ReplaceRecipe();
            }

            void ReplaceProjectile()
            {
                //add empty comps
                root.AppendChild(XmlUtility.AddIfNotExist(xmlDoc, targetDef.defName, "comps"));

                ProjectilePropertiesCE props = targetDef.projectile as ProjectilePropertiesCE;

                if (props == null)
                {
                    MP_Log.Error("TargetDef is not a ProjectilePropertiesCE", def: targetDef);
                    return;
                }

                XmlElement projectileElement = xmlDoc.CreateElement("projectile");
                projectileElement.SetAttribute("Class", "CombatExtended.ProjectilePropertiesCE");

                //props
                bool needPatchProjectile = false;

                ProjectilePropertiesCE defaultProps = new ProjectilePropertiesCE();
                ProjectilePropertiesCE originalProps = projectile?.OriginalData;
                foreach (var fieldName in ProjectileDefSaveable.propNames)
                {
                    object value = PropUtility.GetPropValueString(props, fieldName);
                    object defaultValue = PropUtility.GetPropValueString(defaultProps, fieldName);
                    object originalValue = PropUtility.GetPropValueString(originalProps, fieldName);

                    if (!object.Equals(value, defaultValue))
                    {
                        XmlUtility.AddChildElement(xmlDoc, projectileElement, fieldName, value.ToString());
                    }

                    if (!object.Equals(value, originalValue))
                    {
                        needPatchProjectile = true;
                    }
                }

                //secondary damage
                if (!props.secondaryDamage.NullOrEmpty())
                {
                    XmlElement secondaryDamageElement = xmlDoc.CreateElement("secondaryDamage");
                    projectileElement.AppendChild(secondaryDamageElement);

                    foreach (var item in props.secondaryDamage)
                    {
                        XmlElement liElement = xmlDoc.CreateElement("li");
                        secondaryDamageElement.AppendChild(liElement);

                        XmlUtility.AddChildElement(xmlDoc, liElement, "def", item.def.defName);
                        XmlUtility.AddChildElement(xmlDoc, liElement, "amount", item.amount.ToString());
                    }
                }

                if (projectile.SecondaryDamageChanged)
                {
                    needPatchProjectile = true;
                }

                if (needPatchProjectile)
                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/projectile", projectileElement));

                //secondary explosion
                if (targetDef.HasComp<CompExplosiveCE>())
                {
                    CompProperties_ExplosiveCE compProps = targetDef.GetCompProperties<CompProperties_ExplosiveCE>();
                    CompProperties_ExplosiveCE defaultCompProps = new CompProperties_ExplosiveCE();

                    XmlElement valueElemnet = xmlDoc.CreateElement("li");
                    valueElemnet.SetAttribute("Class", "CombatExtended.CompProperties_ExplosiveCE");

                    foreach (var item in SecondaryExplosionSaveable.propNames)
                    {
                        object value = PropUtility.GetPropValue(compProps, item);
                        object defaultValue = PropUtility.GetPropValue(defaultCompProps, item);
                        if (value != defaultValue)
                        {
                            XmlUtility.AddChildElement(xmlDoc, valueElemnet, item, value.ToString());
                        }
                    }

                    if (compProps.explosiveDamageType != null)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElemnet, "explosiveDamageType", compProps.explosiveDamageType.defName);
                    }

                    if (compProps.postExplosionGasType != null)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElemnet, "postExplosionGasType", compProps.postExplosionGasType.ToString());
                    }

                    //add
                    if (projectile.SecondaryExplosionChanged)
                    {
                        // add if not exists
                        root.AppendChild(XmlUtility.PatchAddEmptyComp(xmlDoc, targetDef.defName, typeof(CompProperties_ExplosiveCE).ToString()));

                        root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/comps/li[@Class = \"{typeof(CompProperties_ExplosiveCE)}\"]", valueElemnet));
                    }
                }

                //fragments
                if (targetDef.HasComp<CompFragments>())
                {
                    CompProperties_Fragments compProps = targetDef.GetCompProperties<CompProperties_Fragments>();

                    XmlElement valueElement = xmlDoc.CreateElement("li");
                    valueElement.SetAttribute("Class", "CombatExtended.CompProperties_Fragments");

                    XmlElement fragmentsElement = xmlDoc.CreateElement("fragments");
                    valueElement.AppendChild(fragmentsElement);

                    foreach (var fragment in compProps.fragments)
                    {
                        XmlUtility.AddChildElement(xmlDoc, fragmentsElement, fragment.thingDef.defName, fragment.count.ToString());
                    }

                    if (projectile.FragmentsChanged)
                    {
                        //add if not exists
                        root.AppendChild(XmlUtility.PatchAddEmptyComp(xmlDoc, targetDef.defName, typeof(CompProperties_Fragments).ToString()));

                        root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/comps/li[@Class = \"{typeof(CompProperties_Fragments)}\"]", valueElement));
                    }
                }

                //label
                if (targetDef.label != null && targetDef.label != this.projectile.OriginalLabel)
                {
                    XmlElement valueElement = xmlDoc.CreateElement("label");
                    valueElement.InnerText = targetDef.label;

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{targetDef.defName}\"]/label", valueElement));
                }
            }

            void ReplaceAmmo()
            {
                if (ammoDef == null)
                {
                    return;
                }

                if (ammoDef.label != ammo.OriginalLabel)
                {
                    XmlElement valueElement = xmlDoc.CreateElement("label");
                    valueElement.InnerText = ammoDef.label;
                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/ThingDef[defName=\"{ammoDef.defName}\"]/label", valueElement));
                }
            }

            void ReplaceRecipe()
            {
                if (ammoDef == null || recipeDef == null)
                {
                    return;
                }

                //product count
                ThingDefCountClass product = recipeDef.products.FirstOrDefault(x => x.thingDef == this.ammoDef);
                if (product != null && product.count != recipe.OriginalProductCount)
                {
                    XmlElement valueElement = xmlDoc.CreateElement(ammoDef.defName);
                    valueElement.InnerText = product.count.ToString();

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/RecipeDef[defName=\"{recipeDef.defName}\"]/products/{ammoDef}", valueElement));
                }

                //work amount
                if (Math.Abs(recipeDef.workAmount - recipe.OriginalWorkAmount) > float.Epsilon)
                {
                    XmlElement valueElement = xmlDoc.CreateElement("workAmount");
                    valueElement.InnerText = recipeDef.workAmount.ToString();

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/RecipeDef[defName=\"{recipeDef.defName}\"]", valueElement));
                }

                //ingredients
                if (!recipe.CompareOriginalIngredients())
                {
                    //count
                    XmlElement ingredientsElement = xmlDoc.CreateElement("ingredients");
                    foreach (var item in recipeDef.ingredients)
                    {
                        ThingDef thingDef = item.filter.AllowedThingDefs.FirstOrDefault();
                        if (thingDef == null)
                        {
                            continue;
                        }

                        int count = (int)item.GetBaseCount();
                        if (count <= 0)
                        {
                            continue;
                        }

                        XmlElement liElement = xmlDoc.CreateElement("li");

                        XmlElement filterElement = xmlDoc.CreateElement("filter");
                        liElement.AppendChild(filterElement);

                        XmlElement thingDefsElement = xmlDoc.CreateElement("thingDefs");
                        filterElement.AppendChild(thingDefsElement);

                        foreach (var def in item.filter.AllowedThingDefs)
                        {
                            XmlUtility.AddChildElement(xmlDoc, thingDefsElement, "li", def.defName);
                        }

                        XmlUtility.AddChildElement(xmlDoc, liElement, "count", count.ToString());
                        ingredientsElement.AppendChild(liElement);
                    }

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/RecipeDef[defName=\"{recipeDef.defName}\"]/ingredients", ingredientsElement));

                    //fixedIngredientFilter
                    XmlElement fixedIngredientFilterElement = xmlDoc.CreateElement("fixedIngredientFilter");
                    XmlUtility.AddChildElementList(xmlDoc, fixedIngredientFilterElement, "thingDefs", recipeDef.fixedIngredientFilter.AllowedThingDefs.Select(x => x.defName).ToList());

                    if (!recipe.CompareOriginalFixedIngredientFilter())
                        root.AppendChild(XmlUtility.PatchReplace(xmlDoc, $"Defs/RecipeDef[defName=\"{recipeDef.defName}\"]/fixedIngredientFilter", fixedIngredientFilterElement));
                }
            }

        }
    }
}
