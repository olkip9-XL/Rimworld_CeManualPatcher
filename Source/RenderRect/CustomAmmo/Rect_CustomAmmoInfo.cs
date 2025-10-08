using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_CustomAmmoInfo : RenderRectBase
    {

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 0f;

        private List<string> ammoBase = new List<string>()
        {
            "SmallAmmoBase",
            "NeolithicAmmoBase",
            "SpacerSmallAmmoBase",
            "SpacerAmmoBase",
            "SpacerMediumAmmoBase",
            "MediumAmmoBase",
            "HeavyAmmoBase"
        };

        CustomAmmoManager manager => CustomAmmoManager.instance;
        CustomAmmoSet curAmmoSet => CustomAmmoManager.curAmmoSet;

        MP_Ammo copiedAmmo
        {
            get => AmmoManager.instance.copiedAmmo;
            set => AmmoManager.instance.copiedAmmo = value;
        }

        private readonly Color ColorGrey = new Color(107f / 255f, 113f / 255f, 122f / 255f);

        public override void DoWindowContents(Rect rect)
        {
            if (curAmmoSet == null)
                return;

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            //head

            DrawHead(listing);
            if (curAmmoSet == null)
            {
                return;
            }

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f - 30f), ref scrollPosition, ref scrollViewHeight, (innerListing) =>
            {
                try
                {
                    DrawBaseInfo(innerListing);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Error while drawing BaseInfo : {e}");
                }

                //ammo reference
                try
                {
                    DrawAmmoReference(innerListing);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Error while drawing Ammo Reference : {e}");
                }

                foreach (var item in curAmmoSet.ammoTypes)
                {
                    try
                    {
                        innerListing.GapLine();

                        if (DrawAmmoHead(innerListing, item)) break;
                        DrawAmmo(innerListing, item.ammo);
                        DrawProjectile(innerListing, item.projectile);
                        DrawRecipe(innerListing, item.recipe);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[CeManualPatcher] Error while drawing Custom Ammo {item.ammo?.DefName ?? "null"} : {e}");
                    }
                }

            });

            DrawControlPannel(listing);

            listing.End();
        }

        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //save
            Rect saveRect = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(saveRect, "MP_Save".Translate()))
            {
                if (manager.SaveToLocal())
                    Messages.Message("MP_SaveSuccess".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            //export
            Rect exportCEPatchRect = rect.RightPartPixels(120f);
            if (Widgets.ButtonText(exportCEPatchRect, "MP_Export".Translate()))
            {
                Mod_CEManualPatcher.settings.ExportPatch();
            }
        }

        private void DrawAmmoReference(Listing_Standard listing)
        {
            listing.GapLine();

            //head
            listing.ButtonImageLine("<b>" + "MP_AmmoReference".Translate() + "</b>", TexButton.Add, () =>
            {
                Find.WindowStack.Add(new Dialog_AddAmmoReference(curAmmoSet.referencedAmmoStr, curAmmoSet.referencedProjectileStr));
            });

            //items
            foreach (var item in curAmmoSet.referencedAmmoStr)
            {
                AmmoDef ammo = DefDatabase<AmmoDef>.GetNamedSilentFail(item);

                Rect rect = listing.GetRect(Text.LineHeight);
                rect.width -= 20f;
                rect.x += 20f;

                Rect iconRect = rect.LeftPartPixels(rect.height);
                Texture2D icon = ammo.uiIcon ?? BaseContent.BadTex;
                Widgets.DrawTextureFitted(iconRect, icon, 1f);

                Rect labelRect = iconRect.RightAdjoin(rect.width - rect.height);
                Widgets.Label(labelRect, ammo.LabelCap);

                Rect buttonRect = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(buttonRect, TexButton.Delete))
                {
                    curAmmoSet.referencedAmmoStr.Remove(item);
                    break;
                }
            }
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            Rect iconRect = rect.LeftPartPixels(rect.height);
            Widgets.DrawTextureFitted(iconRect, curAmmoSet.Icon, 0.7f);

            Rect labelRect = iconRect.RightAdjoin(rect.width);
            Text.Font = GameFont.Medium;
            WidgetsUtility.LabelChange(labelRect, ref curAmmoSet.label, curAmmoSet.GetHashCode(), () =>
            {
            }, 200f);
            Text.Font = GameFont.Small;

            Rect deleteRect = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                Find.WindowStack.Add(new Dialog_Confirm($"MP_ConfirmDelete".Translate(curAmmoSet.Label), () =>
                {
                    manager.AllSets.Remove(curAmmoSet);
                    CustomAmmoManager.curAmmoSet = null;
                }));
                return;
            }

            Rect addRect = deleteRect.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                curAmmoSet.ammoTypes.Add(new AmmoProjectilePair(curAmmoSet));
            }

        }

        private void DrawBaseInfo(Listing_Standard listing)
        {
            listing.LabelLine($"<b>{"MP_C_BaseInfo".Translate()}</b>");

            //common
            listing.FieldLine("MP_DefNameBase".Translate(), ref curAmmoSet.defNameBase, indent: 20f);
            listing.FieldLine("MP_Description".Translate(), ref curAmmoSet.description, indent: 20f);

            //category
            listing.ButtonTextLine("MP_Category".Translate(), curAmmoSet.categoryDef?.label ?? "MP_Null".Translate(), () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.ammoCategoryDefs,
                    (cat) => cat?.label ?? "MP_Null".Translate(),
                    (cat) => delegate
                    {
                        curAmmoSet.categoryDef = cat;
                    });
            }, indent: 20f);

            listing.ButtonImageLine("MP_CategoryIcon".Translate(), ContentFinder<Texture2D>.Get(curAmmoSet.categoryTexPath, false) ?? BaseContent.BadTex, () =>
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (var item in MP_Options.ammoCategoryIcons)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    options.Add(new FloatMenuOption(item, () =>
                    {
                        curAmmoSet.categoryTexPath = item;
                    }, null, ContentFinder<Texture2D>.Get(item, false) ?? BaseContent.BadTex));
                }

                if (options.Any())
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }, indent: 20f);

            //ammo
            listing.ButtonTextLine("MP_AmmoParent".Translate(), curAmmoSet.parentAmmo ?? "MP_Null".Translate(), () =>
            {
                FloatMenuUtility.MakeMenu(ammoBase,
                    (parent) => parent,
                    (parent) => delegate
                    {
                        curAmmoSet.parentAmmo = parent;
                    });
            }, indent: 20f);

            listing.FieldLine(StatDefOf.Mass.label, ref curAmmoSet.baseMass, indent: 20f);
            listing.FieldLine(CE_StatDefOf.Bulk.label, ref curAmmoSet.baseBulk, indent: 20f);

            DrawGraphicSelector(listing, "MP_ProjectileGraphic".Translate(), curAmmoSet.projectileGraphicWarpper, MP_Options.projectileTex);
            listing.FieldLine("MP_ProjectileSpeed".Translate(), ref curAmmoSet.baseSpeed, indent: 20f);

            //tradeTags
            DrawTradeTags(listing, curAmmoSet.tradeTags, curAmmoSet.parentAmmo, 20f);
        }

        private void DrawTradeTags(Listing_Standard listing, List<string> tradeTags, string parentDef, float indent = 0f)
        {
            if (tradeTags == null)
            {
                Log.Warning($"[CeManualPatcher] TradeTags is null");
                return;
            }

            listing.ButtonImageLine("MP_TradeTags".Translate(), TexButton.Add, () =>
            {
                List<string> list = new List<string>(MP_Options.tradeTags);
                list = list.Except(tradeTags).ToList();

                FloatMenuUtility.MakeMenu(list,
                    (tag) => tag,
                    (tag) => delegate
                    {
                        tradeTags.Add(tag);
                    });
            }, indent: indent);

            listing.LabelLine("CE_AmmoInjector", indent: indent + 20f);
            switch (parentDef)
            {
                case "SpacerMediumAmmoBase":
                case "MediumAmmoBase":
                    listing.LabelLine("CE_MediumAmmo", indent: indent + 20f);
                    break;
                case "HeavyAmmoBase":
                    listing.LabelLine("CE_HeavyAmmo", indent: indent + 20f);
                    break;
                default:
                    listing.LabelLine("CE_Ammo", indent: indent + 20f);
                    break;
            }

            bool needBreak = false;
            foreach (var item in tradeTags)
            {
                listing.ButtonImageLine(item, TexButton.Delete, () =>
                {
                    tradeTags.Remove(item);
                    needBreak = true;
                }, indent: indent + 20f);
                if (needBreak)
                {
                    break;
                }
            }

        }

        private bool DrawAmmoHead(Listing_Standard listing, AmmoProjectilePair pair)
        {
            CustomAmmo ammo = pair.ammo;

            Rect rect = listing.GetRect(Text.LineHeight);

            //delete
            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Delete))
            {
                //curAmmoSet.ammoTypes.Remove(pair);
                Find.WindowStack.Add(new Dialog_Confirm($"MP_ConfirmDelete".Translate(pair.ammo.label), () =>
                {
                    curAmmoSet.ammoTypes.Remove(pair);
                }));
                return true;
            }

            //label;
            WidgetsUtility.LabelChange(rect, ref ammo.label, ammo.GetHashCode());

            //paste
            Rect rect2 = rect1.LeftAdjoin(rect.height);
            if (copiedAmmo != null && Widgets.ButtonImage(rect2, TexButton.Paste))
            {
                CopyAmmo(copiedAmmo, pair);
                copiedAmmo = null;
            }

            listing.FieldLine("MP_Suffix".Translate(), ref pair.suffix);

            return false;
        }

        private void CopyAmmo(MP_Ammo source, AmmoProjectilePair target)
        {
            if (source == null || target == null)
            {
                return;
            }

            //ammo
            AmmoDef sourceAmmo = source.ammo as AmmoDef;
            CustomAmmo targetAmmo = target.ammo;
            if (sourceAmmo != null && targetAmmo != null)
            {
                targetAmmo.graphicWarpper.texPath = sourceAmmo.graphicData.texPath;
                targetAmmo.graphicWarpper.graphicClass = sourceAmmo.graphicData.graphicClass;
                targetAmmo.ammoCategoryDef = sourceAmmo.ammoClass;
                targetAmmo.mass = sourceAmmo.statBases.FirstOrDefault(x => x.stat == StatDefOf.Mass).value;
                targetAmmo.bulk = sourceAmmo.statBases.FirstOrDefault(x => x.stat == CE_StatDefOf.Bulk).value;
            }

            //projectile
            ThingDef projectileDef = source.projectile;
            CustomProjectile targetProjectile = target.projectile;
            if (projectileDef != null && targetProjectile != null)
            {
                targetProjectile.graphicWarpper.texPath = projectileDef.graphicData.texPath;
                targetProjectile.graphicWarpper.graphicClass = projectileDef.graphicData.graphicClass;

                ProjectilePropertiesCE props = projectileDef.projectile as ProjectilePropertiesCE;
                if (props != null)
                {
                    //common
                    targetProjectile.damageAmount = (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props);
                    targetProjectile.damageDef = props.damageDef;
                    targetProjectile.explosionRadius = props.explosionRadius;
                    targetProjectile.suppressionFactor = props.suppressionFactor;
                    targetProjectile.stoppingPower = props.stoppingPower;
                    targetProjectile.speed = props.speed;
                    targetProjectile.armorPenetrationBlunt = props.armorPenetrationBlunt;
                    targetProjectile.armorPenetrationSharp = props.armorPenetrationSharp;

                    targetProjectile.secondaryDamages.Clear();
                    if (!props.secondaryDamage.NullOrEmpty())
                    {
                        foreach (var item in props.secondaryDamage)
                        {
                            targetProjectile.secondaryDamages.Add(new SecondaryDamage()
                            {
                                def = item.def,
                                amount = item.amount
                            });
                        }
                    }
                    targetProjectile.gas = props.postExplosionGasType;
                    targetProjectile.fragments.Clear();

                    //comps
                    if (projectileDef.HasComp<CompFragments>())
                    {
                        CompProperties_Fragments compProps = projectileDef.GetCompProperties<CompProperties_Fragments>();
                        if (compProps != null)
                        {
                            foreach (var item in compProps.fragments)
                            {
                                targetProjectile.fragments.Add(new ThingDefCountClass(item.thingDef, item.count));
                            }
                        }
                    }
                    if (projectileDef.HasComp<CompExplosiveCE>())
                    {
                        CompProperties_ExplosiveCE compProps = projectileDef.GetCompProperties<CompProperties_ExplosiveCE>();
                        if (compProps != null)
                        {
                            PropUtility.CopyPropValue(compProps, targetProjectile.secondaryExplosion);
                        }
                    }


                }
            }

        }

        private void DrawAmmo(Listing_Standard listing, CustomAmmo ammo)
        {
            listing.LabelLine($"<b>{"MP_Ammo".Translate()}</b>");

            DrawGraphicSelector(listing, "MP_AmmoGraphic".Translate(), ammo.graphicWarpper, MP_Options.ammoTex);

            listing.ButtonTextLine("MP_AmmoClass".Translate(), ammo.ammoCategoryDef?.LabelCap ?? "MP_Null".Translate(), () =>
            {
                List<AmmoCategoryDef> list = DefDatabase<AmmoCategoryDef>.AllDefsListForReading;

                FloatMenuUtility.MakeMenu(list,
                    (def) => def?.LabelCap ?? "MP_Null".Translate(),
                    (def) => delegate
                    {
                        ammo.ammoCategoryDef = def;
                    });
            }, indent: 20f);

            listing.FieldLine(StatDefOf.Mass.label, ref ammo.mass, indent: 20f);
            listing.FieldLine(CE_StatDefOf.Bulk.label, ref ammo.bulk, indent: 20f);
        }

        private void DrawProjectile(Listing_Standard listing, CustomProjectile projectile)
        {
            bool isExpo = false;
            if (Math.Abs(projectile.explosionRadius) > float.Epsilon)
            {
                isExpo = true;
            }

            listing.LabelLine($"<b>{"MP_Projectile".Translate()}</b>");

            DrawGraphicSelector(listing, "MP_ProjectileGraphic".Translate(), projectile.graphicWarpper, MP_Options.projectileTex);

            listing.FieldLine("MP_DamageAmountBase".Translate(), ref projectile.damageAmount, indent: 20f);
            listing.ButtonTextLine("MP_DamageDef".Translate(), projectile.damageDef?.label ?? "MP_Null".Translate(), () =>
            {
                FloatMenuUtility.MakeMenu(GetDamageOptions(projectile.secondaryDamages, projectile.damageDef, isExpo),
               (def) => def?.label ?? "MP_Null".Translate(),
               (def) => delegate
               {
                   projectile.damageDef = def;
               });
            }, indent: 20f);

            ;

            listing.FieldLine("CE_DescExplosionRadius".Translate(), ref projectile.explosionRadius, min: 0f, indent: 20f);

            listing.FieldLine("MP_DescSuppressionFactor".Translate(), ref projectile.suppressionFactor, indent: 20f);
            listing.FieldLine("MP_DescStoppingPower".Translate(), ref projectile.stoppingPower, indent: 20f);
            listing.FieldLine("MP_ProjectileSpeed".Translate(), ref projectile.speed, indent: 20f);

            listing.FieldLine($"CE_DescBluntPenetration".Translate().Colorize(isExpo ? ColorGrey : Color.white), ref projectile.armorPenetrationBlunt, indent: 20f);
            listing.FieldLine($"CE_DescSharpPenetration".Translate().Colorize(isExpo ? ColorGrey : Color.white), ref projectile.armorPenetrationSharp, indent: 20f);

            foreach (var fieldName in ProjectileDefSaveable.propNames)
            {
                if (fieldName == "armorPenetrationSharp" ||
                   fieldName == "armorPenetrationBlunt" ||
                   fieldName == "explosionRadius")
                {
                    continue; // Handled above
                }

                listing.FieldLineReflexion($"MP_ProjectilePropertiesCE.{fieldName}".Translate(), fieldName, projectile, null, indent: 20f);
            }

            DrawSecondaryDamages(listing, projectile.secondaryDamages, isExpo);

            listing.ButtonTextLine($"MP_DescExplosionGas".Translate().Colorize(isExpo ? Color.white : ColorGrey), projectile.gas.GetLabel(), () =>
            {
                List<GasType?> list = new List<GasType?>();
                list.Add(null);
                list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());
                FloatMenuUtility.MakeMenu(list,
                    (gas) => gas.GetLabel(),
                    (gas) => delegate
                    {
                        projectile.gas = gas;
                    });
            }, indent: 20f);

            //comps
            DrawSecondaryExplosion(listing, projectile.secondaryExplosion);
            DrawFragments(listing, projectile.fragments);

        }

        private void DrawSecondaryDamages(Listing_Standard listing, List<SecondaryDamage> damages, bool isExpo)
        {
            if (damages == null)
            {
                Log.Warning($"[CeManualPatcher] Secondary damages is null");
                return;
            }

            listing.ButtonImageLine($"MP_SecondaryDamage".Translate().Colorize(isExpo ? ColorGrey : Color.white), TexButton.Add, () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.allDamageDefs,
                    (def) => def?.LabelCap ?? "MP_Null".Translate(),
                    (def) => delegate
                    {
                        damages.Add(new SecondaryDamage() { def = def, amount = 1 });
                    });

            }, indent: 20f);
            foreach (var item in damages)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 40f;
                rect.width -= 40f;

                Rect rect1 = rect.LeftPartPixels(200f);
                if (Widgets.ButtonText(rect1, item.def?.LabelCap ?? "MP_Null".Translate()))
                {
                    FloatMenuUtility.MakeMenu(MP_Options.allDamageDefs,
                        (def) => def?.LabelCap ?? "MP_Null".Translate(),
                        (def) => delegate
                        {
                            item.def = def;
                        });
                }

                Rect rect2 = rect1.RightAdjoin(100f);
                WidgetsUtility.TextFieldOnChange(rect2, ref item.amount, null);

                Rect rect3 = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(rect3, TexButton.Delete))
                {
                    damages.Remove(item);
                    break;
                }

            }
        }

        private List<DamageDef> GetDamageOptions(List<SecondaryDamage> secondaryDamages, DamageDef mainDamageDef, bool isExpo)
        {
            List<DamageDef> list = new List<DamageDef>();
            if (isExpo)
            {
                list.AddRange(MP_Options.allDamageDefsEX);
            }
            else
            {
                list.AddRange(MP_Options.allDamageDefs);
            }

            if (secondaryDamages != null)
            {
                list = list.Except(secondaryDamages.Select(x => x.def)).ToList();
            }

            list.Remove(mainDamageDef);

            return list;
        }

        private void DrawFragments(Listing_Standard listing, List<ThingDefCountClass> fragments)
        {
            if (fragments == null)
            {
                Log.Warning($"[CeManualPatcher] Fragments is null");
                return;
            }

            Color fontColor = Color.white;
            if (fragments.NullOrEmpty())
            {
                fontColor = ColorGrey;
            }

            listing.ButtonImageLine("CE_DescFragments".Translate().Colorize(fontColor), TexButton.Add, () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.fragments,
                    (def) => def?.LabelCap,
                    (def) => delegate
                    {
                        if (!fragments.Any(x => x.thingDef == def))
                        {
                            fragments.Add(new ThingDefCountClass(def, 1));
                        }
                    });
            }, indent: 20f);

            foreach (var item in fragments)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 40f;
                rect.width -= 40f;

                Widgets.Label(rect, item.thingDef?.LabelCap ?? "MP_Null".Translate());

                Rect rect1 = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(rect1, TexButton.Delete))
                {
                    fragments.Remove(item);
                    break;
                }

                Rect rect2 = rect1.LeftAdjoin(100f);
                WidgetsUtility.TextFieldOnChange(rect2, ref item.count, null);

                Rect rect3 = rect2.LeftAdjoin(Text.CalcSize("x").x);
                Widgets.Label(rect3, "x");
            }

        }

        private void DrawSecondaryExplosion(Listing_Standard listing, CompProperties_ExplosiveCE explosiveCE)
        {
            if (explosiveCE == null)
            {
                Log.Warning($"[CeManualPatcher] ExplosiveCE is null");
                return;
            }

            Color fontColor = Color.white;

            if (explosiveCE.explosiveDamageType == null)
            {
                fontColor = ColorGrey;
            }

            listing.LabelLine("CE_DescSecondaryExplosion".Translate().Colorize(fontColor), indent: 20f);
            listing.FieldLine("MP_DamageAmountBase".Translate().Colorize(fontColor), ref explosiveCE.damageAmountBase, indent: 40f);
            listing.ButtonTextLine("MP_DamageDef".Translate().Colorize(fontColor), explosiveCE.explosiveDamageType?.label ?? "MP_Null".Translate(), () =>
            {
                List<DamageDef> list = new List<DamageDef>();
                list.Add(null);
                list.AddRange(MP_Options.allDamageDefsEX);

                FloatMenuUtility.MakeMenu(list,
                    (def) => def?.label ?? "MP_Null".Translate(),
                    (def) => delegate
                    {
                        explosiveCE.explosiveDamageType = def;
                    });
            }, indent: 40f);
            listing.FieldLine("CE_DescExplosionRadius".Translate().Colorize(fontColor), ref explosiveCE.explosiveRadius, indent: 40f);
            listing.ButtonTextLine("MP_DescExplosionGas".Translate().Colorize(fontColor), explosiveCE.postExplosionGasType.GetLabel(), () =>
            {
                List<GasType?> list = new List<GasType?>();
                list.Add(null);
                list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());
                FloatMenuUtility.MakeMenu(list,
                    (gas) => gas.GetLabel(),
                    (gas) => delegate
                    {
                        explosiveCE.postExplosionGasType = gas;
                    });
            }, indent: 40f);

        }

        private void DrawGraphicSelector(Listing_Standard listing, TaggedString label, MP_GraphicWarpper warpper, ReadOnlyDictionary<Type, List<string>> texDic)
        {
            if (warpper == null)
            {
                return;
            }

            Rect rect = listing.GetRect(30f);
            rect.x += 20f;
            rect.width -= 20f;

            Widgets.Label(rect, label);

            //texture
            Rect rect1 = rect.RightPartPixels(rect.height);
            Texture2D texture = GetTexture(warpper) ?? BaseContent.BadTex;
            if (Widgets.ButtonImage(rect1, texture))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                if (texDic.ContainsKey(warpper.graphicClass))
                {
                    foreach (string path in texDic[warpper.graphicClass])
                    {
                        options.Add(new FloatMenuOption(path, () =>
                        {
                            warpper.texPath = path;
                        }, null, GetTexture(new MP_GraphicWarpper(path, warpper.graphicClass))));
                    }
                }
                else
                {
                    options.Add(new FloatMenuOption("No Texture", null));
                }

                if (options.Any())
                    Find.WindowStack.Add(new FloatMenu(options));
            }

            //label1
            string label1 = "MP_GraphicTexture".Translate();
            float label1Width = Text.CalcSize(label1).x;
            Rect rect2 = rect1.LeftAdjoin(label1Width);
            Widgets.Label(rect2, label1);

            //class
            Rect rect3 = rect2.LeftAdjoin(150f);
            if (Widgets.ButtonText(rect3, warpper.graphicClass?.Name ?? "MP_Null".Translate()))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var item in texDic)
                {
                    options.Add(new FloatMenuOption(item.Key.Name, () =>
                    {
                        warpper.graphicClass = item.Key;
                        warpper.texPath = "";
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            //label2
            string label2 = "MP_GraphicClass".Translate();
            float label2Width = Text.CalcSize(label2).x;
            Rect rect4 = rect3.LeftAdjoin(label2Width);
            Widgets.Label(rect4, label2);

            //misc
            Widgets.DrawHighlightIfMouseover(rect);

            listing.Gap(listing.verticalSpacing);
        }

        Texture2D GetTexture(MP_GraphicWarpper warpper)
        {
            if (!typeof(Graphic).IsAssignableFrom(warpper.graphicClass))
            {
                Log.Error($"graphicClass ({warpper.graphicClass.Name}) is not a Graphic");
                return BaseContent.BadTex;
            }

            if (warpper.texPath.NullOrEmpty())
            {
                return BaseContent.BadTex;
            }

            var graphic = GraphicDatabase.Get(warpper.graphicClass, warpper.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white, Color.white);
            if (graphic == null)
            {
                Log.Warning($"Load texture failed: texPath = {warpper.texPath}, graphicClass = {warpper.graphicClass.Name}");
                return BaseContent.BadTex;
            }

            if (warpper.graphicClass == typeof(Graphic_Random))
            {
                graphic = (graphic as Graphic_Random).FirstSubgraphic();
            }

            Texture2D texture = graphic.MatSingle.mainTexture as Texture2D;
            return texture ?? BaseContent.BadTex;
        }


        private void DrawRecipe(Listing_Standard listing, CustomAmmoRecipe recipe)
        {
            //title
            listing.LabelLine("<b>" + "MP_Recipe".Translate() + "</b>");

            //parentClass
            listing.ButtonTextLine("MP_RecipeParentClass".Translate(), recipe.parentRecipeClass, () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.parentAmmoRecipeClass,
                    (x) => x,
                    (x) => delegate
                    {
                        recipe.parentRecipeClass = x;
                    });
            }, indent: 20f);

            //properties
            listing.FieldLine("MP_RecipeLabel".Translate(), ref recipe.label, indent: 20f);
            listing.FieldLine("MP_RecipeDescription".Translate(), ref recipe.description, indent: 20f);
            listing.FieldLine("MP_RecipeJobString".Translate(), ref recipe.jobString, indent: 20f);
            listing.FieldLine("MP_RecipeWorkAmount".Translate(), ref recipe.workAmount, indent: 20f);
            listing.FieldLine("MP_RecipeProductAmount".Translate(), ref recipe.productAmount, indent: 20f);

            //ingredients
            listing.ButtonImageLine("MP_RecipeIngredients".Translate(), TexButton.Add, () =>
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var item in MP_Options.ingredientsForAmmo)
                {
                    FloatMenuOption option = new FloatMenuOption(item.LabelCap, () =>
                    {
                        recipe.ingredients.Add(new MP_ThingDefCountClass_Save(item, 1));
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

            }, indent: 20f);

            foreach (var item in recipe.ingredients)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 40f;
                rect.width -= 40f;

                //icon
                Rect rect4 = rect.LeftPartPixels(rect.height);
                Widgets.DrawTextureFitted(rect4, item.thingDef.uiIcon, 1f);

                //label
                Rect rect5 = rect4.RightAdjoin(rect.width - rect.height);
                Widgets.Label(rect5, item.thingDef?.LabelCap ?? "MP_Null".Translate());

                //delete
                Rect rect1 = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(rect1, TexButton.Delete))
                {
                    recipe.ingredients.Remove(item);
                    break;
                }

                //count
                Rect rect2 = rect1.LeftAdjoin(100f);
                WidgetsUtility.TextFieldOnChange(rect2, ref item.count, null);

                Rect rect3 = rect2.LeftAdjoin(Text.CalcSize("x").x);
                Widgets.Label(rect3, "x");
            }

        }
    }
}
