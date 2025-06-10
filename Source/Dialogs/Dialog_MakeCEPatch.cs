using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

using RimWorld;
using CeManualPatcher.Misc;
using CombatExtended;
using CeManualPatcher.Extension;
using CeManualPatcher.Saveable;
using RuntimeAudioClipLoader;
using CeManualPatcher.Manager;
using CeManualPatcher.RenderRect;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_MakeCEPatch : Window
    {

        private WeaponManager weaponManager => WeaponManager.instance;
        private CEPatchManager patchManager => CEPatchManager.instance;
        private ThingDef thingDef;
        private CEPatcher patcher;

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;

        public Dialog_MakeCEPatch(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            this.patcher = new CEPatcher(thingDef);
        }

        public override void PreOpen()
        {
            this.focusWhenOpened = true;
            this.draggable = true;
            this.doCloseX = true;
            this.resizeable = true;

            this.windowRect = new Rect((UI.screenWidth - this.InitialSize.x) / 2f, (UI.screenHeight - this.InitialSize.y) / 2f, this.InitialSize.x, this.InitialSize.y).Rounded();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            if (thingDef == null)
            {
                listing.Label("ThingDef is null");
                listing.End();
                return;
            }

            Text.Font = GameFont.Medium;
            listing.Label("Make CE Patch");
            Text.Font = GameFont.Small;

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(inRect.height - listing.CurHeight - 30f), ref scrollPosition, ref scrollHeight,
            (innerListing) =>
            {

                RenderRectUtility.DrawStats(innerListing, ref patcher.stats, MP_Options.statDefs_Weapon, null);
                RenderRectUtility.DrawStats(innerListing, ref patcher.statOffsets, MP_Options.statDefs_WeaponOffset, null, headLabel: "MP_StatOffset".Translate());

                Rect_WeaponInfo.DrawWeaponTags(innerListing, patcher.weaponTags, null);
                Rect_WeaponInfo.DrawVebs(innerListing, patcher.verbProperties, patcher.ammoUser, null);
                Rect_WeaponInfo.DrawTools(innerListing, patcher.tools, null);
                Rect_WeaponInfo.DrawComps(innerListing, patcher.fireMode, patcher.ammoUser, patcher.verbProperties, null);
                Rect_WeaponInfo.DrawChargeSpeed(innerListing, patcher.charges, null);

            });

            Rect buttonRect = new Rect(0, 0, 100f, 30f).CenterIn(listing.GetRect(30f));
            if (Widgets.ButtonText(buttonRect, "MP_Accept".Translate()))
            {
                this.OnAcceptKeyPressed();
            }

            listing.End();
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            Rect iconRect = rect.LeftPartPixels(30f);
            Rect labelRect = rect.RightPartPixels(rect.width - 30f);

            Widgets.ThingIcon(iconRect, thingDef);
            Widgets.Label(labelRect, thingDef.LabelCap);

            listing.Gap(listing.verticalSpacing);
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            patcher.ApplyCEPatch();
            patchManager.AddPatcher(patcher);

            this.Close();
        }

        public override void OnCancelKeyPressed()
        {
            base.OnCancelKeyPressed();

            weaponManager.Reset(thingDef);
        }
    }
}
