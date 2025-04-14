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

                if(curSourceMod != null)
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

            Rect listRect = listingStandard.GetRect(rect.height - listingStandard.CurHeight);
            WidgetsUtility.ScrollView(listRect, ref scrollPosition, ref innerHeight, (listing) =>
            {
                foreach (var item in allWeaponDefs)
                {
                    DrawRow(item, listing);
                }
            });

            listingStandard.End();
        }

        private void DrawRow(ThingDef item, Listing_Standard listing)
        {
            if (item == null)
                return;

            Rect rect = listing.GetRect(rowHeight);

            if (Widgets.ButtonInvisible(rect))
            {
                curWeaponDef = item;
                Messages.Message(item.defName, MessageTypeDefOf.SilentInput);
            }

            Rect iconRect = new Rect(rect.x, rect.y, rowHeight, rowHeight);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width, rowHeight);

            Widgets.DrawTextureFitted(iconRect, item.uiIcon, 0.7f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, item.label);
            Text.Anchor = TextAnchor.UpperLeft;

            if (curWeaponDef == item)
            {
                Widgets.DrawHighlightSelected(rect);
            }

            TooltipHandler.TipRegion(rect, $"{item.description}\n\n{"MP_Source".Translate()} {item.modContentPack.Name}");
        }
    }
}
