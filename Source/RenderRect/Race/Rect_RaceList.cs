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
    internal class Rect_RaceList : RenderRectBase
    {
        private RaceManager manager => RaceManager.instance;

        private List<ModContentPack> activeModsInt = null;
        private List<ModContentPack> ActiveMods
        {
            get
            {
                if (activeModsInt == null)
                {
                    activeModsInt = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race != null).Select(x => x.modContentPack).Distinct().ToList();
                }
                return activeModsInt;
            }
        }

        private string keyWords;
        private ModContentPack curSourceMod = null;
        private bool modifiedOnly = false;

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;

        private List<ThingDef> FilteredDefs
        {
            get
            {
                List<ThingDef> list = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && !x.IsCorpse).ToList();

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.label != null && x.label.ContainsIgnoreCase(keyWords)).ToList();
                }

                if (curSourceMod != null)
                {
                    list = list.Where(x => x.modContentPack == curSourceMod).ToList();
                }

                if(modifiedOnly)
                {
                    list = list.Where(x => manager.HasPatch(x)).ToList();
                }   

                return list;
            }
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

            listingStandard.SearchBar(ref keyWords);

            Rect listRect = listingStandard.GetRect(rect.height - listingStandard.CurHeight - 0.1f);

            WidgetsUtility.ScrollView(listRect, ref scrollPosition, ref scrollHeight, (listing) =>
            {
                foreach (var item in FilteredDefs)
                {
                    try
                    {
                        RenderRectUtility.DrawItemRow(listing, item, ref RaceManager.curDef);
                    }
                    catch (Exception e)
                    {
                        MP_Log.Error("Error while drawing Weapon tab", e, item);
                    }

                }
            });

            listingStandard.End();
        }
    }
}
