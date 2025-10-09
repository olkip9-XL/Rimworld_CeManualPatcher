using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Patch;
using CeManualPatcher.Saveable;
using CombatExtended;
using CombatExtended.Compatibility;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_WeaponInfo : RenderRectBase
    {
        //scroll
        private float innerHeight = 0f;
        private Vector2 scrollPosition = Vector2.zero;

        //copy
        private ThingDef copiedThing = null;

        private static WeaponManager manager => WeaponManager.instance;
        //private static CEPatchManager patchManager => CEPatchManager.instance;
        private static ThingDef curWeaponDef => WeaponManager.curWeaponDef;

        private Action preChange = null;

        public Rect_WeaponInfo()
        {
            preChange = delegate
            {
                manager.GetPatch(curWeaponDef);
            };
        }

        public override void DoWindowContents(Rect rect)
        {
            if (curWeaponDef == null)
            {
                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 30f - 0.1f), ref scrollPosition, ref innerHeight, (innerListing) =>
            {
                RenderRectUtility.DrawStats(innerListing, ref curWeaponDef.statBases, MP_Options.statDefs_Weapon, preChange);

                RenderRectUtility.DrawStats(innerListing, ref curWeaponDef.equippedStatOffsets, MP_Options.statDefs_WeaponOffset, preChange, headLabel: "MP_StatOffset".Translate());

                DrawWeaponTags(innerListing, curWeaponDef.weaponTags, preChange);

                DrawVebs(innerListing, curWeaponDef.Verbs.FirstOrDefault(), curWeaponDef.GetCompProperties<CompProperties_AmmoUser>(), preChange);
                DrawTools(innerListing, curWeaponDef.tools, preChange);

                DrawComp_Firemodes(innerListing);

                DrawComp_Ammouser(innerListing);

                DrawComp_ChargeSpeed(innerListing);
            });

            //control pannel
            DrawControlPannel(listing);

            listing.End();
        }

        public static void DrawWeaponTags(Listing_Standard listing, List<string> weaponTags, Action preChange)
        {
            if (weaponTags == null)
                return;

            listing.GapLine(6f);

            listing.Label("<b>" + "MP_WeaponTags".Translate() + "</b>");

            //isOneHand
            bool isOnehand = WeaponTagsSaveable.GetOneHandWeapon(weaponTags);
            listing.FieldLineOnChange(CE_StatDefOf.OneHandedness.LabelCap, ref isOnehand, (x) =>
            {
                preChange?.Invoke();
                WeaponTagsSaveable.SetOneHandWeapon(weaponTags, x);
            }, indent: 20f);

            //bipod
            BipodCategoryDef bipodCategoryDef = WeaponTagsSaveable.GetBipod(weaponTags);
            listing.ButtonTextLine(CE_StatDefOf.BipodStats.LabelCap, WeaponTagsSaveable.GetBipod(weaponTags)?.label ?? "MP_Null".Translate(), () =>
            {
                List<BipodCategoryDef> list = new List<BipodCategoryDef>();

                list.Add(null);
                list.AddRange(MP_Options.bipodCategoryDefs);

                FloatMenuUtility.MakeMenu(list,
                    (BipodCategoryDef x) => x?.label ?? "MP_Null".Translate(),
                    (BipodCategoryDef x) => delegate ()
                    {
                        preChange?.Invoke();
                        WeaponTagsSaveable.SetBipod(weaponTags, x);
                    });
            }, indent: 20f);

            //combat role
            string roleTag = WeaponTagsSaveable.GetCombatRole(weaponTags);
            listing.ButtonTextLine("MP_CombatRole".Translate(), roleTag ?? "MP_Null".Translate(), () =>
            {
                List<string> list = new List<string>();
                list.Add(null);
                list.AddRange(WeaponTagsSaveable.combatRoleTags);

                FloatMenuUtility.MakeMenu(list,
                    (string x) => x ?? "MP_Null".Translate(),
                    (string x) => delegate ()
                    {
                        preChange?.Invoke();
                        WeaponTagsSaveable.SetCombatRole(weaponTags, x);
                    });
            }, indent: 20f);

            //All weaponTags
            listing.ButtonImageLine("MP_WeaponTags".Translate().Colorize(MP_Color.Grey), TexButton.Add, () =>
            {
                List<string> tags = new List<string>(MP_Options.weaponTags);
                tags = tags.Except(weaponTags).ToList();

                FloatMenuUtility.MakeMenu(tags,
                    (string x) => x,
                    (string x) => delegate ()
                    {
                        preChange?.Invoke();
                        weaponTags.Add(x);
                    });
            }, indent: 20f);

            foreach (var item in weaponTags)
            {
                bool needBreak = false;
                listing.ButtonImageLine(item.Colorize(MP_Color.Grey), TexButton.Delete, () =>
                {
                    preChange?.Invoke();
                    weaponTags.Remove(item);
                    needBreak = true;
                }, indent: 40f);
                if (needBreak)
                {
                    break;
                }
            }

        }

        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            Rect resetAllRect = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(resetAllRect, "MP_ResetAll".Translate()))
            {
                manager.ResetAll();
                //patchManager.ResetAll();
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
            if (manager.HasPatch(curWeaponDef))
            {
                Widgets.DrawBoxSolid(rect0, MP_Color.SignGreen);
            }

            //icon
            Rect rect1 = rect0.RightAdjoin(30f, 0);
            Widgets.DrawTextureFitted(rect1, curWeaponDef.uiIcon, 0.7f);
            //Widgets.ThingIcon(rect1, curWeaponDef);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, curWeaponDef.label ?? "No Label");
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
                    //Find.WindowStack.Add(new Dialog_MakeCEPatch(curWeaponDef));

                    //Patch fuction
                    preChange?.Invoke();

                    PatchWeapon();
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
                        manager.GetPatch(curWeaponDef);
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
            curWeaponDef.statBases = CopyUtility.CopyStats(copiedThing.statBases);

            //stat Offset
            curWeaponDef.equippedStatOffsets = CopyUtility.CopyStats(copiedThing.equippedStatOffsets);

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
            if (copiedThing.HasComp<CompAmmoUser>() && curWeaponDef.HasComp<CompAmmoUser>())
            {
                //CompProperties_AmmoUser propCopy = curWeaponDef.GetCompProperties<CompProperties_AmmoUser>();
                //if (propCopy == null)
                //{
                //    propCopy = new CompProperties_AmmoUser();
                //    curWeaponDef.comps.Add(propCopy);
                //}
                CopyUtility.CopyComp(
                    copiedThing.GetCompProperties<CompProperties_AmmoUser>(),
                    curWeaponDef.GetCompProperties<CompProperties_AmmoUser>());
            }

            //FireModes
            if (copiedThing.HasComp<CompFireModes>() && curWeaponDef.HasComp<CompFireModes>())
            {
                //CompProperties_FireModes propCopy = curWeaponDef.GetCompProperties<CompProperties_FireModes>();
                //if (propCopy == null)
                //{
                //    propCopy = new CompProperties_FireModes();
                //    curWeaponDef.comps.Add(propCopy);
                //}
                //CopyUtility.CopyComp(copiedThing.GetCompProperties<CompProperties_FireModes>(), propCopy);

                CopyUtility.CopyComp(
                   copiedThing.GetCompProperties<CompProperties_FireModes>(),
                   curWeaponDef.GetCompProperties<CompProperties_FireModes>());

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

        private void PatchWeapon()
        {
            ThingDef thingDef = curWeaponDef;

            //statBases
            if (thingDef.statBases != null)
            {
                List<StatModifier> stats = new List<StatModifier>();
                foreach (var stat in thingDef.statBases)
                {
                    if (MP_Options.exceptStatDefs.Contains(stat.stat))
                    {
                        continue;
                    }

                    StatModifier statModifier = new StatModifier();
                    PropUtility.CopyPropValue(stat, statModifier);
                    stats.Add(statModifier);
                }

                // Add CE stats, default AR
                stats.Add(new StatModifier() { stat = CE_StatDefOf.Bulk, value = 10.03f });

                if (!thingDef.Verbs.NullOrEmpty())
                {
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.TicksBetweenBurstShots, value = 4 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.BurstShotCount, value = 6 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SightsEfficiency, value = 1 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SwayFactor, value = 1.33f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ShotSpread, value = 0.07f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.Recoil, value = 1.5f });

                    stats.Add(new StatModifier() { stat = CE_StatDefOf.MagazineCapacity, value = 30f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ReloadSpeed, value = 4f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.AmmoGenPerMagOverride });
                }

                thingDef.statBases = stats;
            }

            //statOffsets stay the same

            //weaponTags stay the same

            //verbs
            if (!thingDef.Verbs.NullOrEmpty())
            {
                VerbPropertiesCE verb = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(thingDef.Verbs[0]);

                if (verb.verbClass == typeof(Verb_Shoot))
                    verb.verbClass = typeof(Verb_ShootCE);

                if (verb.verbClass == typeof(Verb_ShootOneUse))
                    verb.verbClass = typeof(Verb_ShootCEOneUse);

                if (verb.verbClass == typeof(Verb_LaunchProjectile))
                    verb.verbClass = typeof(Verb_LaunchProjectileCE);

                verb.defaultProjectile = MP_ProjectileDefOf.Bullet_556x45mmNATO_FMJ;

                thingDef.Verbs.Clear();
                thingDef.Verbs.Add(verb);
            }

            //tools
            if (thingDef.tools != null)
            {
                List<Tool> tools = new List<Tool>();
                foreach (var tool in thingDef.tools)
                {
                    ToolCE toolCopy = PropUtility.ConvertToChild<Tool, ToolCE>(tool);

                    List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();
                    capacities.AddRange(tool.capacities);
                    toolCopy.capacities = capacities;

                    tools.Add(toolCopy);
                }

                thingDef.tools = tools;
            }

            //comps
            if (thingDef.comps != null && !thingDef.Verbs.NullOrEmpty())
            {
                //AmmoUser
                if (!thingDef.HasComp<CompAmmoUser>())
                {
                    CompProperties_AmmoUser ammoUser = new CompProperties_AmmoUser();
                    ammoUser.ammoSet = MP_AmmoSetDefOf.AmmoSet_556x45mmNATO;
                    thingDef.comps.Add(ammoUser);
                }

                //fireModes
                if (!thingDef.HasComp<CompFireModes>())
                {
                    CompProperties_FireModes fireModes = new CompProperties_FireModes();
                    fireModes.aimedBurstShotCount = 3;
                    thingDef.comps.Add(fireModes);
                }

                //charges stay the same
            }
        }

        public static void DrawVebs(Listing_Standard listing, VerbProperties verb, CompProperties_AmmoUser ammoUser, Action preChange)
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
            listing.ButtonTextLine("MP_VerbClass".Translate(), props.verbClass.ToString(), () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.VerbClasses,
                    (Type x) => x.ToString(),
                    (Type x) => delegate ()
                    {
                        preChange?.Invoke();
                        props.verbClass = x;
                    });
            }, indent: 20f);

            listing.ButtonTextLine("MP_RecoilPattern".Translate(), props.recoilPattern.ToString(), () =>
            {
                List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                FloatMenuUtility.MakeMenu(list,
                    (RecoilPattern x) => x.ToString(),
                    (RecoilPattern x) => delegate ()
                    {
                        preChange?.Invoke();
                        props.recoilPattern = x;
                    });

            }, indent: 20f);

            //if (props.defaultProjectile != null)
            listing.ButtonTextLine("MP_DefaultProjectile".Translate(), props.defaultProjectile?.LabelCap ?? "null", () =>
            {
                preChange?.Invoke();
                Dialog_SetDefaultProjectile dialog = new Dialog_SetDefaultProjectile(props, ammoUser);
                Find.WindowStack.Add(dialog);
            }, indent: 20f);

            //Sounds
            listing.ButtonTextLine("MP_SoundCast".Translate(), props.soundCast?.defName ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.soundCast,
                    (SoundDef x) => x.defName,
                    (SoundDef x) => delegate ()
                    {
                        preChange?.Invoke();
                        props.soundCast = x;
                    });
            }, indent: 20f);

            listing.ButtonTextLine("MP_SoundCastTail".Translate(), props.soundCastTail?.defName ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.soundCastTail,
                    (SoundDef x) => x.defName,
                    (SoundDef x) => delegate ()
                    {
                        preChange?.Invoke();
                        props.soundCastTail = x;
                    });
            }, indent: 20f);

            foreach (var item in VerbPropertiesCESaveable.propNames)
            {
                listing.FieldLineReflexion($"MP_Verbproperties.{item}".Translate(), item, props, newValue =>
                {
                    preChange?.Invoke();
                }, indent: 20f);
            }

            DrawTargetingParameters(listing, props, preChange);
        }

        private static void DrawTargetingParameters(Listing_Standard listing, VerbProperties verb, Action preChange)
        {
            TargetingParameters parameters = verb.targetParams;
            if (parameters == null)
            {
                return;
            }

            listing.CollaspseField(innerListing =>
            {
                foreach (string fieldName in TargetingParametersSaveable.propNames)
                {
                    innerListing.FieldLineReflexion($"MP_TargetingParameters.{fieldName}".Translate(), fieldName, parameters, newValue =>
                    {
                        preChange?.Invoke();
                    }, indent: 20f);
                }
            }, "<b>" + "MP_TargetingParameters".Translate() + "</b>", parameters.GetHashCode(), 20f);

            listing.Gap(listing.verticalSpacing);
        }
        public static void DrawTools(Listing_Standard listing, List<Tool> tools, Action preChange)
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
                preChange?.Invoke();
                Find.WindowStack.Add(new Dialog_AddTool(tools));
            }

            foreach (var item in tools)
            {
                listing.Gap(6f);

                Rect subRect = listing.GetRect(Text.LineHeight);

                //label
                WidgetsUtility.LabelChange(subRect, ref item.label, item.GetHashCode(), onClick: () =>
                {
                    preChange?.Invoke();
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
                    preChange?.Invoke();
                    tools.Remove(item);
                    return;
                }

                string idString = $"id: {item.id}";
                Rect idRect = removeRect.LeftAdjoin(Text.CalcSize(idString).x);
                Widgets.Label(idRect, idString);

                //内容
                DrawToolContent(listing, toolCE, preChange);
            }
        }
        public static void DrawToolContent(Listing_Standard listing, ToolCE tool, Action preChange, float indent = 20f)
        {
            if (tool == null)
            {
                return;
            }

            //内容
            DrawCapacities(listing, tool, preChange);
            DrawSurpriseAttack(listing, tool, preChange);
            DrawExtraMeleeDamage(listing, tool, preChange);
            listing.ButtonTextLine("MP_LinkedBodyPartsGroup".Translate(), tool.linkedBodyPartsGroup?.label ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu(MP_Options.bodyPartGroupDefs,
                    (BodyPartGroupDef x) => $"{x.LabelCap} ({x.defName})",
                    (BodyPartGroupDef x) => delegate ()
                    {
                        preChange?.Invoke();
                        tool.linkedBodyPartsGroup = x;
                        Messages.Message(x.defName, MessageTypeDefOf.SilentInput);
                    });
            }, indent: indent);

            foreach (var fieldName in ToolCESaveable.propNames)
            {
                listing.FieldLineReflexion($"MP_Tools.{fieldName}".Translate(), fieldName, tool, newValue =>
                {
                    preChange?.Invoke();
                }, indent: indent);
            }
        }
        private static void DrawCapacities(Listing_Standard listing, ToolCE tool, Action preChange)
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
                        preChange?.Invoke();
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
                    preChange?.Invoke();
                    tool.capacities.Remove(item);
                    break;
                }
            }
        }
        private static void DrawSurpriseAttack(Listing_Standard listing, ToolCE tool, Action preChange)
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
                        preChange?.Invoke();

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

            if (tool.surpriseAttack != null)
            {
                foreach (var item in tool.surpriseAttack.extraMeleeDamages)
                {
                    if (DrawExtraDamageRow(listing, item, preChange, indent: 40f))
                    {
                        preChange?.Invoke();
                        tool.surpriseAttack.extraMeleeDamages.Remove(item);
                        return;
                    }
                }
            }

            listing.Gap(listing.verticalSpacing);
        }
        private static void DrawExtraMeleeDamage(Listing_Standard listing, ToolCE tool, Action preChange)
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
                        preChange?.Invoke();

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
                if (DrawExtraDamageRow(listing, item, preChange, indent: 40f))
                {
                    preChange?.Invoke();
                    tool.extraMeleeDamages.Remove(item);
                    return;
                }
            }

            listing.Gap(listing.verticalSpacing);
        }
        private static bool DrawExtraDamageRow(Listing_Standard listing, ExtraDamage extraDamage, Action preChange, float indent = 20f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, extraDamage.def?.LabelCap ?? "null");

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
                preChange?.Invoke();
            });
            Rect rect2 = rect1.LeftAdjoin(Text.CalcSize(chanceLabel).x);
            Widgets.Label(rect2, chanceLabel);

            //damage amount
            string amountLabel = "MP_DamageAmount".Translate();
            Rect rect3 = rect2.LeftAdjoin(100f);
            WidgetsUtility.TextFieldOnChange<float>(rect3, ref extraDamage.amount, newValue =>
            {
                preChange?.Invoke();
            });
            Rect rect4 = rect3.LeftAdjoin(Text.CalcSize(amountLabel).x);
            Widgets.Label(rect4, amountLabel);

            listing.Gap(listing.verticalSpacing);

            return false;
        }
        public static void DrawComps(Listing_Standard listing, CompProperties_FireModes fireModes, CompProperties_AmmoUser ammoUser, VerbProperties verb, Action preChange)
        {
            //CompFireModes
            if (fireModes != null)
            {
                listing.DrawComp("MP_FireModes".Translate(), (innerListing) =>
                {
                    innerListing.ButtonTextLine("MP_AiAimMode".Translate(), fireModes.aiAimMode.ToString(), () =>
                    {
                        List<AimMode> list = Enum.GetValues(typeof(AimMode)).Cast<AimMode>().ToList();
                        FloatMenuUtility.MakeMenu(list,
                            (AimMode x) => x.ToString(),
                            (AimMode x) => delegate ()
                            {
                                preChange?.Invoke();
                                fireModes.aiAimMode = x;
                            });
                    });

                    foreach (var item in CompFireModesSaveable.propNames)
                    {
                        innerListing.FieldLineReflexion($"MP_FireModes.{item}".Translate(), item, fireModes, newValue =>
                        {
                            preChange?.Invoke();
                        });
                    }
                }, fireModes.GetHashCode());
            }

            //CompAmmoUser
            if (ammoUser != null)
            {
                Color fontColor = ammoUser.ammoSet == null ? MP_Color.Grey : Color.white;

                listing.DrawComp("MP_AmmoUser".Translate(), (innerListing) =>
                {
                    innerListing.ButtonTextLine("MP_AmmoSet".Translate().Colorize(fontColor), ammoUser.ammoSet?.LabelCap ?? "null", () =>
                    {
                        preChange?.Invoke();
                        Find.WindowStack.Add(new Dialog_SetDefaultProjectile(verb as VerbPropertiesCE, ammoUser));
                    });

                    foreach (var item in CompAmmoUserSaveable.propNames)
                    {
                        innerListing.FieldLineReflexion($"MP_AmmoUser.{item}".Translate().Colorize(fontColor), item, ammoUser, newValue =>
                        {
                            preChange?.Invoke();
                        });
                    }
                }, ammoUser.GetHashCode());
            }
        }

        public void DrawComp_ChargeSpeed(Listing_Standard listing)
        {
            CompProperties_Charges compProps = curWeaponDef.GetCompProperties<CompProperties_Charges>();

            listing.DrawComp("MP_Charges".Translate(), (innerListing) =>
            {
                List<int> chargeSpeeds = compProps.chargeSpeeds;

                Color fontColor = chargeSpeeds.NullOrEmpty() ? MP_Color.Grey : Color.white;

                //add
                innerListing.ButtonImageLine("MP_ChargeSpeed".Translate().Colorize(fontColor), TexButton.Add, () =>
                {
                    preChange?.Invoke();
                    compProps.chargeSpeeds.Add(1);
                });

                for (int i = 0; i < chargeSpeeds.Count; i++)
                {
                    Rect rect = innerListing.GetRect(Text.LineHeight);

                    //number
                    Rect rect1 = rect.LeftPartPixels(100f);
                    int tempItem = chargeSpeeds[i];
                    WidgetsUtility.TextFieldOnChange<int>(rect1, ref tempItem, newValue =>
                    {
                        preChange?.Invoke();
                        chargeSpeeds[i] = newValue;
                    });

                    //delete
                    Rect rect2 = rect.RightPartPixels(rect.height);

                    if (Widgets.ButtonImage(rect2, TexButton.Delete))
                    {
                        preChange?.Invoke();
                        compProps.chargeSpeeds.RemoveAt(i);
                        break;
                    }

                }
            }, compProps?.GetHashCode() ?? 0, () =>
            {
                preChange?.Invoke();
                curWeaponDef.comps.Add(new CompProperties_Charges()
                {
                    chargeSpeeds = new List<int>()
                });
            });
        }
        //1.6 improve
        private void DrawComp_Firemodes(Listing_Standard listing)
        {
            CompProperties_FireModes compProps = curWeaponDef.GetCompProperties<CompProperties_FireModes>();

            listing.DrawComp("MP_FireModes".Translate(), (innerListing) =>
            {
                innerListing.ButtonTextLine("MP_AiAimMode".Translate(), compProps.aiAimMode.ToString(), () =>
                {
                    List<AimMode> list = Enum.GetValues(typeof(AimMode)).Cast<AimMode>().ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (AimMode x) => x.ToString(),
                        (AimMode x) => delegate ()
                        {
                            preChange?.Invoke();
                            compProps.aiAimMode = x;
                        });
                });

                foreach (var item in CompFireModesSaveable.propNames)
                {
                    innerListing.FieldLineReflexion($"MP_FireModes.{item}".Translate(), item, compProps, newValue =>
                    {
                        preChange?.Invoke();
                    });
                }

            }, compProps?.GetHashCode() ?? 0, () =>
            {
                preChange?.Invoke();

                curWeaponDef.comps.Add(new CompProperties_FireModes());
            });
        }

        private void DrawComp_Ammouser(Listing_Standard listing)
        {
            CompProperties_AmmoUser compProps = curWeaponDef.GetCompProperties<CompProperties_AmmoUser>();

            VerbPropertiesCE verb = curWeaponDef.Verbs.FirstOrDefault() as VerbPropertiesCE;

            listing.DrawComp("MP_AmmoUser".Translate(), (innerListing) =>
            {
                Color fontColor = compProps.ammoSet == null ? MP_Color.Grey : Color.white;
                innerListing.ButtonTextLine("MP_AmmoSet".Translate().Colorize(fontColor), compProps.ammoSet?.LabelCap ?? "null", () =>
                {
                    preChange?.Invoke();
                    Find.WindowStack.Add(new Dialog_SetDefaultProjectile(verb, compProps));
                });

                foreach (var item in CompAmmoUserSaveable.propNames)
                {
                    innerListing.FieldLineReflexion($"MP_AmmoUser.{item}".Translate().Colorize(fontColor), item, compProps, newValue =>
                    {
                        preChange?.Invoke();
                    });
                }
            }, compProps?.GetHashCode() ?? 0, () =>
            {
                preChange?.Invoke();
                curWeaponDef.comps.Add(new CompProperties_AmmoUser());
            });
        }
    }
}
