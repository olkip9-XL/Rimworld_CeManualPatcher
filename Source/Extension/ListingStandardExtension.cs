using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using RuntimeAudioClipLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CeManualPatcher.Extension
{
    public static class ListingStandardExtension
    {
        public static void SearchBar(this Listing_Standard listing, ref string keyWords)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect fieldRect = rect.LeftPartPixels(rect.width - rect.height);
            Rect iconRect = rect.RightPartPixels(rect.height);

            keyWords = Widgets.TextField(fieldRect, keyWords);

            Widgets.DrawTextureFitted(iconRect, TexButton.Search, 0.7f);

            listing.Gap(listing.verticalSpacing);
        }

        public static void ButtonX(this Listing_Standard listing, string label, float buttonWidth, string buttonLabel, Action onClick, float indent = 0f, string tooltip = null)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;
            Rect buttonRect = rect.RightPartPixels(buttonWidth);

            Widgets.Label(rect, label);
            if (Widgets.ButtonText(buttonRect, buttonLabel))
            {
                onClick();
            }
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }
        public static void TextX(this Listing_Standard listing, string label, float inputWidth, ref string value, float indent = 0f, string tooltip = null)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect textRect = rect.RightPartPixels(inputWidth);

            value = Widgets.TextField(textRect, value);

            Widgets.Label(rect, label);
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }
        public static void DamageRow(this Listing_Standard listing, string damageLabel, float buttonWidth, Action onClick, int damageAmount, float fieldWidth, Action<int> onChange, Action onDelete = null, float indent = 0f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect buttonRect = rect.LeftPartPixels(buttonWidth);
            Rect fieldRect = new Rect(buttonRect.xMax + 6f, rect.y, fieldWidth, rect.height);
            Rect deleteRect = rect.RightPartPixels(rect.height);

            Widgets.Label(rect, "Damage:");

            if (Widgets.ButtonText(buttonRect, damageLabel))
            {
                onClick();
            }

            WidgetsUtility.TextFieldOnChange(fieldRect, ref damageAmount, onChange);

            if (onDelete != null)
            {
                if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
                {
                    onDelete();
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        //*********************************************
        public static void FieldLine<T>(this Listing_Standard listing, TaggedString label, ref T value, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue) where T : struct
        {
            listing.FieldLineOnChange(label, ref value, (x) => { }, fieldWidth, tooltip, indent, min, max);
        }
        public static void FieldLineOnChange<T>(this Listing_Standard listing, TaggedString label, ref T value, Action<T> onChange, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);

            if (value.GetType() == typeof(bool))
            {
                Rect fieldRect = rect.RightPartPixels(rect.height);
                bool temp = (bool)(object)value;

                Widgets.Checkbox(new Vector2(fieldRect.x, fieldRect.y), ref temp);
                if (temp != (bool)(object)value)
                {
                    onChange((T)(object)temp);
                    value = (T)(object)temp;
                }
            }
            else
            {
                Rect fieldRect = rect.RightPartPixels(fieldWidth);

                //Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
                WidgetsUtility.TextFieldOnChange(fieldRect, ref value, (x) =>
                {
                    onChange(x);
                }, min, max);
            }

            Widgets.DrawHighlightIfMouseover(rect);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
        }
        public static void ButtonTextLine(this Listing_Standard listing, TaggedString label, string buttonLabel, Action onClick, float fieldWidth = 100f, string tooltip = null, float indent = 0)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);

            float width = Math.Max(Text.CalcSize(buttonLabel).x + 6f, fieldWidth);

            Rect fieldRect = rect.RightPartPixels(width);
            if (Widgets.ButtonText(fieldRect, buttonLabel))
            {
                onClick();
            }
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }
        public static void ButtonImageLine(this Listing_Standard listing, TaggedString label, Texture2D buttonImage, Action onClick, string tooltip = null, float indent = 0)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);
            Rect fieldRect = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(fieldRect, buttonImage))
            {
                onClick();
            }
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }
        public static void FieldLineReflexion(this Listing_Standard listing, TaggedString label, string fieldName, object instance, Action<object> onChange, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue)
        {
            FieldInfo fieldInfo = instance.GetType().GetField(fieldName);
            if (fieldInfo == null)
            {
                Log.Error($"Field {fieldName} not found in {instance.GetType()}");
                return;
            }

            Type type = fieldInfo.FieldType;
            object value = fieldInfo.GetValue(instance);

            if (type == typeof(int))
            {
                int intValue = (int)value;

                listing.FieldLineOnChange(label, ref intValue, (x) =>
                {
                    onChange?.Invoke(x);
                    fieldInfo.SetValue(instance, x);
                }, fieldWidth, tooltip, indent, min, max);
            }
            else if (type == typeof(float))
            {
                float floatValue = (float)value;

                listing.FieldLineOnChange(label, ref floatValue, (x) =>
                {
                    onChange(x);
                    fieldInfo.SetValue(instance, x);
                }, fieldWidth, tooltip, indent, min, max);

            }
            else if (type == typeof(bool))
            {
                bool boolValue = (bool)value;

                listing.FieldLineOnChange(label, ref boolValue, (x) =>
                {
                    onChange(x);
                    fieldInfo.SetValue(instance, x);
                }, fieldWidth, tooltip, indent);
            }
            else
            {
                Log.Error($"Unsupported type {type} for field {fieldName}");
            }

        }
        public static void LabelLine(this Listing_Standard listing, TaggedString label, string tooltip = null, float indent = 0f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
        }
        public static void FieldLine(this Listing_Standard listing, TaggedString label, ref string value, float fieldWidth = 100f, string tooltip = null, float indent = 0)
        {
            float width = Math.Max(Text.CalcSize(value).x, fieldWidth);

            float maxInputFieldWidth = listing.ColumnWidth / 2f;

            int lineCount = (int)(width / maxInputFieldWidth) + 1;
            Rect rect = listing.GetRect(Text.LineHeight * lineCount);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);

            if (lineCount == 1)
            {
                Rect fieldRect = rect.RightPartPixels(width);
                value = Widgets.TextField(fieldRect, value);
            }
            else
            {
                Rect fieldRect = rect.RightPartPixels(maxInputFieldWidth);
                value = Widgets.TextArea(fieldRect, value);
            }

            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }

        private static Dictionary<int, bool> compCollapsedDic = new Dictionary<int, bool>();
        public static void DrawComp(this Listing_Standard listing, string compName, Action<Listing_Standard> drawComp, int id)
        {
            if (drawComp == null)
            {
                return;
            }

            listing.GapLine(6f);
            //head
            Rect rect = listing.GetRect(Text.LineHeight);

            if (!compCollapsedDic.ContainsKey(id))
            {
                compCollapsedDic[id] = false;
            }

            Rect rect1 = rect.LeftPartPixels(rect.height);
            if (compCollapsedDic[id])
            {
                if (Widgets.ButtonImage(rect1, TexButton.Reveal))
                {
                    compCollapsedDic[id] = false;
                }
            }
            else
            {
                if (Widgets.ButtonImage(rect1, TexButton.Collapse))
                {
                    compCollapsedDic[id] = true;
                }
            }

            Rect rect2 = rect1.RightAdjoin(Text.CalcSize(compName).x);
            Widgets.Label(rect2, "<b>" + compName + "</b>");

            //menu
            //Rect rect3 = rect.RightPartPixels(rect.height);
            //if (Widgets.ButtonImage(rect3, TexButton.Suspend))
            //{
            //    //todo comp options: copy, paste, delete
            //    List<FloatMenuOption> options = new List<FloatMenuOption>();

            //    options.Add(new FloatMenuOption("Copy", () => { /*todo copy comp*/ }));
            //    options.Add(new FloatMenuOption("Paste", () => { /*todo paste comp*/ }));
            //    options.Add(new FloatMenuOption("Delete", () => { /*todo delete comp*/ }));

            //    Find.WindowStack.Add(new FloatMenu(options));
            //}

            //draw
            if (!compCollapsedDic[id])
            {
                listing.Indent(20f);
                listing.ColumnWidth -= 20f;

                drawComp?.Invoke(listing);

                listing.Outdent(20f);
                listing.ColumnWidth += 20f;
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static void ThingDefCountClassLine(this Listing_Standard listing, ThingDefCountClass thingDefCount, Action onDelete, Action preChange, float fieldWidth = 100f, string tooltip = null, float indent = 0f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            ThingDef thingDef = thingDefCount.thingDef;
            int count = thingDefCount.count;

            //icon
            Rect rect4 = rect.LeftPartPixels(rect.height);
            Widgets.DrawTextureFitted(rect4, thingDef.uiIcon, 1f);

            //label
            Rect rect5 = rect4.RightAdjoin(rect.width - rect.height);
            Widgets.Label(rect5, thingDef?.LabelCap ?? "MP_Null".Translate());

            //delete
            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Delete))
            {
                preChange?.Invoke();
                onDelete?.Invoke();
            }

            //count
            Rect rect2 = rect1.LeftAdjoin(100f);
            WidgetsUtility.TextFieldOnChange(rect2, ref count, (newValue) =>
            {
                preChange?.Invoke();
                thingDefCount.count = newValue;
            });

            Rect rect3 = rect2.LeftAdjoin(Text.CalcSize("x").x);
            Widgets.Label(rect3, "x");

            listing.Gap(listing.verticalSpacing);
        }
    }
}
