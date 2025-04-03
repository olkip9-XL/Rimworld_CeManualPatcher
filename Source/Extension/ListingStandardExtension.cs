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
        public static void TextfieldX<T>(this Listing_Standard listing, string label, float fieldWidth, T value, Action<T> onChange, float indent = 0f, string tooltip = null, float min = 0f, float max = 1E+09f) where T : struct
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect fieldRect = rect.RightPartPixels(fieldWidth);

            Widgets.Label(rect, label);
          
            WidgetsUtility.TextFieldOnChange(fieldRect, value, onChange, min, max);

            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static void SearchBar(this Listing_Standard listing, ref string keyWords)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect fieldRect = rect.LeftPartPixels(rect.width - rect.height);
            Rect iconRect = rect.RightPartPixels(rect.height);

            keyWords = Widgets.TextField(fieldRect, keyWords);

            Widgets.DrawTextureFitted(iconRect, TexButton.Search, 0.7f);

            listing.Gap(listing.verticalSpacing);
        }

        public static void ButtonX(this Listing_Standard listing, string label, float buttonWidth, string buttonLabel, Action onClick, float indent = 0f, string tooltip= null)
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
        
        public static void ChenkBoxX(this Listing_Standard listing, string label,  bool value, Action<bool> onChange, float indent = 0f, string tooltip = null)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            bool tempValue = value;
            Widgets.CheckboxLabeled(rect, label, ref tempValue);
            if(tempValue != value)
            {
                onChange(tempValue);
            }
            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            listing.Gap(listing.verticalSpacing);
        }
        
        public static void TextX(this Listing_Standard listing, string label,float inputWidth, ref string value, float indent = 0f, string tooltip = null)
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

        //专用
        public static void StatDefX(this Listing_Standard listing, StatModifier statModifier, float inputWidth, Action<float> onChange, Action onDelete, float indent = 0f, string tooltip = null)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect buttonRect = rect.RightPartPixels(rect.height);
            Rect fieldRect = new Rect { x = rect.x + rect.width - rect.height - 6f - inputWidth, y = rect.y, width = inputWidth, height = rect.height };

            Widgets.Label(rect, statModifier.stat.LabelCap);

            WidgetsUtility.TextFieldOnChange(fieldRect, statModifier.value, onChange);

            if (Widgets.ButtonImage(buttonRect, TexButton.Delete))
            {
                onDelete();
            }

            Widgets.DrawHighlightIfMouseover(rect);

            listing.Gap(listing.verticalSpacing);
        }

        public static void DamageRow(this Listing_Standard listing, string damageLabel, float buttonWidth, Action onClick, int damageAmount, float fieldWidth, Action<int> onChange, Action onDelete = null, float indent = 0f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect buttonRect = rect.LeftPartPixels(buttonWidth);
            Rect fieldRect = new Rect(buttonRect.xMax+ 6f, rect.y, fieldWidth, rect.height);
            Rect deleteRect = rect.RightPartPixels(rect.height);

            Widgets.Label(rect, "Damage:");

            if (Widgets.ButtonText(buttonRect, damageLabel))
            {
                onClick();
            }

            WidgetsUtility.TextFieldOnChange(fieldRect, damageAmount, onChange);

            if(onDelete != null)
            {
                if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
                {
                    onDelete();
                }
            }

            listing.Gap(listing.verticalSpacing);
        }

        public static void TestField(this Listing_Standard listing, FieldInfoWarpper warpper, float indent = 0f, string tooltip = null, float min = 0f, float max = 1E+09f)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += indent;
            rect.width -= indent;

            Rect fieldRect = rect.RightPartPixels(warpper.fieldWidth);

            if (warpper.FieldType == typeof(int))
            {
                Widgets.Label(rect, warpper.Label);
                int tempValue = (int)warpper.Value;
                WidgetsUtility.TextFieldOnChange(fieldRect, tempValue, newValue => warpper.ValueSetter(newValue), min, max);
            }
            else if (warpper.FieldType == typeof(float))
            {
                Widgets.Label(rect, warpper.Label);
                float tempValue = (float)warpper.Value;
                WidgetsUtility.TextFieldOnChange(fieldRect, tempValue, newValue => warpper.ValueSetter(newValue), min, max);
            }
            else if (warpper.FieldType == typeof(bool))
            {
                bool tempValue = (bool)warpper.Value;
                Widgets.CheckboxLabeled(rect, warpper.Label, ref tempValue);
                warpper.ValueSetter(tempValue);
            }
            else
            {
                Widgets.Label(rect, warpper.Label);
                if (Widgets.ButtonText(fieldRect, warpper.ValueLabelGetter(warpper.Value)))
                {
                    warpper.ValueSetter(warpper.instance);
                }
            }

            Widgets.DrawHighlightIfMouseover(rect);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            listing.Gap(listing.verticalSpacing);
        }

    }
}
