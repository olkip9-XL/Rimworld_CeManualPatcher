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
    internal class Rect_CustomAmmoList : RenderRectBase
    {

        private CustomAmmoManager manager => CustomAmmoManager.instance;

        private CustomAmmoSet curAmmoSet
        {
            get => CustomAmmoManager.curAmmoSet;
            set => CustomAmmoManager.curAmmoSet = value;
        }

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight = 0f;


        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            listing.ButtonImageLine("MP_AddCustomAmmo".Translate(), TexButton.Add, () =>
            {
                manager.AddAmmoSet(new CustomAmmoSet());
            });

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f), ref scrollPosition, ref scrollViewHeight, (innerListing) =>
            {
                foreach (var item in manager.AllSets)
                {
                    try
                    {
                        DrawRow(innerListing, item);
                    }
                    catch (Exception e)
                    {
                        //Log.Error($"[CeManualPatcher] Error while drawing Custom Ammo Set tab {item?.defNameBase ?? "null"} : {e}");
                        MP_Log.Error("Error while drawing Custom Ammo Set tab", e);
                    }
                }
            });

            listing.End();
        }

        private void DrawRow(Listing_Standard listing, CustomAmmoSet item)
        {
            bool changed = false;
            RenderRectUtility.DrawItemRow(listing, item.Icon, item.label, item.description, () =>
            {
                changed = true;
                Messages.Message(item.defNameBase, MessageTypeDefOf.SilentInput);
            }, curAmmoSet == item);

            if (changed)
            {
                curAmmoSet = item;
            }
        }

    }
}
