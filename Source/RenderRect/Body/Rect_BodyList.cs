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

namespace CeManualPatcher.RenderRect.Body
{
    internal class Rect_BodyList : RenderRectBase
    {
        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;

        //filter
        private string keyWords;

        private BodyDefManager manager => BodyDefManager.instance;
        private BodyDef body
        {
            get => manager.curBody;
            set => manager.curBody = value;
        }

        private List<BodyDef> allBodyDefs
        {
            get
            {
                List<BodyDef> list = new List<BodyDef>();

                list.AddRange(DefDatabase<BodyDef>.AllDefs);

                if (!keyWords.NullOrEmpty())
                {
                    list = list.Where(x => x.label != null && x.label.ContainsIgnoreCase(keyWords)).ToList();
                }

                return list;
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            listing.SearchBar(ref keyWords);

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 0.1f), ref scrollPosition, ref scrollHeight, (innerListing) =>
            {
                foreach (var item in allBodyDefs)
                {
                    try
                    {
                        RenderRectUtility.DrawItemRow(innerListing, manager.iconDic[item], item.label ?? "No Label", manager.descriptionDic[item] ?? "No Description", () =>
                        {
                            body = item;
                            Messages.Message(item.defName, MessageTypeDefOf.SilentInput);
                        }, body == item);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce($"[CeManualPatcher] Error while drawing Body tab {item?.defName ?? "null"} from {item?.modContentPack.Name ?? "null"} : {e}", e.GetHashCode());
                    }
                }
            });

            listing.End();
        }
    }
}
