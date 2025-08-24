using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
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
    internal enum HediffDefCategory
    {
        All,
        Bionic,
        HasArmor,
        HasTools
    }

    internal static class HediffDefCategoryExtension
    {
        public static string GetLabel(this HediffDefCategory category)
        {
            switch (category)
            {
                case HediffDefCategory.All:
                case HediffDefCategory.Bionic:
                case HediffDefCategory.HasArmor:
                case HediffDefCategory.HasTools:
                    return $"MP_HediffDefCategory.{category.ToString()}".Translate();
                default:
                    return category.ToString();
            }
        }
    }

    internal class Rect_HediffList : RenderRectBase
    {
        HediffDefManager manager => HediffDefManager.instance;

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float innerHeight = 0f;

        //filters
        private string keyWords = string.Empty;
        private ModContentPack curSourceMod = null;
        private bool modifiedOnly = false;
        private HediffDefCategory curCategory = HediffDefCategory.All;


        private List<ModContentPack> activeModsInt = null;
        private List<ModContentPack> ActiveMods
        {
            get
            {
                if (activeModsInt == null)
                {
                    activeModsInt = DefDatabase<HediffDef>.AllDefsListForReading.Select(x => x.modContentPack).Distinct().ToList();
                }
                return activeModsInt;
            }
        }


        private List<HediffDef> HediffDefs
        {

            get
            {
                List<HediffDef> list = new List<HediffDef>(DefDatabase<HediffDef>.AllDefs);

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x != null && x.label.ContainsIgnoreCase(keyWords)).ToList();
                }

                if (curSourceMod != null)
                {
                    list = list.Where(x => x.modContentPack == curSourceMod).ToList();
                }

                if (modifiedOnly)
                {
                    list = list.Where(x => manager.HasPatch(x)).ToList();
                }

                if (curCategory != HediffDefCategory.All)
                {
                    switch (curCategory)
                    {
                        case HediffDefCategory.Bionic:
                            list = list.Where(x => x.spawnThingOnRemoved != null).ToList();
                            break;
                        case HediffDefCategory.HasArmor:
                            list = list.Where(x => HediffHasArmor((x))).ToList();
                            break;
                        case HediffDefCategory.HasTools:
                            list = list.Where(x => x.HasComp(typeof(HediffComp_VerbGiver))).ToList();
                            break;
                    }
                }

                return list;
            }
        }

        private bool HediffHasArmor(HediffDef hediffDef)
        {
            if(hediffDef.stages.NullOrEmpty())
            {
                return false;
            }

            foreach (var stage in hediffDef.stages)
            {
                if(stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Sharp) != 0f ||
                   stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Blunt) != 0f ||
                   stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Heat) != 0f ||
                   stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Sharp) != 1f ||
                   stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Blunt) != 1f ||
                   stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Heat) != 1f)
                {
                    return true;
                }
            }

            return false;
        }


        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            //来源mod
            listingStandard.ButtonTextLine("MP_Source".Translate(), curSourceMod?.Name ?? "MP_All".Translate(), delegate
            {
                List<ModContentPack> options = new List<ModContentPack>();
                options.Add(null);
                options.AddRange(ActiveMods);

                FloatMenuUtility.MakeMenu<ModContentPack>(options,
                    (mod) => mod?.Name ?? "MP_All".Translate(),
                    (mod) => delegate
                    {
                        curSourceMod = mod;
                    });
            });

            listingStandard.FieldLine("MP_ModifiedOnly".Translate(), ref modifiedOnly);

            listingStandard.ButtonTextLine("MP_Category".Translate(), curCategory.GetLabel(), delegate
            {

                FloatMenuUtility.MakeMenu<HediffDefCategory>(Enum.GetValues(typeof(HediffDefCategory)).Cast<HediffDefCategory>().ToList(),
                    (c) => c.GetLabel(),
                    (c) => delegate
                    {
                        curCategory = c;
                    });
            });

            listingStandard.SearchBar(ref keyWords);



            List<HediffDef> allHediffDefs = HediffDefs;

            Rect listRect = listingStandard.GetRect(rect.height - listingStandard.CurHeight - 0.1f);

            WidgetsUtility.ScrollView(listRect, ref scrollPosition, ref innerHeight, (listing) =>
            {
                foreach (var item in allHediffDefs)
                {
                    try
                    {
                        ThingDef thingdef = item?.spawnThingOnRemoved;

                        RenderRectUtility.DrawItemRow(listing,
                            thingdef?.uiIcon ?? BaseContent.BadTex,
                            item.label,
                            item.description,
                            () =>
                            {
                                HediffDefManager.curHediffDef = item;
                                Messages.Message(item.defName, MessageTypeDefOf.SilentInput);
                            },
                            HediffDefManager.curHediffDef == item);
                    }
                    catch (Exception e)
                    {
                        MP_Log.Error("Error while drawing Hediff tab", e, item);
                    }
                }
            });

            listingStandard.End();

        }
    }
}
