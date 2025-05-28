using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Misc.CustomAmmoMisc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher
{
    internal class Rect_AmmoInfo : RenderRectBase
    {
        private static MP_AmmoSet curAmmoSet => AmmoManager.curAmmoSet;
        private static AmmoManager manager => AmmoManager.instance;

        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight = 0f;

        private readonly float damageButtonWidth = 150f;

        public MP_Ammo copiedAmmo = null;

        public override void DoWindowContents(Rect rect)
        {
            if (curAmmoSet == null)
            {
                return;
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            WidgetsUtility.ScrollView(listingStandard.GetRect(rect.height - listingStandard.CurHeight - Text.LineHeight - 0.1f), ref scrollPosition, ref viewHeight, scrollListing =>
            {
                DrawHead(scrollListing);

                foreach (var item in curAmmoSet.ammoList)
                {
                    try
                    {
                        DrawAmmo(scrollListing, item);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce($"[CeManualPatcher] Error while drawing Ammo info {item?.DefName ?? "null"} : {e}", e.GetHashCode());
                    }

                }
            });

            DrawControlPannel(listingStandard);

            listingStandard.End();
        }


        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            //Reset all
            Rect rect1 = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(rect1, "MP_ResetAll".Translate()))
            {
                manager.ResetAll();
            }
        }

        private void DrawHead(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label(curAmmoSet.Label);
            Text.Font = GameFont.Small;
        }

        private void DrawAmmo(Listing_Standard listing, MP_Ammo ammo)
        {
            //无法编辑
            if (ammo?.projectile?.projectile?.damageDef == null)
            {
                listing.Label("Projectile is Null or Damage is Null");
                return;
            }

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            if (props == null)
            {
                listing.Label($"Not CE Projectile, Projectile Class is {ammo.projectile.projectile.GetType()}");
                DrawNonCEProjectile(listing, ammo.projectile);
                return;
            }

            listing.GapLine();
            DrawAmmoHead(listing, ammo);

            //Explosive
            if (ammo.isExplosive)
            {
                listing.FieldLineReflexion("CE_DescExplosionRadius".Translate(), "explosionRadius", props, newValue =>
                {
                    manager.GetAmmoPatch(ammo);
                });

                listing.ButtonX("MP_DescExplosionGas".Translate(), 100f, props.postExplosionGasType.GetLabel(), () =>
                {
                    List<GasType?> list = new List<GasType?>();
                    list.Add(null);
                    list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());

                    FloatMenuUtility.MakeMenu<GasType?>(list,
                        (gas) => gas.GetLabel(),
                        (gas) => delegate
                        {
                            manager.GetAmmoPatch(ammo);
                            props.postExplosionGasType = gas;
                        }
                    );
                });

                //damage
                DrawAmmoDamageEx(listing, ammo);
            }
            //Non-Explosive
            else
            {
                listing.FieldLineReflexion("CE_DescSharpPenetration".Translate(), "armorPenetrationSharp", props, newValue =>
                {
                    manager.GetAmmoPatch(ammo);
                });

                listing.FieldLineReflexion("CE_DescBluntPenetration".Translate(), "armorPenetrationBlunt", props, newValue =>
                {
                    manager.GetAmmoPatch(ammo);
                });

                //damage
                DrawAmmoDamageNEx(listing, ammo);
            }

            //common
            listing.FieldLineReflexion("MP_DescSuppressionFactor".Translate(), "suppressionFactor", props, newValue =>
            {
                manager.GetAmmoPatch(ammo);
            });

            listing.FieldLineReflexion("MP_DescStoppingPower".Translate(), "stoppingPower", props, newValue =>
            {
                manager.GetAmmoPatch(ammo);
            });

            //comps
            if (ammo.projectile.HasComp<CompFragments>())
            {
                CompProperties_Fragments compProps = ammo.projectile.GetCompProperties<CompProperties_Fragments>();

                //head
                Rect headRect = listing.GetRect(Text.LineHeight);
                Rect addRect = headRect.RightPartPixels(headRect.height);

                Widgets.Label(headRect, "CE_DescFragments".Translate());
                if (Widgets.ButtonImage(addRect, TexButton.Add))
                {
                    List<ThingDef> list = MP_Options.fragments.ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (x) => x.LabelCap,
                        (x) => delegate
                        {
                            manager.GetAmmoPatch(ammo);

                            if (!compProps.fragments.Any(y => y.thingDef == x))
                            {
                                compProps.fragments.Add(new ThingDefCountClass(x, 1));
                            }
                        }
                    );
                }

                //fragments
                for (int i = 0; i < compProps.fragments.Count; i++)
                {
                    DrawFragmentsRow(listing, ammo, compProps, i);
                }
            }

            if (ammo.projectile.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE compProps = ammo.projectile.GetCompProperties<CompProperties_ExplosiveCE>();

                listing.Label("CE_DescSecondaryExplosion".Translate());

                listing.DamageRow(compProps.explosiveDamageType.LabelCap, damageButtonWidth,
                    () =>
                    {
                        FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                            (x) => x.LabelCap,
                            (x) => delegate
                            {
                                manager.GetAmmoPatch(ammo);
                                compProps.explosiveDamageType = x;
                            }
                        );
                    }, (int)compProps.damageAmountBase, 100f,
                    (newValue) =>
                    {
                        manager.GetAmmoPatch(ammo);
                        compProps.damageAmountBase = newValue;
                    }, indent: 20f);

                listing.FieldLineReflexion("CE_DescExplosionRadius".Translate(), "explosiveRadius", compProps, newValue =>
                {
                    manager.GetAmmoPatch(ammo);
                }, indent: 20f);

                listing.ButtonX("MP_DescExplosionGas".Translate(), 100f, compProps.postExplosionGasType.GetLabel(), () =>
                {
                    List<GasType?> list = new List<GasType?>();
                    list.Add(null);
                    list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());
                    FloatMenuUtility.MakeMenu<GasType?>(list,
                        (gas) => gas.GetLabel(),
                        (gas) => delegate
                        {
                            manager.GetAmmoPatch(ammo);
                            compProps.postExplosionGasType = gas;
                        }
                    );
                }, indent: 20f);
            }

            //recipe
            RecipeDef recipe = DefDatabase<RecipeDef>.GetNamed("Make" + ammo.ammo?.defName, false);
            if (recipe != null)
            {

                listing.FieldLineOnChange("MP_RecipeWorkAmount".Translate(), ref recipe.workAmount, (newValue) =>
                {
                    manager.GetAmmoPatch(ammo);
                });

                ThingDefCountClass productCount = recipe.products.FirstOrDefault(x => x.thingDef == ammo.ammo);
                if (productCount != null)
                {
                    listing.FieldLineOnChange("MP_RecipeProductAmount".Translate(), ref productCount.count, (newValue) =>
                    {
                        manager.GetAmmoPatch(ammo);
                    });
                }

                //ingredients
                listing.ButtonImageLine("MP_RecipeIngredients".Translate(), TexButton.Add, () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var item in MP_Options.ingredientsForAmmo)
                    {
                        FloatMenuOption option = new FloatMenuOption(item.LabelCap, () =>
                        {
                            manager.GetAmmoPatch(ammo);

                            ThingFilter filter = new ThingFilter();
                            filter.SetAllow(item, true);

                            IngredientCount ingredientCount = new IngredientCount();
                            ingredientCount.filter = filter;
                            ingredientCount.SetBaseCount(1);

                            recipe.ingredients.Add(ingredientCount);

                            recipe.fixedIngredientFilter.SetAllow(item, true);
                        }, null, item.uiIcon);

                        if (option != null)
                        {
                            options.Add(option);
                        }
                    }
                    if (options.Any())
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }


                });

                foreach (var item in recipe.ingredients)
                {
                    Rect rect = listing.GetRect(Text.LineHeight);
                    rect.x += 20f;
                    rect.width -= 20f;

                    ThingDef ingredientDef = item.filter?.AllowedThingDefs?.FirstOrDefault();
                    int count = (int)item.GetBaseCount();

                    //icon
                    Rect rect4 = rect.LeftPartPixels(rect.height);
                    Widgets.DrawTextureFitted(rect4, ingredientDef.uiIcon, 1f);

                    //label
                    Rect rect5 = rect4.RightAdjoin(rect.width - rect.height);
                    Widgets.Label(rect5, ingredientDef?.LabelCap ?? "MP_Null".Translate());

                    //delete
                    Rect rect1 = rect.RightPartPixels(rect.height);
                    if (Widgets.ButtonImage(rect1, TexButton.Delete))
                    {
                        manager.GetAmmoPatch(ammo);

                        recipe.ingredients.Remove(item);
                        recipe.fixedIngredientFilter.SetAllow(ingredientDef, false);
                        break;
                    }

                    //count
                    Rect rect2 = rect1.LeftAdjoin(100f);
                    WidgetsUtility.TextFieldOnChange(rect2, ref count, (newValue) =>
                    {
                        manager.GetAmmoPatch(ammo);

                        item.SetBaseCount(newValue);
                    });

                    Rect rect3 = rect2.LeftAdjoin(Text.CalcSize("x").x);
                    Widgets.Label(rect3, "x");
                }


            }

        }

        private void DrawAmmoHead(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect signRect = rect.LeftPartPixels(4f);
            Rect iconRect = new Rect(signRect.xMax, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width - 4f, rect.height);
            Rect resetRect = labelRect.RightPartPixels(rect.height);

            if (manager.HasAmmoPatch(ammo))
            {
                Widgets.DrawBoxSolid(signRect, new Color(85f / 256f, 177f / 256f, 85f / 256f));
            }

            Widgets.DrawTextureFitted(iconRect, ammo.Icon, 0.7f);

            ref string label = ref ammo.projectile.label;
            if (ammo.ammo != null)
            {
                label = ref ammo.ammo.label;
            }

            WidgetsUtility.LabelChange(labelRect, ref label, ammo.GetHashCode(), () =>
            {
                manager.GetAmmoPatch(ammo);
            }, 100f);


            if (Widgets.ButtonImage(resetRect, MP_Texture.Reset))
            {
                manager.Reset(ammo.projectile);
            }

            //copy
            Rect copyRect = resetRect.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(copyRect, TexButton.Copy))
            {
                copiedAmmo = ammo;
            }

            Rect pasteRect = copyRect.LeftAdjoin(rect.height);
            if (copiedAmmo != null && Widgets.ButtonImage(pasteRect, TexButton.Paste))
            {
                manager.GetAmmoPatch(ammo);
                copyAmmo(copiedAmmo, ammo);
                copiedAmmo = null;
            }


            listing.Gap(listing.verticalSpacing);
        }

        private void copyAmmo(MP_Ammo source, MP_Ammo target)
        {
            if (source == null || target == null || source == target)
                return;

            ProjectilePropertiesCE sourcePrj = source.projectile.projectile as ProjectilePropertiesCE;
            ProjectilePropertiesCE targetPrj = target.projectile.projectile as ProjectilePropertiesCE;

            if (sourcePrj == null || targetPrj == null)
            {
                Log.Error($"[CeManualPatcher] Copy ammo failed, source: {source?.DefName ?? "null"}, target: {target?.DefName ?? "null"}");
            }

            //common
            targetPrj.damageDef = sourcePrj.damageDef;
            FieldInfo fieldInfo = typeof(ProjectileProperties).GetField("armorPenetrationBase", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(targetPrj, fieldInfo.GetValue(sourcePrj));
            targetPrj.suppressionFactor = sourcePrj.suppressionFactor;
            targetPrj.stoppingPower = sourcePrj.stoppingPower;

            //explosive
            if (target.isExplosive && source.isExplosive)
            {
                targetPrj.explosionRadius = sourcePrj.explosionRadius;
                targetPrj.postExplosionGasType = sourcePrj.postExplosionGasType;
            }

            //non explosive
            if (!target.isExplosive && !source.isExplosive)
            {
                targetPrj.armorPenetrationSharp = sourcePrj.armorPenetrationSharp;
                targetPrj.armorPenetrationBlunt = sourcePrj.armorPenetrationBlunt;

                sourcePrj.secondaryDamage?.Clear();
                if (!targetPrj.secondaryDamage.NullOrEmpty())
                {
                    foreach (var item in targetPrj.secondaryDamage)
                    {
                        sourcePrj.secondaryDamage.Add(new SecondaryDamage()
                        {
                            def = item.def,
                            amount = item.amount
                        });
                    }
                }
            }

            //comps
            if (target.projectile.HasComp<CompFragments>() && source.projectile.HasComp<CompFragments>())
            {
                CompProperties_Fragments targetComp = target.projectile.GetCompProperties<CompProperties_Fragments>();
                CompProperties_Fragments sourceComp = source.projectile.GetCompProperties<CompProperties_Fragments>();

                targetComp.fragments.Clear();
                foreach (var item in sourceComp.fragments)
                {
                    targetComp.fragments.Add(new ThingDefCountClass()
                    {
                        thingDef = item.thingDef,
                        count = item.count
                    });
                }
            }

            if (target.projectile.HasComp<CompExplosiveCE>() && source.projectile.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE targetComp = target.projectile.GetCompProperties<CompProperties_ExplosiveCE>();
                CompProperties_ExplosiveCE sourceComp = source.projectile.GetCompProperties<CompProperties_ExplosiveCE>();

                targetComp.explosiveDamageType = sourceComp.explosiveDamageType;
                targetComp.damageAmountBase = sourceComp.damageAmountBase;
                targetComp.explosiveRadius = sourceComp.explosiveRadius;
                targetComp.postExplosionGasType = sourceComp.postExplosionGasType;
            }

        }

        private void DrawAmmoDamageEx(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect fieldRect = rect.RightPartPixels(100f);
            Rect buttonRect = new Rect(fieldRect.x - 6f - damageButtonWidth, rect.y, damageButtonWidth, rect.height);

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            Widgets.Label(rect, "CE_DescBaseDamage".Translate());

            int damageAmountBase = (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props);
            //int damageAmountBase = (int)PropUtility.GetPropValue(props, "damageAmountBase");
            WidgetsUtility.TextFieldOnChange(fieldRect, ref damageAmountBase, newValue =>
            {
                manager.GetAmmoPatch(ammo);
                typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(props, newValue);
            });

            if (Widgets.ButtonText(buttonRect, props.damageDef.label))
            {
                List<DamageDef> list = DefDatabase<DamageDef>.AllDefs.Except(ammo.projectile.ExistDamages()).ToList();

                FloatMenuUtility.MakeMenu(list,
                    (x) => x.LabelCap,
                    (x) => delegate
                    {
                        manager.GetAmmoPatch(ammo);
                        props.damageDef = x;
                    }
                 );
            }

            listing.Gap(listing.verticalSpacing);
        }

        private void DrawAmmoDamageNEx(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(headRect.height);

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            Widgets.Label(headRect, "CE_DescDamage".Translate());

            //add button
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                    (x) => x.LabelCap,
                    (x) => delegate
                    {
                        manager.GetAmmoPatch(ammo);
                        props.secondaryDamage.Add(new SecondaryDamage()
                        {
                            def = x,
                            amount = 1
                        });
                    }
                );

            }

            //main damage
            for (int i = -1; i < props.secondaryDamage.Count; i++)
            {
                string label = i == -1 ? props.damageDef.LabelCap : props.secondaryDamage[i].def.LabelCap;
                int amount = i == -1 ? (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props) : props.secondaryDamage[i].amount;

                Action onDelete = delegate
                {
                    manager.GetAmmoPatch(ammo);
                    props.secondaryDamage.RemoveAt(i);
                };

                listing.DamageRow(label, damageButtonWidth,
                    () =>
                    {
                        int index = i;
                        FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                            (x) => x.LabelCap,
                            (x) => delegate
                            {
                                manager.GetAmmoPatch(ammo);
                                if (index == -1)
                                    props.damageDef = x;
                                else
                                    props.secondaryDamage[index].def = x;
                            }
                        );
                    }, amount, 100f,
                    (newValue) =>
                    {
                        manager.GetAmmoPatch(ammo);
                        if (i == -1)
                            //PropUtility.SetPropValue(props, "damageAmountBase", newValue);
                            typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(props, newValue);
                        else
                            props.secondaryDamage[i].amount = newValue;

                    }, i == -1 ? null : onDelete, 20f);
            }
        }
        private void DrawFragmentsRow(Listing_Standard listing, MP_Ammo ammo, CompProperties_Fragments compProps, int index)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;

            Rect deleteRect = rect.RightPartPixels(rect.height);
            Rect fieldRect = new Rect(deleteRect.x - 6f - 100f, rect.y, 100f, rect.height);
            Rect xRect = new Rect(fieldRect.x - rect.height, rect.y, rect.height, rect.height);

            ThingDefCountClass fragment = compProps.fragments[index];

            Widgets.Label(rect, fragment.thingDef.LabelCap);

            Widgets.Label(xRect, "x");

            WidgetsUtility.TextFieldOnChange(fieldRect, ref fragment.count, newValue =>
            {
                manager.GetAmmoPatch(ammo);
            });

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                manager.GetAmmoPatch(ammo);
                compProps.fragments.RemoveAt(index);
            }

            listing.Gap(listing.verticalSpacing);
        }
        private List<DamageDef> AvaliableDamageDefs(MP_Ammo ammo)
        {
            if (ammo.isExplosive)
            {
                return MP_Options.allDamageDefsEX.Except(ammo.projectile.ExistDamages()).ToList();
            }
            else
            {
                return MP_Options.allDamageDefs.Except(ammo.projectile.ExistDamages()).ToList();
            }
        }

        private void DrawNonCEProjectile(Listing_Standard listing, ThingDef projectileDef)
        {
            if (projectileDef == null) return;

            listing.GapLine(6f);

            //head
            Rect rect = listing.GetRect(Text.LineHeight);
            Widgets.LabelWithIcon(rect, projectileDef.LabelCap, projectileDef.uiIcon);

            ProjectileProperties props = projectileDef.projectile;
            if (props == null) return;

            //common
            FieldInfo fieldInfo = typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance);
            listing.Label("CE_DescBaseDamage".Translate() + " : " + $"{fieldInfo.GetValue(props)}({props.damageDef.LabelCap})");

            if (!props.extraDamages.NullOrEmpty())
            {
                foreach (var item in props.extraDamages)
                {
                    listing.Label($"\t{item.amount}({item.def.LabelCap})");
                }
            }

            fieldInfo = typeof(ProjectileProperties).GetField("armorPenetrationBase", BindingFlags.NonPublic | BindingFlags.Instance);
            listing.Label("CE_DescArmorPenetration".Translate() + " : " + fieldInfo.GetValue(props));
            listing.Label("MP_DescStoppingPower".Translate() + "  " + props.stoppingPower);

            //explosive
            if (props.explosionRadius > 0)
            {
                listing.Label("CE_DescExplosionRadius".Translate() + " : " + props.explosionRadius);
                listing.Label("MP_DescExplosionGas".Translate() + " : " + props.postExplosionGasType.GetLabel());
            }
            //non-explosive
            else
            {

            }
        }
    }
}
