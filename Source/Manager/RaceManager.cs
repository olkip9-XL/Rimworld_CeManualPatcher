using CeManualPatcher.Misc;
using CeManualPatcher.Patch;
using CeManualPatcher.RenderRect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    internal class RaceManager : MP_DefManagerBase<ThingDef>
    {
        public static ThingDef curDef = null;
        public static RaceManager instance { get; private set; }

        public RaceManager()
        {
            instance = this;
        }

        //rect
        Rect_RaceInfo rect_RaceInfo = new Rect_RaceInfo();
        Rect_RaceList rect_RaceList = new Rect_RaceList();

        public override void DoWindowContents(Rect rect)
        {
            Rect rightRect = rect.RightPart(0.7f);
            Rect leftRect = rect.LeftPart(0.3f);
            leftRect.width -= 20f;

            try
            {
                rect_RaceInfo.DoWindowContents(rightRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("RaceManager info error", e, curDef);
            }

            try
            {
                rect_RaceList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("RaceManager list error", e);
            }
        }

        protected override void NewPatch(ref PatchBase<ThingDef> patch, ThingDef def)
        {
            patch = new RacePatch(def);
        }

        public bool HasPatch(ThingDef def)
        {
            return patches.Any(x => x?.targetDef == def);
        }
    }
}
