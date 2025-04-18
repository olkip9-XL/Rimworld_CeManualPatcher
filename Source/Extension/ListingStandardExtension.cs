using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
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
        public static void FieldLine<T>(this Listing_Standard listing, string label, ref T value, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue) where T : struct
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
                value = (T)(object)temp;
            }
            else
            {
                Rect fieldRect = rect.RightPartPixels(fieldWidth);
                string buffer = value.ToString();

                Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);

                if (buffer.NullOrEmpty())
                {
                    value = default(T);
                }
            }

            Widgets.DrawHighlightIfMouseover(rect);

            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static void FieldLineOnChange<T>(this Listing_Standard listing, string label, ref T value, Action<T> onChange, float fieldWidth = 100f, string tooltip = null, float indent = 0, float min = float.MinValue, float max = float.MaxValue) where T : struct
        {
            T temp = value;
            FieldLine(listing, label, ref temp, fieldWidth, tooltip, indent, min, max);
            if (temp.ToString() != value.ToString())
            {
                onChange(temp);
                value = temp;
            }
        }


        public static void ButtonTextLine(this Listing_Standard listing, string label, string buttonLabel, Action onClick, float fieldWidth = 100f, string tooltip = null, float indent = 0)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Widgets.Label(rect, label);
            Rect fieldRect = rect.RightPartPixels(fieldWidth);
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
                listing.FieldLine(label, ref intValue, fieldWidth, tooltip, indent, min, max);
                if (intValue != (int)value)
                {
                    onChange(intValue);
                    fieldInfo.SetValue(instance, intValue);
                }
            }
            else if (type == typeof(float))
            {
                float floatValue = (float)value;
                listing.FieldLine(label, ref floatValue, fieldWidth, tooltip, indent, min, max);
                if (Math.Abs(floatValue - (float)value) > float.Epsilon)
                {
                    onChange(floatValue);
                    fieldInfo.SetValue(instance, floatValue);
                }
            }
            else if (type == typeof(bool))
            {
                bool boolValue = (bool)value;
                listing.FieldLine(label, ref boolValue, fieldWidth, tooltip, indent);
                if (boolValue != (bool)value)
                {
                    onChange(boolValue);
                    fieldInfo.SetValue(instance, boolValue);
                }
            }
            else
            {
                Log.Error($"Unsupported type {type} for field {fieldName}");
            }

        }

    }
}
