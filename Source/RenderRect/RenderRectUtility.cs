using CeManualPatcher.Extension;
using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal static class RenderRectUtility
    {

        public static void DrawItemRow(Listing_Standard listing, ThingDef item, ref ThingDef curItem)
        {
            if (item == null)
                return;

            string description = $"{item?.description}\n\n{"MP_Source".Translate()} {item.modContentPack?.Name}";

            bool changed = false;
            DrawItemRow(listing, item.uiIcon, item.label, description, () =>
            {
                changed = true;
                Messages.Message(item.defName, MessageTypeDefOf.SilentInput);
            }, curItem == item);

            if (changed)
            {
                curItem = item;
            }
        }

        public static void DrawItemRow(Listing_Standard listing, Texture2D icon, TaggedString label, string description, Action onClick, bool drawHighlight)
        {
            float rowHeight = 30f;

            Rect rect = listing.GetRect(rowHeight);

            if (Widgets.ButtonInvisible(rect))
            {
                onClick?.Invoke();
                WidgetsUtility.ResetTextFieldBuffer();
            }

            Rect iconRect = rect.LeftPartPixels(rowHeight);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width, rowHeight);

            Texture2D uiTexture = icon ?? BaseContent.BadTex;
            Widgets.DrawTextureFitted(iconRect, uiTexture, 0.7f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            if (drawHighlight)
            {
                Widgets.DrawHighlightSelected(rect);
            }

            TooltipHandler.TipRegion(rect, description);

        }


        //Stats dic
        private static Dictionary<int, StatCategoryDef> statCategoryDic = new Dictionary<int, StatCategoryDef>();

        public static void DrawStats(Listing_Standard listing, ref List<StatModifier> stats, ReadOnlyCollection<StatDef> avaliableStats, Action preChange, string headLabel = null)
        {
            listing.GapLine(6f);

            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(headRect.height);

            if (headLabel.NullOrEmpty())
                headLabel = "MP_StatBase".Translate();

            Widgets.Label(headRect, "<b>" + headLabel + "</b>");

            int hashCode = stats?.GetHashCode() ?? 0;
            if (!statCategoryDic.ContainsKey(hashCode))
            {
                statCategoryDic[hashCode] = null;
            }

            // Add button
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                if (stats == null)
                    stats = new List<StatModifier>();

                List<StatModifier> localStats = stats;

                List<StatDef> list = new List<StatDef>();
                list.AddRange(avaliableStats);

                if (stats != null)
                {
                    list = list.Where(x => !localStats.Any(y => y.stat == x)).ToList();
                }

                if (statCategoryDic[hashCode] != null)
                {
                    list = list.Where(x => x.category == statCategoryDic[hashCode]).ToList();
                }

                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var item in list)
                {
                    FloatMenuOption option = new FloatMenuOption(item.LabelCap, delegate
                    {
                        preChange?.Invoke();
                        //???
                        localStats.Add(new StatModifier()
                        {
                            stat = item,
                            value = item.defaultBaseValue,
                        });
                    }, mouseoverGuiAction: (rect) =>
                    {
                        TooltipHandler.TipRegion(rect, item.description);
                    });
                    options.Add(option);
                }
                if (options.Any())
                {
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            // Stat category button
            Rect categoryRect = addRect.LeftAdjoin(150f);
            if (Widgets.ButtonText(categoryRect, statCategoryDic[hashCode]?.LabelCap ?? "MP_All".Translate()))
            {
                List<StatCategoryDef> categoryDefs = new List<StatCategoryDef>();
                categoryDefs.Add(null);
                foreach (var item in avaliableStats)
                {
                    categoryDefs.AddDistinct(item.category);
                }

                FloatMenuUtility.MakeMenu<StatCategoryDef>(categoryDefs,
                    (item) => (item?.LabelCap ?? "MP_All".Translate()) + $"({item?.defName})",
                    (item) => delegate
                    {
                        statCategoryDic[hashCode] = item;
                    });
            }

            // Category Label
            string label = "MP_Category".Translate();
            Rect labelRect = categoryRect.LeftAdjoin(Text.CalcSize(label).x);
            Widgets.Label(labelRect, label);

            listing.Gap(6f);
            // Stat entries
            foreach (var item in stats ?? new List<StatModifier>())
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 20f;
                rect.width -= 20f;

                Widgets.Label(rect, item.stat.LabelCap);

                // Delete button
                Rect rect1 = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(rect1, TexButton.Delete))
                {
                    preChange?.Invoke();

                    stats.RemoveWhere(x => x.stat == item.stat);
                    break;
                }

                // Field
                Rect fieldRect = rect1.LeftAdjoin(100f);
                WidgetsUtility.TextFieldOnChange<float>(fieldRect, ref item.value, newValue =>
                {
                    preChange?.Invoke();
                });

                TooltipHandler.TipRegion(rect, item.stat.description);
                Widgets.DrawHighlightIfMouseover(rect);
            }
        }

    }
}
