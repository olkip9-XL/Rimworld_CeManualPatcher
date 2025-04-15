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

namespace CeManualPatcher.RenderRect.Ammo
{
    internal class Rect_AmmoList : RenderRectBase
    {
        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight = 0f;

        //filter
        private string keyWords = "";
        private MP_AmmoCategory curCategory = null;
        private bool modifiedOnly = false;

        private List<MP_AmmoSet> allAmmoSetsInt = null;
        private List<MP_AmmoSet> AllAmmoSets
        {
            get
            {
                if (allAmmoSetsInt == null)
                {
                    List<ThingDef> processedProjectiles = new List<ThingDef>();
                    allAmmoSetsInt = new List<MP_AmmoSet>();

                    //normal
                    foreach (var item in DefDatabase<AmmoSetDef>.AllDefs)
                    {
                        allAmmoSetsInt.Add(new MP_AmmoSet(item));
                        foreach (var ammoLink in item.ammoTypes)
                        {
                            processedProjectiles.AddDistinct(ammoLink.projectile);
                        }
                    }

                    //hand grenade
                    MP_AmmoCategory grenadeCategory = MP_Options.ammoCategories.Find(x => x.name == "Grenades");
                    foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWithinCategory(MP_ThingCategoryDefOf.Grenades)))
                    {
                        ThingDef defaultProjectile = item.Verbs.FirstOrDefault()?.defaultProjectile;
                        allAmmoSetsInt.Add(new MP_AmmoSet(item, defaultProjectile, grenadeCategory));

                        processedProjectiles.AddDistinct(defaultProjectile);
                    }

                    //uncategrized
                    MP_AmmoCategory unCategorized = MP_Options.ammoCategories.Find(x => x.name == "Uncategorized");
                    foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(x => x.projectile != null).Except(processedProjectiles))
                    {
                        allAmmoSetsInt.Add(new MP_AmmoSet(null, item, unCategorized));
                    }
                }
                return allAmmoSetsInt;
            }
        }
        private MP_AmmoSet curAmmoSet
        {
            get => AmmoManager.curAmmoSet;
            set => AmmoManager.curAmmoSet = value;
        }
        private AmmoManager manager => AmmoManager.instance;
        private List<MP_AmmoSet> filteredAmmoSet
        {
            get
            {
                List<MP_AmmoSet> list = new List<MP_AmmoSet>();
                list.AddRange(AllAmmoSets);
                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.Label.ContainsIgnoreCase(keyWords)).ToList();
                }

                if (curCategory != null)
                {
                    list = list.Where(x => x.category == curCategory).ToList();
                }

                if (modifiedOnly)
                {
                    list = list.Where(x => manager.HasAmmoPatch(x)).ToList();
                }

                return list;
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            //filter
            listingStandard.ButtonX("MP_Category".Translate(), 150f, curCategory?.Label ?? "MP_All".Translate(), () =>
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();

                list.Add(new FloatMenuOption("MP_All".Translate(), () => curCategory = null));
                foreach (var item in MP_Options.ammoCategories)
                {
                    list.Add(new FloatMenuOption(item.Label, () => curCategory = item));
                }

                Find.WindowStack.Add(new FloatMenu(list));
            });

            listingStandard.CheckboxLabeled("MP_ModifiedOnly".Translate(), ref modifiedOnly);

            listingStandard.SearchBar(ref keyWords);

            Rect innerRect = listingStandard.GetRect(rect.height - listingStandard.CurHeight - 0.1f);

            WidgetsUtility.ScrollView(innerRect, ref scrollPosition, ref viewHeight, listing =>
            {
                foreach (var item in filteredAmmoSet)
                {
                    DrawRow(listing, item);
                }
            });

            listingStandard.End();
        }

        private void DrawRow(Listing_Standard listing, MP_AmmoSet ammoSet)
        {
            if (ammoSet == null)
            {
                return;
            }

            Rect rect = listing.GetRect(30f);

            Rect iconRect = rect.LeftPartPixels(30f);
            Rect labelRect = rect.RightPartPixels(rect.width - 30f);

            Widgets.DrawTextureFitted(iconRect, ammoSet.Icon, 0.7f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, ammoSet.Label);
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(rect))
            {
                curAmmoSet = ammoSet;
                Messages.Message(ammoSet.DefName, MessageTypeDefOf.SilentInput);
            }

            if (curAmmoSet == ammoSet)
            {
                Widgets.DrawHighlightSelected(rect);
            }

            TooltipHandler.TipRegion(rect, $"{ammoSet.Description}\n\n{"MP_Source".Translate()} {ammoSet.sourceModName}");
        }
    }
}
