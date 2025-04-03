using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CeManualPatcher.Extension
{
    public static class RectExtension
    {
        public static Rect LeftAdjoin(this Rect rect, float width, float gap =6f)
        {
            return new Rect(rect.x - width -gap, rect.y, width, rect.height);
        }

        public static Rect RightAdjoin(this Rect rect, float width, float gap = 6f)
        {
            return new Rect(rect.x + rect.width + gap, rect.y, width, rect.height);
        }

        public static Rect TopAdjoin(this Rect rect, float height, float gap = 6f)
        {
            return new Rect(rect.x, rect.y - height - gap, rect.width, height);
        }

        public static Rect BottomAdjoin(this Rect rect, float height, float gap = 6f)
        {
            return new Rect(rect.x, rect.y + rect.height + gap, rect.width, height);
        }

        public static Rect CenterIn(this Rect rect, Rect outRect)
        {
            float x = outRect.x + (outRect.width - rect.width) / 2;
            float y = outRect.y + (outRect.height - rect.height) / 2;
            return new Rect(x, y, rect.width, rect.height);
        }
    }
}
