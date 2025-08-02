using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


namespace CeManualPatcher.RenderRect
{
    internal abstract class RenderRectBase
    {
        public abstract void DoWindowContents(Rect rect);
    }
}
