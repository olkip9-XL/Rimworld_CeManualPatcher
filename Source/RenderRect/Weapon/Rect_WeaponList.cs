using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Manager;
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
    internal class Rect_WeaponList : RenderRectBase
    {
        public ThingDef curWeaponDef
        {
            get
            {
                return WeaponManager.curWeaponDef;
            }
            set
            {
                WeaponManager.curWeaponDef = value;
            }
        }

        private List<ModContentPack> activeModsInt = null;
        private List<ModContentPack> ActiveMods
        {
            get
            {
                if (activeModsInt == null)
                {
                    activeModsInt = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsWeapon).Select(x => x.modContentPack).Distinct().ToList();
                }
                return activeModsInt;
            }
        }

        private string keyWords;
        private ModContentPack curSourceMod = null;

        private Vector2 scrollPosition;

        private float innerHeight = 0;

        private readonly float rowHeight = 30;

        private List<ThingDef> WeaponDefs
        {
            get
            {
                List<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsWeapon).ToList();

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.label.ToLower().Contains(keyWords.ToLower())).ToList();
                }

                if (curSourceMod != null)
                {
                    list = list.Where(x => x.modContentPack == curSourceMod).ToList();
                }

                return list;
            }
        }


        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            //来源mod
            listingStandard.ButtonX("MP_Source".Translate(), 150f, curSourceMod?.Name ?? "MP_All".Translate(), delegate
            {
                FloatMenuUtility.MakeMenu<ModContentPack>(ActiveMods,
                    (mod) => mod?.Name ?? "MP_All".Translate(),
                    (mod) => delegate
                    {
                        curSourceMod = mod;
                    });
            });

            listingStandard.SearchBar(ref keyWords);

            List<ThingDef> allWeaponDefs = WeaponDefs;

            Rect listRect = listingStandard.GetRect(rect.height - listingStandard.CurHeight - 0.1f);

            WidgetsUtility.ScrollView(listRect, ref scrollPosition, ref innerHeight, (listing) =>
            {
                foreach (var item in allWeaponDefs)
                {
                    try
                    {
                        RenderRectUtility.DrawItemRow(listing, item, ref WeaponManager.curWeaponDef);
                    }
                    catch(Exception e)
                    {
                        Log.ErrorOnce($"[CeManualPatcher] Error while drawing Weapon tab {item?.defName ?? "null"} from {item?.modContentPack.Name ?? "null"} : {e}", e.GetHashCode());
                    }

                }
            });

            listingStandard.End();
        }
    }
}
