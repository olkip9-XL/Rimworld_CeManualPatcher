using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Patch;
using CeManualPatcher.Patch;
using CeManualPatcher.Saveable;
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
    internal class Rect_BodyInfo : RenderRectBase
    {
        private Dictionary<BodyDef, List<ThingDef>> bodyRaceDic = new Dictionary<BodyDef, List<ThingDef>>();

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

            WidgetsUtility.ScrollView(listing.GetRect(rect.height - listing.CurHeight - 30f - 0.1f), ref scrollPosition, ref scrollHeight, (innerListing) =>
            {
                DrawBodyUsers(innerListing);
                DrawBodyPart(innerListing, body.corePart, new List<int>());
            });

            DrawControlPannel(listing);

            listing.End();
        }
        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            Rect resetAllRect = rect.LeftPartPixels(100f);
            if (Widgets.ButtonText(resetAllRect, "MP_ResetAll".Translate()))
            {
                manager.ResetAll();
            }

            Rect exportCEPatchRect = rect.RightPartPixels(120f);
            if (Widgets.ButtonText(exportCEPatchRect, "MP_Export".Translate()))
            {
                Mod_CEManualPatcher.settings.ExportPatch();
            }
        }
        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //sign
            Rect signRect = rect.LeftPartPixels(3f);
            if (manager.HasPatch(body))
            {
                Widgets.DrawBoxSolid(signRect, MP_Color.SignGreen);
            }

            //icon
            Rect rect1 = signRect.RightAdjoin(rect.height);
            Widgets.DrawTextureFitted(rect1, manager.iconDic[body], 0.7f);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - rect1.width - signRect.width - 0.1f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, body.LabelCap);
            Text.Font = GameFont.Small;

            //reset 
            Rect rect3 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, MP_Texture.Reset))
            {
                //TODO : reset
                manager.Reset(body);
            }

        }
       
        private void DrawBodyUsers(Listing_Standard listing)
        {
            listing.GapLine();

            listing.LabelLine("MP_BodyUser".Translate());

            List<ThingDef> thingDefs = null;

            if (!this.bodyRaceDic.ContainsKey(body))
            {
                thingDefs = DefDatabase<ThingDef>.AllDefs.Where(x=>x.race?.body == body && !x.IsCorpse).ToList();

                this.bodyRaceDic.Add(body, thingDefs);
            }
            else
            {
                bodyRaceDic.TryGetValue(body, out thingDefs);
            }

            if (thingDefs.NullOrEmpty())
            {
                return;
            }


            foreach (var def in thingDefs)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                rect.x += 20f; //indent
                rect.width -= 20f; //indent

                Rect iconRect = rect.LeftPartPixels(rect.height);
                Widgets.DrawTextureFitted(iconRect, def.uiIcon ?? BaseContent.BadTex, 0.7f);

                Rect labelRect = iconRect.RightAdjoin(rect.width);

                if (Widgets.ButtonText(labelRect, def.LabelCap, drawBackground: false, doMouseoverSound: false, textColor: Widgets.NormalOptionColor))
                {
                    Mod_CEManualPatcher.instance.SetCurTab(MP_SettingTab.Race);
                    RaceManager.curDef = def;
                }

                listing.Gap(listing.verticalSpacing);
            }

        }

        private void DrawBodyPart(Listing_Standard listing, BodyPartRecord record, List<int> path, float indent = 0f)
        {
            listing.GapLine();

            Rect headRect;

            DrawCollapsibleRect(listing, indent, record.GetHashCode(), (innerListing) =>
            {
                innerListing.ButtonTextLine("MP_Height".Translate(), record.height.ToString(), () =>
                {
                    List<BodyPartHeight> list = Enum.GetValues(typeof(BodyPartHeight)).Cast<BodyPartHeight>().ToList();
                    List<int> innerPath = new List<int>(path);

                    FloatMenuUtility.MakeMenu(list,
                        (x) => x.ToString(),
                        (x) => delegate
                        {
                            (manager.GetPatch(body) as BodyDefPatch).AddBodyPart(innerPath);
                            record.height = x;
                        }
                     );
                }, indent: indent + Text.LineHeight);

                innerListing.ButtonTextLine("MP_Depth".Translate(), record.depth.ToString(), () =>
                {
                    List<BodyPartDepth> list = Enum.GetValues(typeof(BodyPartDepth)).Cast<BodyPartDepth>().ToList();
                    List<int> innerPath = new List<int>(path);
                    
                    FloatMenuUtility.MakeMenu(list,
                        (x) => x.ToString(),
                        (x) => delegate
                        {
                            (manager.GetPatch(body) as BodyDefPatch).AddBodyPart(innerPath);
                            record.depth = x;
                        }
                     );
                }, indent: indent + Text.LineHeight);

                innerListing.FieldLineOnChange("MP_Coverage".Translate(), ref record.coverage, (newValue) =>
                {
                    (manager.GetPatch(body) as BodyDefPatch).AddBodyPart(path);
                }, indent: indent + Text.LineHeight);

                bool coverdByArmor = BodyPartRecordSaveable.GetCoverByArmor(record);
                innerListing.FieldLineOnChange("MP_CoverdByArmor".Translate(), ref coverdByArmor, (newValue) =>
                {
                    (manager.GetPatch(body) as BodyDefPatch).AddBodyPart(path);
                }, indent:indent+Text.LineHeight);
                BodyPartRecordSaveable.SetCoverByArmor(record, coverdByArmor);

                if (!record.parts.NullOrEmpty())
                {
                    for (int i = 0; i < record.parts.Count; i++)
                    {
                        path.Add(i);
                        DrawBodyPart(innerListing, record.parts[i], path, indent + 20f);
                        path.Pop(); 
                    }
                }
            }, out headRect);

            if (record.customLabel.NullOrEmpty())
            {
                record.customLabel = record.LabelCap;
            }
            WidgetsUtility.LabelChange(headRect, ref record.customLabel, record.GetHashCode(), () =>
            {
                (manager.GetPatch(body) as BodyDefPatch).AddBodyPart(path);
            });
        }

        private Dictionary<int, bool> collapseDic = new Dictionary<int, bool>();
        private void DrawCollapsibleRect(Listing_Standard listing, float indent, int id, Action<Listing_Standard> DrawContent, out Rect headRect)
        {
            if (!collapseDic.ContainsKey(id))
            {
                collapseDic.Add(id, false);
            }

            bool isCollapsed = collapseDic[id];

            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect buttonRect = rect.LeftPartPixels(rect.height);
            headRect = rect.RightPartPixels(rect.width - rect.height - 0.1f);

            if (isCollapsed)
            {
                if (Widgets.ButtonImage(buttonRect, TexButton.Reveal))
                {
                    collapseDic[id] = false;
                }
            }
            else
            {
                if (Widgets.ButtonImage(buttonRect, TexButton.Collapse))
                {
                    collapseDic[id] = true;
                }
                DrawContent?.Invoke(listing);
            }
        }
    }
}
