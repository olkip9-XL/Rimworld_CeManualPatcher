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
    internal class ApparelManager : MP_DefManagerBase<ThingDef>
    {
        internal static ThingDef curApparelDef;
        internal static ApparelManager instance;

        //private List<ApparelPatch> patches = new List<ApparelPatch>();
        private Rect_ApparelList rect_ApparelList = new Rect_ApparelList();
        private Rect_ApparelInfo rect_ApparelInfo = new Rect_ApparelInfo();

        internal ApparelManager()
        {
            ApparelManager.instance = this;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect leftRect = rect.LeftPart(0.3f);
            Rect rightRect = rect.RightPart(0.7f);
            leftRect.width -= 20f;

            try
            {
                rect_ApparelList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("ApparelManager apparel list error", e);
            }

            try
            {
                rect_ApparelInfo.DoWindowContents(rightRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("ApparelManager apparel info error", e, curApparelDef);
            }
        }

        protected override void NewPatch(ref PatchBase<ThingDef> patch, ThingDef def)
        {
            patch = new ApparelPatch(def);
        }

        public bool HasPatch(ThingDef thing)
        {
            return patches.Any(x => x?.targetDef == thing);
        }


        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<ApparelPatch> apparelPatches = new List<ApparelPatch>();
                Scribe_Collections.Look(ref apparelPatches, "apparelPatches", LookMode.Deep);
                if(!apparelPatches.NullOrEmpty())
                {
                    patches.AddRange(apparelPatches);
                }
            }
        }
 
    }
}
