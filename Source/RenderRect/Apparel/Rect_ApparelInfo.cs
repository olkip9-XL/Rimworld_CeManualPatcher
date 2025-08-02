using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_ApparelInfo : RenderRectBase
    {

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 0f;

        private ThingDef copiedThing;

        private ApparelManager manager => ApparelManager.instance;
        private ThingDef curApparel => ApparelManager.curApparelDef;
        public override void DoWindowContents(Rect rect)
        {
            if (curApparel == null)
                return;

            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);

            DrawHead(listing_Standard);

            WidgetsUtility.ScrollView(listing_Standard.GetRect(rect.height - listing_Standard.CurHeight - Text.LineHeight - 0.1f), ref scrollPosition, ref scrollViewHeight, (innerListing) =>
            {
                RenderRectUtility.DrawStats(innerListing, ref curApparel.statBases, MP_Options.statDefs_Apparel, delegate
                {
                    manager.GetPatch(curApparel);
                });

                RenderRectUtility.DrawStats(innerListing, ref curApparel.equippedStatOffsets, MP_Options.statDefs_ApparelOffset, delegate
                {
                    manager.GetPatch(curApparel);
                }, headLabel: "MP_StatOffset".Translate());

                DrawPartialArmorExt(innerListing);
            });

            DrawControlPannel(listing_Standard);

            listing_Standard.End();
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
                Mod_CEManualPatcher.settings.ExportPatch();
            }
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //sign
            Rect rect0 = rect.LeftPartPixels(3f);

            if (manager.HasPatch(curApparel))
            {
                Widgets.DrawBoxSolid(rect0, new Color(85f / 256f, 177f / 256f, 85f / 256f));
            }

            //icon
            Rect rect1 = rect0.RightAdjoin(30f, 0);
            Widgets.DrawTextureFitted(rect1, curApparel.uiIcon, 0.7f);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, curApparel.label);
            Text.Font = GameFont.Small;

            //reset button
            Rect rect3 = rect2.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                manager.Reset(curApparel);
            }

            //CE patch button
            Rect rect4 = rect3.LeftAdjoin(rect.height);

            if (!IsCECompactied(curApparel))
            {
                if (Widgets.ButtonImage(rect4, MP_Texture.CEPatch))
                {
                    manager.GetPatch(curApparel);
                    MakeCEPatch(curApparel);
                }
            }

            //copy button
            Rect rect5 = rect4.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(rect5, TexButton.Copy))
            {
                copiedThing = curApparel;
            }

            //paste button
            Rect rect6 = rect5.LeftAdjoin(rect.height);

            if (copiedThing != null)
            {
                if (Widgets.ButtonImage(rect6, TexButton.Paste))
                {
                    if (copiedThing != null)
                    {
                        manager.GetPatch(curApparel);
                        CopyThing();
                        copiedThing = null;
                    }
                }
            }


            listing.Gap(listing.verticalSpacing);
        }

        private void CopyThing()
        {
            if (curApparel == null || copiedThing == null || curApparel == copiedThing)
                return;

            //stat
            curApparel.statBases = CopyUtility.CopyStats(copiedThing.statBases);

            //stat offsets
            curApparel.equippedStatOffsets = CopyUtility.CopyStats(copiedThing.equippedStatOffsets);

            //modExtension
            if (copiedThing.HasModExtension<PartialArmorExt>())
            {
                PartialArmorExt propCopy = curApparel.GetModExtension<PartialArmorExt>();
                if (propCopy == null)
                {
                    propCopy = new PartialArmorExt()
                    {
                        stats = new List<ApparelPartialStat>()
                    };
                    curApparel.AddModExtension(propCopy);
                }
                CopyUtility.CopyModExtension(copiedThing.GetModExtension<PartialArmorExt>(), propCopy);
            }
        }


        private bool IsCECompactied(ThingDef thing)
        {
            if (thing == null)
                return false;

            return thing.statBases.Any(x => x.stat == CE_StatDefOf.Bulk);
        }

        private void MakeCEPatch(ThingDef thing)
        {
            if (thing == null)
                return;

            //CE related stats
            List<StatDef> statReadyToAdd = new List<StatDef>()
            {
                StatDefOf.ArmorRating_Sharp,
                StatDefOf.ArmorRating_Blunt,
                StatDefOf.ArmorRating_Heat,
                CE_StatDefOf.ArmorRating_Electric,
                CE_StatDefOf.Bulk,
                CE_StatDefOf.WornBulk,
            };

            foreach (var item in statReadyToAdd)
            {
                if (!thing.statBases.Any(x => x.stat == item))
                {
                    thing.statBases.Add(new StatModifier()
                    {
                        stat = item,
                        value = item.defaultBaseValue,
                    });
                }
                else
                {
                    StatModifier stat = thing.statBases.FirstOrDefault(x => x.stat == item);
                    if (stat != null)
                    {
                        thing.statBases.Remove(stat);
                        thing.statBases.Add(stat);
                    }
                }
            }
        }

        private void DrawPartialArmorExt(Listing_Standard listing)
        {
            if (curApparel == null)
                return;

            listing.GapLine(6f);

            listing.ButtonImageLine("<b>" + "MP_PartialArmorExt".Translate() + "</b>", TexButton.Add, () =>
            {
                List<StatDef> statDefs = new List<StatDef>()
                 {
                        StatDefOf.ArmorRating_Sharp,
                        StatDefOf.ArmorRating_Blunt,
                        StatDefOf.ArmorRating_Heat,
                        CE_StatDefOf.ArmorRating_Electric,
                 };

                FloatMenuUtility.MakeMenu(statDefs,
                   (x) => x.LabelCap,
                   (x) => delegate ()
                   {
                       manager.GetPatch(curApparel);
                       if (!curApparel.HasModExtension<PartialArmorExt>())
                       {
                           curApparel.AddModExtension(new PartialArmorExt()
                           {
                               stats = new List<ApparelPartialStat>()
                           });
                       }

                       PartialArmorExt partialArmorExt = curApparel.GetModExtension<PartialArmorExt>();
                       partialArmorExt.stats.Add(new ApparelPartialStat()
                       {
                           stat = x,
                           parts = new List<BodyPartDef>()
                       });
                   });
            }, indent: 0f);

            //partial armor
            if (curApparel.GetModExtension<PartialArmorExt>() is PartialArmorExt ext)
            {
                bool needBreak = false;
                foreach (var item in ext.stats)
                {
                    listing.Gap(6f);

                    listing.ButtonImageLine(item.stat.LabelCap, TexButton.Delete, () =>
                    {
                        manager.GetPatch(curApparel);
                        ext.stats.Remove(item);
                        needBreak = true;
                    }, indent: 0f);
                    if (needBreak) break;

                    listing.FieldLineOnChange("MP_UseStaticValue".Translate(), ref item.isStatValueStatic, (x) =>
                    {
                        manager.GetPatch(curApparel);
                    }, indent: 20f);

                    if (item.isStatValueStatic)
                    {
                        listing.FieldLineOnChange("MP_StaticValue".Translate(), ref item.statValue, (x) =>
                        {
                            manager.GetPatch(curApparel);
                        }, indent: 20f);
                    }
                    else
                    {
                        listing.FieldLineOnChange("MP_MultValue".Translate(), ref item.statValue, (x) =>
                        {
                            manager.GetPatch(curApparel);
                        }, indent: 20f);
                    }

                    listing.ButtonImageLine("MP_BodyPart".Translate(), TexButton.Add, () =>
                    {
                        manager.GetPatch(curApparel);
                        Find.WindowStack.Add(new Dialog_SelectBodyParts(item.parts));
                    }, indent: 20f);

                    bool needBreak2 = false;
                    foreach (var bodyPart in item.parts)
                    {
                        listing.ButtonImageLine(bodyPart.LabelCap, TexButton.Delete, () =>
                        {
                            manager.GetPatch(curApparel);
                            item.parts.Remove(bodyPart);
                            needBreak2 = true;
                        }, indent: 40f);
                        if (needBreak2) break;
                    }

                }
            }
        }


    }
}
