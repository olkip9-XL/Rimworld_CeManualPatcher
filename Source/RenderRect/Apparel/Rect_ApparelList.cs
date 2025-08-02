using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.RenderRect
{
    internal class Rect_ApparelList : RenderRectBase
    {

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight = 0f;

        //filter
        private string keyWords;
        private ModContentPack curMod;

        private List<ModContentPack> activeModsInt = null;
        private List<ModContentPack> ActiveMods
        {
            get
            {
                if (activeModsInt == null)
                {
                    activeModsInt = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel).Select(x => x.modContentPack).Distinct().ToList();
                }
                return activeModsInt;
            }
        }

        List<ThingDef> ApparelDefs
        {
            get
            {
                List<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsApparel).ToList();

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.label!=null && x.label.ContainsIgnoreCase(keyWords)).ToList();
                }

                if (curMod != null)
                {
                    list = list.Where(x => x.modContentPack == curMod).ToList();
                }

                return list;
            }
        }

        private ApparelManager manager => ApparelManager.instance;
        private ThingDef curApparel
        {
            get => ApparelManager.curApparelDef;
            set => ApparelManager.curApparelDef = value;
        }

        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            //head

            listing.ButtonTextLine("MP_Source".Translate(), curMod?.Name ?? "MP_All".Translate(), delegate
            {
                List<ModContentPack> options = new List<ModContentPack>();
                options.Add(null);
                options.AddRange(ActiveMods);

                FloatMenuUtility.MakeMenu<ModContentPack>(options,
                    (mod) => mod?.Name ?? "MP_All".Translate(),
                    (mod) => delegate
                    {
                        curMod = mod;
                    });

            }, fieldWidth:150f);

            listing.SearchBar(ref keyWords);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f), ref scrollPosition, ref viewHeight, (innerListing) =>
            {
                foreach (var item in ApparelDefs)
                {
                    try
                    {
                        RenderRectUtility.DrawItemRow(innerListing, item, ref ApparelManager.curApparelDef);
                    }
                    catch (Exception e)
                    {
                        //Log.ErrorOnce($"[CeManualPatcher] Error while drawing Apparel tab {item?.defName ?? "null"} from {item?.modContentPack?.Name ?? "null"} : {e}", e.GetHashCode());
                        MP_Log.Error("Error while drawing Apparel tab", e, item);
                    }
                }
            });

            listing.End();
        }
    }
}
