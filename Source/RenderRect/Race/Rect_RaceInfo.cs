using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_RaceInfo : RenderRectBase
    {
        RaceManager manager => RaceManager.instance;
        ThingDef curDef => RaceManager.curDef;

        BodyDef BodyDef => curDef?.race?.body;

        //scroll
        private float innerHeight = 0f;
        private Vector2 scrollPosition = Vector2.zero;

        private Action preChange = null;

        public Rect_RaceInfo()
        {
            preChange = delegate
            {
                manager.GetPatch(curDef);
            };
        }

        public override void DoWindowContents(Rect rect)
        {
            if (curDef == null)
            {
                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 30f - 0.1f), ref scrollPosition, ref innerHeight, (innerListing) =>
            {
                RenderRectUtility.DrawStats(innerListing, ref curDef.statBases, MP_Options.statDefs_Race, preChange);

                DrawComp_ArmorDurability(innerListing);

                Rect_WeaponInfo.DrawTools(innerListing, curDef.tools, preChange, BodyDef);

                Rect_ApparelInfo.DrawPartialArmorExt(innerListing, curDef, preChange, BodyDef);
            });

            //control pannel
            DrawControlPannel(listing);

            listing.End();
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //sign
            Rect rect0 = rect.LeftPartPixels(3f);
            if (manager.HasPatch(curDef))
            {
                Widgets.DrawBoxSolid(rect0, MP_Color.SignGreen);
            }

            //icon
            Rect rect1 = rect0.RightAdjoin(30f, 0);
            Widgets.DrawTextureFitted(rect1, curDef.uiIcon, 0.7f);

            //label
            Text.Font = GameFont.Medium;
            string defLabel = curDef.label ?? "No Label";
            Rect rect2 = rect1.RightAdjoin(Text.CalcSize(defLabel).x);
            Widgets.Label(rect2, defLabel);
            Text.Font = GameFont.Small;

            //hyperlink
            string bodyDefLabel = BodyDef?.LabelCap ?? "No Body Label";
            Rect hyperLinkRect = rect2.RightAdjoin(Text.CalcSize(bodyDefLabel).x);
            if (Widgets.ButtonText(hyperLinkRect, bodyDefLabel, drawBackground: false, doMouseoverSound: false, textColor: Widgets.NormalOptionColor, overrideTextAnchor: TextAnchor.LowerLeft))
            {
                Mod_CEManualPatcher.instance.SetCurTab(MP_SettingTab.Body);
                BodyDefManager.instance.curBody = curDef.race?.body;
            }

            //reset button
            Rect rect3 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                manager.Reset(curDef);
            }

            //CE patch button
            Rect rect4 = rect3.LeftAdjoin(rect.height);
            if (!IsCECompactied(curDef))
            {
                if (Widgets.ButtonImage(rect4, MP_Texture.CEPatch))
                {
                    preChange?.Invoke();
                    PatchRace();
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        private bool IsCECompactied(ThingDef thingDef)
        {
            if (thingDef.tools != null && thingDef.tools.Any(x => !(x is ToolCE)))
            {
                return false;
            }

            return true;
        }

        private void PatchRace()
        {
            ThingDef thingDef = curDef;

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

        }

        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            Rect resetAllRect = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(resetAllRect, "MP_ResetAll".Translate()))
            {
                manager.ResetAll();
            }

            Rect exportCEPatchRect = rect.RightPartPixels(120f);
            if (Widgets.ButtonText(exportCEPatchRect, "MP_Export".Translate()))
            {
                Mod_CEManualPatcher.settings.ExportPatch();
            }
        }

        private void DrawComp_ArmorDurability(Listing_Standard listing)
        {
            CompProperties_ArmorDurability comp = curDef.GetCompProperties<CompProperties_ArmorDurability>();

            listing.DrawComp("MP_ArmorDurability".Translate(), (innerListing) =>
            {
                foreach (var name in CompArmorDurabilitySaveable.propNames)
                {
                    innerListing.FieldLineReflexion($"MP_ArmorDurability.{name}".Translate(), name, comp, (newValue) =>
                    {
                        preChange?.Invoke();
                    });
                }

                if (comp.RepairIngredients == null)
                {
                    comp.RepairIngredients = new List<ThingDefCountClass>();
                }

                innerListing.ButtonImageLine("MP_RepairIngredients".Translate(), TexButton.Add, () =>
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (var def in MP_Options.ingredientsForRepairArmor)
                    {
                        options.Add(new FloatMenuOption(def.LabelCap, () =>
                        {
                            if (!comp.RepairIngredients.Any(x => x.thingDef == def))
                            {
                                preChange?.Invoke();
                                comp.RepairIngredients.Add(new ThingDefCountClass
                                {
                                    thingDef = def,
                                    count = 1
                                });
                            }
                        }, null, def.uiIcon));
                    }
                    if (options.Any())
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }

                });

                foreach (var item in comp.RepairIngredients)
                {
                    bool needBreak = false;
                    innerListing.ThingDefCountClassLine(item, () =>
                    {
                        comp.RepairIngredients.Remove(item);
                        needBreak = true;
                    }, preChange, indent: 20f);

                    if (needBreak)
                    {
                        break;
                    }
                }
            }, comp?.GetHashCode() ?? 0, () =>
            {
                preChange?.Invoke();
                curDef.comps.Add(new CompProperties_ArmorDurability());
            });

        }
    }
}
