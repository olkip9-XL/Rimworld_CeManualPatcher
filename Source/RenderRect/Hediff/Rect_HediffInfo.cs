using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_HediffInfo : RenderRectBase
    {
        HediffDef curDef => HediffDefManager.curHediffDef;

        HediffDefManager manager => HediffDefManager.instance;

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 0f;

        Action preChange = null;

        public Rect_HediffInfo()
        {
            this.preChange = () =>
            {
                manager.GetPatch(curDef);
            };
        }


        public override void DoWindowContents(Rect rect)
        {
            if (curDef == null)
                return;

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f - 30f), ref scrollPosition, ref scrollViewHeight, (innerListing) =>
            {
                if (curDef.stages.NullOrEmpty() && !curDef.HasComp(typeof(HediffComp_VerbGiver)))
                {
                    innerListing.LabelLine("Nothing to modify");
                    return;
                }

                DrawStages(innerListing);
                DrawVerbGiver(innerListing);
            });

            listing.ControlPannel(manager);

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
            Texture2D icon = curDef.spawnThingOnRemoved?.uiIcon ?? BaseContent.BadTex;

            Widgets.DrawTextureFitted(rect1, icon, 0.7f);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, curDef.label ?? "No Label");
            Text.Font = GameFont.Small;

            //reset button
            Rect rect3 = rect2.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                manager.Reset(curDef);
            }

            listing.Gap(listing.verticalSpacing);
        }

        private void DrawStages(Listing_Standard listing)
        {
            if (curDef.stages.NullOrEmpty())
            {
                return;
            }

            listing.GapLine(6f);

            List<string> propNames = new List<string>()
            {
                "minSeverity",
                "severityGainFactor",
            };


            for (int i = 0; i < curDef.stages.Count; i++)
            {
                var stage = curDef.stages[i];
                if (stage == null)
                {
                    continue;
                }

                listing.LabelLine("MP_Hediff_Stage".Translate(i+1, stage.label));

                foreach (var propName in propNames)
                {
                    listing.FieldLineReflexion($"MP_HediffDef.{propName}".Translate(), propName, stage, (newValue) =>
                    {
                        preChange?.Invoke();
                    }, indent: 20f);
                }

                DrawArmorData(StatDefOf.ArmorRating_Blunt, stage);
                DrawArmorData(StatDefOf.ArmorRating_Sharp, stage);
                DrawArmorData(StatDefOf.ArmorRating_Heat, stage);
            }

            void DrawArmorData(StatDef stat, HediffStage stage)
            {
                string statName = stat.label.CapitalizeFirst();

                float offsets = stage.statOffsets.GetStatOffsetFromList(stat);
                float factors = stage.statFactors.GetStatFactorFromList(stat);

                Rect rect = listing.GetRect(Text.LineHeight);
                rect.width -= 20f;
                rect.x += 20f;

                //label
                Rect labelRect = rect.LeftPartPixels(100f);
                Widgets.Label(labelRect, statName);

                Rect inputRect1 = rect.RightPartPixels(100f);
                WidgetsUtility.TextFieldOnChange(inputRect1, ref offsets, (newValue) =>
                {
                    if (stage.statOffsets == null)
                    {
                        stage.statOffsets = new List<StatModifier>();
                    }
                    preChange?.Invoke();
                    stage.statOffsets.SetStatValueToList(stat, newValue);
                });

                Rect labelRect1 = inputRect1.LeftAdjoin(Text.CalcSize("MP_Hediff_offsset".Translate()).x);
                Widgets.Label(labelRect1, "MP_Hediff_offsset".Translate());

                Rect inputRect2 = labelRect1.LeftAdjoin(100f);
                WidgetsUtility.TextFieldOnChange(inputRect2, ref factors, (newValue) =>
                {
                    if (stage.statFactors == null)
                    {
                        stage.statFactors = new List<StatModifier>();
                    }
                    preChange?.Invoke();
                    stage.statFactors.SetStatValueToList(stat, newValue);
                });

                Rect labelRect2 = inputRect2.LeftAdjoin(Text.CalcSize("MP_Hediff_factor".Translate()).x);
                Widgets.Label(labelRect2, "MP_Hediff_factor".Translate());

                listing.Gap(listing.verticalSpacing);
            }
        }

        private void DrawVerbGiver(Listing_Standard listing)
        {
            if (!curDef.HasComp(typeof(HediffComp_VerbGiver)))
            {
                return;
            }

            listing.GapLine(6f);
            listing.LabelLine("MP_Hediff_VerbGiver".Translate());

            HediffCompProperties_VerbGiver verbGiver = curDef.comps.FirstOrDefault(c => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;

            if (verbGiver.tools.NullOrEmpty())
            {
                listing.LabelLine("No tools defined");
                return;
            }

            if (verbGiver.tools.Any(x => !(x is ToolCE)))
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                Widgets.Label(rect, "Some tools are not ToolCE, please patch them first.");

                Rect buttonRect = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(buttonRect, MP_Texture.CEPatch))
                {
                    preChange?.Invoke();

                    //patch all tools to ToolCE
                    List<Tool> tools = new List<Tool>();
                    foreach (var tool in verbGiver.tools)
                    {
                        ToolCE toolCopy = PropUtility.ConvertToChild<Tool, ToolCE>(tool);

                        List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();
                        capacities.AddRange(tool.capacities);
                        toolCopy.capacities = capacities;

                        tools.Add(toolCopy);
                    }

                    verbGiver.tools = tools;
                }
                return;
            }
            else
            {
                List<string> propNames = new List<string>()
                {
                    "power",
                    "armorPenetrationSharp",
                    "armorPenetrationBlunt",
                };

                foreach (var tool in verbGiver.tools)
                {
                    ToolCE toolCE = tool as ToolCE;

                    foreach (var propName in propNames)
                    {
                        listing.FieldLineReflexion($"MP_Tools.{propName}".Translate(), propName, toolCE, (newValue) =>
                        {
                            preChange?.Invoke();
                        }, indent: 20f);
                    }
                }
            }
        }
    }
}
