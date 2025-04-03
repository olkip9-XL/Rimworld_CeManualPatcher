using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc
{
    public static class WidgetsUtility
    {
        public static void TextFieldOnChange<T>(Rect rect, T value, Action<T> onChange, float min = 0f, float max = 1E+09f) where T : struct
        {
            T tempValue = value;
            string buffer = value.ToString();

            Widgets.TextFieldNumeric(rect, ref tempValue, ref buffer);

            if(buffer.NullOrEmpty())
            {
                tempValue = default(T);
            }

            if (!EqualityComparer<T>.Default.Equals(tempValue, value))
            {
                onChange(tempValue);
            }
        }
    
        public static void ScrollView(Rect outRect, ref Vector2 scrollPosition, ref float innerHeight, Action<Listing_Standard> action, float maxHeight = float.MaxValue)
        {
            Rect innerRect = new Rect(0,0, outRect.width - 16, innerHeight);
            Listing_Standard listing = new Listing_Standard();

            Widgets.BeginScrollView(outRect, ref scrollPosition, innerRect, true);

            innerRect.height = maxHeight;

            listing.Begin(innerRect);

            action(listing);

            innerHeight = listing.CurHeight;

            listing.End();

            Widgets.EndScrollView();
        }
    }
}
