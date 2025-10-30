using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.RenderRect;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_AddTool : Window
    {

        private List<Tool> tools;

        private ToolCE toolReadyToAdd;

        private BodyDef bodyDef = null;

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;
        private WeaponManager manager => WeaponManager.instance;
        private ThingDef curWeaponDef => WeaponManager.curWeaponDef;
        public Dialog_AddTool(List<Tool> tools, BodyDef _bodyDef)
        {
            this.tools = tools;
            this.toolReadyToAdd = new ToolCE()
            {
                label = "New Tool",
            };
            this.toolReadyToAdd.id = "NewTool" + this.toolReadyToAdd.GetHashCode();
            this.bodyDef = _bodyDef;
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
                return new Vector2(400f, 350f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            WidgetsUtility.ScrollView(listingStandard.GetRect(inRect.height - listingStandard.CurHeight - 30f), ref scrollPosition, ref scrollHeight, (innerListing) =>
            {
                innerListing.FieldLine("Label", ref toolReadyToAdd.label);

                Rect_WeaponInfo.DrawToolContent(innerListing, toolReadyToAdd, null, bodyDef: bodyDef);

                DrawControlPannel(innerListing);
            });
            listingStandard.End();
        }
        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //rect 中间两个100f宽的Rect
            Rect leftRect = new Rect(rect.x + rect.width / 2 - 100f - 6f, rect.y, 100f, rect.height);
            Rect rightRect = new Rect(rect.x + rect.width / 2 + 6f, rect.y, 100f, rect.height);

            if (Widgets.ButtonText(leftRect, "Accept"))
            {
                OnAcceptKeyPressed();
                this.Close();
            }

            if (Widgets.ButtonText(rightRect, "Cancel"))
            {
                this.Close();
            }
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            this.tools.Add(toolReadyToAdd);
        }
    }
}
