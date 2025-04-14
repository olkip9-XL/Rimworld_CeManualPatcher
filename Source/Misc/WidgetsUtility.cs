using CeManualPatcher.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CeManualPatcher.Misc
{
    public static class WidgetsUtility
    {
        private static Dictionary<int, bool> labelStats = new Dictionary<int, bool>();

        public static void TextFieldOnChange<T>(Rect rect, ref T value, Action<T> onChange, float min = float.MinValue, float max = float.MaxValue) where T : struct
        {
            T temp = value;
            string buffer = value.ToString();

            Widgets.TextFieldNumeric(rect, ref temp, ref buffer, min, max);

            if (buffer.NullOrEmpty())
            {
                temp = default(T);
            }

            //if(Math.Abs(value- temp)< float.Epsilon)
            if (value.ToString() != buffer)
            {
                onChange(temp);
                value = temp;
            }
        }

        public static void ScrollView(Rect outRect, ref Vector2 scrollPosition, ref float innerHeight, Action<Listing_Standard> action, float maxHeight = float.MaxValue)
        {
            Rect innerRect = new Rect(0, 0, outRect.width - 16, innerHeight);
            Listing_Standard listing = new Listing_Standard();

            Widgets.BeginScrollView(outRect, ref scrollPosition, innerRect, true);

            innerRect.height = maxHeight;

            listing.Begin(innerRect);

            action(listing);

            innerHeight = listing.CurHeight;

            listing.End();

            Widgets.EndScrollView();
        }

        public static void LabelChange(Rect rect, ref string value, int id, Action onClick = null, float inputFieldWidth = 200f)
        {
            if (!labelStats.ContainsKey(id))
            {
                labelStats[id] = true;
            }

            float labelWidth = Text.CalcSize(value).x;

            if (labelStats[id])
            {
                //normal label
                Widgets.Label(rect, value);

                Rect buttonRect = new Rect(rect.x + labelWidth + 6f, rect.y, rect.height, rect.height);
                if (Widgets.ButtonImage(buttonRect, TexButton.Rename))
                {
                    List<int> keys = labelStats.Keys.ToList();
                    foreach (int key in keys)
                    {
                        labelStats[key] = true;
                    }

                    labelStats[id] = false;
                    if (onClick != null)
                    {
                        onClick();
                    }
                }
            }
            else
            {
                //input field
                float width = Math.Max(labelWidth, inputFieldWidth);

                Rect inputFieldRect = new Rect(rect.x, rect.y, width, rect.height);
                string temp = Widgets.TextField(inputFieldRect, value);
                if(temp != value)
                {
                    value = temp;
                    if (onClick != null)
                        onClick();
                }

                Rect buttonRect = inputFieldRect.RightAdjoin(rect.height);

                if (Widgets.ButtonImage(buttonRect, Widgets.CheckboxOnTex))
                {
                    labelStats[id] = true;
                }
            }


        }

    }
}
