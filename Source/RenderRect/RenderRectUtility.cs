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

            float rowHeight = 30f;

            Rect rect = listing.GetRect(rowHeight);

            if (Widgets.ButtonInvisible(rect))
            {
                curItem = item;
                Messages.Message(item.defName, MessageTypeDefOf.SilentInput);
            }

            Rect iconRect = rect.LeftPartPixels(rowHeight);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width, rowHeight);

            Texture2D uiTexture = item.uiIcon ?? BaseContent.BadTex;
            Widgets.DrawTextureFitted(iconRect, uiTexture, 0.7f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, item.label);
            Text.Anchor = TextAnchor.UpperLeft;

            if (curItem == item)
            {
                Widgets.DrawHighlightSelected(rect);
            }

            TooltipHandler.TipRegion(rect, $"{item.description}\n\n{"MP_Source".Translate()} {item.modContentPack.Name}");
        }



        //Stats dic
        private static Dictionary<int, StatCategoryDef> statCategoryDic = new Dictionary<int, StatCategoryDef>();

        public static void DrawStats(Listing_Standard listing, List<StatModifier> stats, ReadOnlyCollection<StatDef> avaliableStats, Action preChange)
        {
            listing.GapLine(6f);

            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(headRect.height);
            Widgets.Label(headRect, "<b>" + "MP_StatBase".Translate() + "</b>");

            int hashCode = stats.GetHashCode();
            if (!statCategoryDic.ContainsKey(hashCode))
            {
                statCategoryDic[hashCode] = null;
            }

            //Add button
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                List<StatDef> list = new List<StatDef>();
                list.AddRange(avaliableStats);
                list = list.Where(x => !stats.Any(y => y.stat == x)).ToList();

                if (statCategoryDic[hashCode] != null)
                {
                    list = list.Where(x => x.category == statCategoryDic[hashCode]).ToList();
                }

                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (var item in list)
                {
                    FloatMenuOption option = new FloatMenuOption(item.LabelCap, delegate
                    {
                        if (preChange != null) preChange();

                        stats.Add(new StatModifier()
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

            //stat分类按钮
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

            //category Label
            string label = "MP_Category".Translate();
            Rect labelRect = categoryRect.LeftAdjoin(Text.CalcSize(label).x);
            Widgets.Label(labelRect, label);

            listing.Gap(6f);
            //stat 条目
            foreach (var item in stats)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 20f;
                rect.width -= 20f;

                Widgets.Label(rect, item.stat.LabelCap);

                //delete 
                Rect rect1 = rect.RightPartPixels(rect.height);
                if (Widgets.ButtonImage(rect1, TexButton.Delete))
                {
                    if (preChange != null) preChange();

                    stats.RemoveWhere(x => x.stat == item.stat);
                    break;
                }

                //field
                Rect fieldRect = rect1.LeftAdjoin(100f);
                WidgetsUtility.TextFieldOnChange<float>(fieldRect, ref item.value, newValue =>
                {
                    if (preChange != null) preChange();
                });

                TooltipHandler.TipRegion(rect, item.stat.description);
                Widgets.DrawHighlightIfMouseover(rect);
            }
        }

    }
}
