﻿using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.RenderRect;
using CeManualPatcher.Saveable;
using CeManualPatcher.Saveable.Weapon;
using CombatExtended;
using CombatExtended.Compatibility;
using KTrie;
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
    internal class Rect_WeaponInfo : RenderRectBase
    {
        //scroll
        private float innerHeight = 0f;
        private Vector2 scrollPosition = Vector2.zero;

        //copy
        private ThingDef copiedThing = null;


        private static WeaponManager manager => WeaponManager.instance;

        private static CEPatchManager patchManager => CEPatchManager.instance;
        private static ThingDef curWeaponDef => WeaponManager.curWeaponDef;

        public override void DoWindowContents(Rect rect)
        {
            if (curWeaponDef == null)
            {
                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - Text.LineHeight - 0.1f), ref scrollPosition, ref innerHeight, (innerListing) =>
            {
                RenderRectUtility.DrawStats(innerListing, curWeaponDef.statBases, MP_Options.statDefs_Weapon, delegate
                {
                    manager.GetWeaponPatch(curWeaponDef);
                });
                DrawWeaponTags(innerListing, curWeaponDef.weaponTags);

                DrawVebs(innerListing, curWeaponDef.Verbs.FirstOrDefault(), curWeaponDef.GetCompProperties<CompProperties_AmmoUser>(), curWeaponDef.weaponTags);
                DrawTools(innerListing, curWeaponDef.tools);
                DrawComps(innerListing,
                    curWeaponDef.GetCompProperties<CompProperties_FireModes>(),
                    curWeaponDef.GetCompProperties<CompProperties_AmmoUser>(),
                    curWeaponDef.Verbs.FirstOrDefault());
            });

            //control pannel
            DrawControlPannel(listing);

            listing.End();
        }

        public static void DrawWeaponTags(Listing_Standard listing, List<string> weaponTags)
        {
            if (weaponTags == null)
                return;

            listing.GapLine(6f);

            listing.Label("<b>" + "MP_WeaponTags".Translate() + "</b>");

            //isOneHand
            bool isOnehand = WeaponTagsSaveable.GetOneHandWeapon(curWeaponDef);
            listing.FieldLineOnChange(CE_StatDefOf.OneHandedness.LabelCap, ref isOnehand, (x) =>
            {
                manager.GetWeaponPatch(curWeaponDef);
                WeaponTagsSaveable.SetOneHandWeapon(curWeaponDef, x);
            }, indent: 20f);

            //bipod
            BipodCategoryDef bipodCategoryDef = WeaponTagsSaveable.GetBipod(curWeaponDef);
            listing.ButtonTextLine(CE_StatDefOf.BipodStats.LabelCap, WeaponTagsSaveable.GetBipod(curWeaponDef)?.label ?? "MP_Null".Translate(), () =>
            {
                List<BipodCategoryDef> list = new List<BipodCategoryDef>();

                list.Add(null);
                list.AddRange(MP_Options.bipodCategoryDefs);

                FloatMenuUtility.MakeMenu(list,
                    (BipodCategoryDef x) => x?.label ?? "MP_Null".Translate(),
                    (BipodCategoryDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        WeaponTagsSaveable.SetBipod(curWeaponDef, x);
                    });
            }, indent: 20f);

        }

        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect resetAllRect = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(resetAllRect, "MP_ResetAll".Translate()))
            {
                manager.ResetAll();
            }

            Rect exportCEPatchRect = rect.RightPartPixels(120f);
            if (Widgets.ButtonText(exportCEPatchRect, "MP_Export".Translate()))
            {
                //patchManager.ExportAll();
                Mod_CEManualPatcher.settings.ExportPatch();
            }
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //sign
            Rect rect0 = rect.LeftPartPixels(3f);

            if (CEPatchManager.instance.HasPatcher(curWeaponDef))
            {
                Widgets.DrawBoxSolid(rect0, Color.blue);
            }
            else if (manager.HasWeaponPatch(curWeaponDef))
            {
                Widgets.DrawBoxSolid(rect0, new Color(85f / 256f, 177f / 256f, 85f / 256f));
            }

            //icon
            Rect rect1 = rect0.RightAdjoin(30f, 0);
            Widgets.DrawTextureFitted(rect1, curWeaponDef.uiIcon, 0.7f);
            //Widgets.ThingIcon(rect1, curWeaponDef);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, curWeaponDef.label);
            Text.Font = GameFont.Small;

            //reset button
            Rect rect3 = rect2.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                manager.Reset(curWeaponDef);
            }

            //CE patch button
            Rect rect4 = rect3.LeftAdjoin(rect.height);

            if (!IsCECompactied(curWeaponDef))
            {
                if (Widgets.ButtonImage(rect4, MP_Texture.CEPatch))
                {
                    Find.WindowStack.Add(new Dialog_MakeCEPatch(curWeaponDef));
                }
            }

            //copy button
            Rect rect5 = rect4.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(rect5, TexButton.Copy))
            {
                copiedThing = curWeaponDef;
                //Messages.Message("MP_Copy".Translate(curWeaponDef.defName), MessageTypeDefOf.NeutralEvent);
            }

            //paste button
            Rect rect6 = rect5.LeftAdjoin(rect.height);

            if (copiedThing != null)
            {
                if (Widgets.ButtonImage(rect6, TexButton.Paste))
                {
                    if (copiedThing != null)
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        CopyThing();
                        copiedThing = null;
                        //Messages.Message("MP_Paste".Translate(curWeaponDef.defName), MessageTypeDefOf.NeutralEvent);
                    }
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        private void CopyThing()
        {
            if (copiedThing == null || curWeaponDef == null || copiedThing == curWeaponDef)
            {
                return;
            }

            //stat
            curWeaponDef.statBases = CopyUtility.CopyStats(copiedThing);

            //verb
            if (!curWeaponDef.Verbs.NullOrEmpty() && !copiedThing.Verbs.NullOrEmpty())
            {
                curWeaponDef.Verbs[0] = CopyUtility.CopyVerb(copiedThing);
            }

            //tools
            if (curWeaponDef.tools != null && copiedThing.tools != null)
            {
                curWeaponDef.tools = CopyUtility.CopyTools(copiedThing);
            }

            //weapon tags
            if (curWeaponDef.weaponTags != null && copiedThing.weaponTags != null)
            {
                curWeaponDef.weaponTags.Clear();
                curWeaponDef.weaponTags.AddRange(copiedThing.weaponTags);
            }

            //comps
            //AmmoUser
            if (copiedThing.HasComp<CompAmmoUser>())
            {
                CompProperties_AmmoUser propCopy = curWeaponDef.GetCompProperties<CompProperties_AmmoUser>();
                if (propCopy == null)
                {
                    propCopy = new CompProperties_AmmoUser();
                    curWeaponDef.comps.Add(propCopy);
                }
                CopyUtility.CopyComp(copiedThing.GetCompProperties<CompProperties_AmmoUser>(), propCopy);
            }

            //FireModes
            if (copiedThing.HasComp<CompFireModes>())
            {
                CompProperties_FireModes propCopy = curWeaponDef.GetCompProperties<CompProperties_FireModes>();
                if (propCopy == null)
                {
                    propCopy = new CompProperties_FireModes();
                    curWeaponDef.comps.Add(propCopy);
                }
                CopyUtility.CopyComp(copiedThing.GetCompProperties<CompProperties_FireModes>(), propCopy);
            }


        }

        private bool IsCECompactied(ThingDef thingDef)
        {
            if (!thingDef.Verbs.NullOrEmpty() && !(thingDef.Verbs[0] is VerbPropertiesCE))
            {
                return false;
            }

            if (thingDef.tools != null && thingDef.tools.Any(x => !(x is ToolCE)))
            {
                return false;
            }

            return true;
        }

        public static void DrawVebs(Listing_Standard listing, VerbProperties verb, CompProperties_AmmoUser ammoUser, List<string> weaponTags)
        {
            if (verb == null)
            {
                return;
            }

            listing.GapLine(6f);
            listing.Label("<b>" + "MP_Verb".Translate() + "</b>");

            VerbPropertiesCE props = verb as VerbPropertiesCE;

            if (props == null)
            {
                listing.Label("Not CE compatiable, Verb is " + verb.GetType().ToString() + " VerbClass is " + verb.verbClass.ToString());
                return;
            }

            //verbClass
            listing.ButtonX("MP_VerbClass".Translate(), 250f, props.verbClass.ToString(), () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.VerbClasses,
                    (Type x) => x.ToString(),
                    (Type x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        props.verbClass = x;
                    });
            }, indent: 20f);

            listing.ButtonX("MP_RecoilPattern".Translate(), 100f, props.recoilPattern.ToString(), () =>
            {
                List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                FloatMenuUtility.MakeMenu(list,
                    (RecoilPattern x) => x.ToString(),
                    (RecoilPattern x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        props.recoilPattern = x;
                    });

            }, indent: 20f);

            //if (props.defaultProjectile != null)
            listing.ButtonX("MP_DefaultProjectile".Translate(), 250f, props.defaultProjectile?.LabelCap ?? "null", () =>
            {
                manager.GetWeaponPatch(curWeaponDef);
                Dialog_SetDefaultProjectile dialog = new Dialog_SetDefaultProjectile(props, ammoUser);
                Find.WindowStack.Add(dialog);
            }, indent: 20f);

            //Sounds
            listing.ButtonX("MP_SoundCast".Translate(), 150f, props.soundCast?.defName ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.soundCast,
                    (SoundDef x) => x.defName,
                    (SoundDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        props.soundCast = x;
                    });
            }, indent: 20f);

            listing.ButtonX("MP_SoundCastTail".Translate(), 150f, props.soundCastTail?.defName ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.soundCastTail,
                    (SoundDef x) => x.defName,
                    (SoundDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        props.soundCastTail = x;
                    });
            }, indent: 20f);

            foreach (var item in VerbPropertiesCESaveable.propNames)
            {
                listing.FieldLineReflexion($"MP_Verbproperties.{item}".Translate(), item, props, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef);
                }, indent: 20f);
            }
        }
        public static void DrawTools(Listing_Standard listing, List<Tool> tools)
        {
            if (tools.NullOrEmpty())
            {
                return;
            }

            listing.GapLine(6f);

            //head
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect addRect = rect.RightPartPixels(rect.height);
            Widgets.Label(rect, "<b>" + "MP_Tools".Translate() + "</b>");
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                manager.GetWeaponPatch(curWeaponDef);
                Find.WindowStack.Add(new Dialog_AddTool(tools));
            }

            foreach (var item in tools)
            {
                listing.Gap(6f);

                Rect subRect = listing.GetRect(Text.LineHeight);

                //label
                WidgetsUtility.LabelChange(subRect, ref item.label, item.GetHashCode(), onClick: () =>
                {
                    manager.GetWeaponPatch(curWeaponDef);
                });

                if (!(item is ToolCE))
                {
                    listing.Label("Not CE compatiable, tool class is " + item.GetType().ToString());
                    continue;
                }

                ToolCE toolCE = item as ToolCE;

                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    manager.GetWeaponPatch(curWeaponDef);
                    tools.Remove(item);
                    return;
                }

                string idString = $"id: {item.id}";
                Rect idRect = removeRect.LeftAdjoin(Text.CalcSize(idString).x);
                Widgets.Label(idRect, idString);

                //内容
                DrawToolContent(listing, toolCE);
            }
        }
        public static void DrawToolContent(Listing_Standard listing, ToolCE tool, float indent = 20f)
        {
            if (tool == null)
            {
                return;
            }

            //内容
            DrawCapacities(listing, tool);
            DrawSurpriseAttack(listing, tool);
            DrawExtraMeleeDamage(listing, tool);
            listing.ButtonX("MP_LinkedBodyPartsGroup".Translate(), 100f, tool.linkedBodyPartsGroup?.label ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.bodyPartGroupDefs,
                    (BodyPartGroupDef x) => $"{x.LabelCap} ({x.defName})",
                    (BodyPartGroupDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        tool.linkedBodyPartsGroup = x;
                        Messages.Message(x.defName, MessageTypeDefOf.SilentInput);
                    });
            }, indent: indent);

            foreach (var fieldName in ToolCESaveable.propNames)
            {
                listing.FieldLineReflexion($"MP_Tools.{fieldName}".Translate(), fieldName, tool, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef);
                }, indent: indent);
            }
        }
        private static void DrawCapacities(Listing_Standard listing, ToolCE tool)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;
            Widgets.Label(rect, "MP_Capacities".Translate());

            Widgets.DrawHighlightIfMouseover(rect);

            //添加按钮
            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(MP_Options.toolCapacityDefs.Except(tool.capacities),
                    (ToolCapacityDef x) => x.label,
                    (ToolCapacityDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                        tool.capacities.Add(x);
                    });
            }

            foreach (var item in tool.capacities)
            {
                Rect subRect = listing.GetRect(Text.LineHeight);
                subRect.x += 40f;
                subRect.width -= 40f;

                Widgets.Label(subRect, item.label);
                Widgets.DrawHighlightIfMouseover(subRect);
                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    manager.GetWeaponPatch(curWeaponDef);
                    tool.capacities.Remove(item);
                    break;
                }
            }
        }
        private static void DrawSurpriseAttack(Listing_Standard listing, ToolCE tool)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;
            Widgets.DrawHighlightIfMouseover(rect);

            Widgets.Label(rect, "MP_SurpriseAttackDamage".Translate());

            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(MP_Options.allDamageDefs,
                    (DamageDef x) => x.label,
                    (DamageDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);

                        if (tool.surpriseAttack == null)
                        {
                            tool.surpriseAttack = new SurpriseAttackProps();
                            tool.surpriseAttack.extraMeleeDamages = new List<ExtraDamage>();
                        }

                        tool.surpriseAttack.extraMeleeDamages.Add(new ExtraDamage()
                        {
                            def = x,
                            amount = 1,
                        });
                    });
            }

            if (tool.surpriseAttack == null)
            {
                return;
            }

            foreach (var item in tool.surpriseAttack.extraMeleeDamages)
            {
                if (DrawExtraDamageRow(listing, item, indent: 40f))
                {
                    manager.GetWeaponPatch(curWeaponDef);
                    tool.surpriseAttack.extraMeleeDamages.Remove(item);
                    return;
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        private static void DrawExtraMeleeDamage(Listing_Standard listing, ToolCE tool)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;
            Widgets.DrawHighlightIfMouseover(rect);


            Widgets.Label(rect, "MP_ExtraMeleeDamage".Translate());

            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(MP_Options.allDamageDefs,
                    (DamageDef x) => x.label,
                    (DamageDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef);

                        if (tool.extraMeleeDamages == null)
                        {
                            tool.extraMeleeDamages = new List<ExtraDamage>();
                        }

                        tool.extraMeleeDamages.Add(new ExtraDamage()
                        {
                            def = x,
                            amount = 1,
                        });
                    });
            }

            if (tool.extraMeleeDamages == null)
            {
                return;
            }

            foreach (var item in tool.extraMeleeDamages)
            {
                if (DrawExtraDamageRow(listing, item, indent: 40f))
                {
                    manager.GetWeaponPatch(curWeaponDef);
                    tool.extraMeleeDamages.Remove(item);
                    return;
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        private static bool DrawExtraDamageRow(Listing_Standard listing, ExtraDamage extraDamage, float indent = 20f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, extraDamage.def.LabelCap);

            Widgets.DrawHighlightIfMouseover(rect);

            //删除按钮
            Rect removeRect = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(removeRect, TexButton.Delete))
            {
                return true;
            }

            //chance
            string chanceLabel = "MP_DamageChance".Translate();
            Rect rect1 = removeRect.LeftAdjoin(100f);
            WidgetsUtility.TextFieldOnChange<float>(rect1, ref extraDamage.chance, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef);
            });
            Rect rect2 = rect1.LeftAdjoin(Text.CalcSize(chanceLabel).x);
            Widgets.Label(rect2, chanceLabel);

            //damage amount
            string amountLabel = "MP_DamageAmount".Translate();
            Rect rect3 = rect2.LeftAdjoin(100f);
            WidgetsUtility.TextFieldOnChange<float>(rect3, ref extraDamage.amount, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef);
            });
            Rect rect4 = rect3.LeftAdjoin(Text.CalcSize(amountLabel).x);
            Widgets.Label(rect4, amountLabel);

            listing.Gap(listing.verticalSpacing);

            return false;
        }

        public static void DrawComps(Listing_Standard listing, CompProperties_FireModes fireModes, CompProperties_AmmoUser ammoUser, VerbProperties verb)
        {
            //CompFireModes
            if (fireModes != null)
            {
                listing.GapLine(6f);
                listing.Label("<b>" + "MP_FireModes".Translate() + "</b>");

                listing.ButtonX("MP_AiAimMode".Translate(), 100f, fireModes.aiAimMode.ToString(), () =>
                {
                    List<AimMode> list = Enum.GetValues(typeof(AimMode)).Cast<AimMode>().ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (AimMode x) => x.ToString(),
                        (AimMode x) => delegate ()
                        {
                            manager.GetWeaponPatch(curWeaponDef);
                            fireModes.aiAimMode = x;
                        });
                }, indent: 20f);

                foreach (var item in CompFireModesSaveable.propNames)
                {
                    listing.FieldLineReflexion($"MP_FireModes.{item}".Translate(), item, fireModes, newValue =>
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                    }, indent: 20f);
                }
            }

            //CompAmmoUser
            if (ammoUser != null)
            {
                listing.GapLine(6f);
                listing.Label("<b>" + "MP_AmmoUser".Translate() + "</b>");

                listing.ButtonX("MP_AmmoSet".Translate(), 200f, ammoUser.ammoSet?.LabelCap ?? "null", () =>
                {
                    manager.GetWeaponPatch(curWeaponDef);
                    Find.WindowStack.Add(new Dialog_SetDefaultProjectile(verb as VerbPropertiesCE, ammoUser));
                }, indent: 20f);

                foreach (var item in CompAmmoUserSaveable.propNames)
                {
                    listing.FieldLineReflexion($"MP_AmmoUser.{item}".Translate(), item, ammoUser, newValue =>
                    {
                        manager.GetWeaponPatch(curWeaponDef);
                    }, indent: 20f);
                }
            }
        }
    }
}
