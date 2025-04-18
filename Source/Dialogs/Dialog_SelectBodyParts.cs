using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using CeManualPatcher.Extension;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_SelectBodyParts : Window
    {
        private List<BodyPartDef> parts;
        private Dictionary<BodyPartDef, bool> selectedParts = new Dictionary<BodyPartDef, bool>();

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;
        public Dialog_SelectBodyParts(List<BodyPartDef> parts)
        {
            this.parts = parts;
            foreach (var item in parts)
            {
                this.selectedParts[item] = true;
            }
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
                BodyDef bodyDef = BodyDefOf.Human;

                //foreach (var part in bodyDef.AllParts)
                //{
                //    Rect rect = innerListing.GetRect(Text.LineHeight);

                //    if (!selectedParts.ContainsKey(part.def))
                //    {
                //        selectedParts[part.def] = false;
                //    }

                //    bool isSelected = selectedParts[part.def];
                //    Widgets.CheckboxLabeled(rect, part.LabelCap, ref isSelected);
                //    selectedParts[part.def] = isSelected;
                //}
                DrawBodyParts(innerListing, bodyDef.corePart);
            });

            DrawControlPannel(listingStandard);

            listingStandard.End();
        }

        private void DrawBodyParts(Listing_Standard listing, BodyPartRecord record, float curIndent = 0)
        {
            if (!selectedParts.ContainsKey(record.def))
            {
                selectedParts[record.def] = false;
            }

            bool check = selectedParts[record.def];
            listing.FieldLine(record.LabelCap, ref check, indent: curIndent);
            selectedParts[record.def] = check;

            if (!record.parts.NullOrEmpty())
            {
                foreach (var item in record.parts)
                {
                    DrawBodyParts(listing, item, curIndent + 20f);
                }
            }
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

            parts.Clear();
            parts.AddRange(selectedParts.Where(x => x.Value).Select(x => x.Key));
        }

    }
}
