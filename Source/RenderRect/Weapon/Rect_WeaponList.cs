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
    internal class Rect_WeaponList : RenderRectBase
    {

        WeaponManager manager => WeaponManager.instance;

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
        private bool modifiedOnly = false;

        private Vector2 scrollPosition;
        private float innerHeight = 0;

        private List<ThingDef> WeaponDefs
        {
            get
            {
                List<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsWeapon).ToList();

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.label != null && x.label.ContainsIgnoreCase(keyWords)).ToList();
                }

                if (curSourceMod != null)
                {
                    list = list.Where(x => x.modContentPack == curSourceMod).ToList();
                }

                if (modifiedOnly)
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
