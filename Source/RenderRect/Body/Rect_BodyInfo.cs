using CeManualPatcher.Extension;
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

namespace CeManualPatcher.RenderRect.Body
{
    internal class Rect_BodyInfo : RenderRectBase
    {

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0;

        private BodyDefManager manager => BodyDefManager.instance;

        private BodyDef body => manager.curBody;

        public override void DoWindowContents(Rect rect)
        {
            if (body == null)
            {
                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            DrawHead(listing);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f), ref scrollPosition, ref scrollHeight, (innerListing) =>
            {
                DrawBodyPart(innerListing, body.corePart);
            });

            listing.End();
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //icon
            Rect rect1 = rect.LeftPartPixels(rect.height);
            Widgets.DrawTextureFitted(rect1, manager.iconDic[body], 1f);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - rect1.width);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, body.LabelCap);
            Text.Font = GameFont.Small;

            //reset 
            Rect rect3 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                //TODO : reset
            }

        }

        private void DrawBodyPart(Listing_Standard listing, BodyPartRecord record, float indent = 0f)
        {
            listing.GapLine();

            listing.LabelLine(record.LabelCap, indent: indent);

            Rect customLabelRect = listing.GetRect(Text.LineHeight);
            customLabelRect.x += indent;
            customLabelRect.width -= indent;
            WidgetsUtility.LabelChange(customLabelRect, ref record.customLabel, record.GetHashCode());

            listing.ButtonTextLine("Height", record.height.ToString(), () =>
            {
                List<BodyPartHeight> list = Enum.GetValues(typeof(BodyPartHeight)).Cast<BodyPartHeight>().ToList();

                FloatMenuUtility.MakeMenu(list,
                    (x) => x.ToString(),
                    (x) => delegate
                    {
                        record.height = x;
                    }
                 );
            }, indent: indent);

            listing.ButtonTextLine("Depth", record.depth.ToString(), () =>
            {
                List<BodyPartDepth> list = Enum.GetValues(typeof(BodyPartDepth)).Cast<BodyPartDepth>().ToList();
                FloatMenuUtility.MakeMenu(list,
                    (x) => x.ToString(),
                    (x) => delegate
                    {
                        record.depth = x;
                    }
                 );
            }, indent: indent);

            listing.FieldLineOnChange("coverage", ref record.coverage, (newValue) =>
            {

            }, indent: indent);

            if (!record.parts.NullOrEmpty())
            {
                foreach (var part in record.parts)
                {
                    DrawBodyPart(listing, part, indent + 20f);
                }
            }
        }
    }
}
