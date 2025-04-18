using CeManualPatcher.Manager;
using CeManualPatcher.Misc.Patch;
using CeManualPatcher.RenderRect;
using CeManualPatcher.RenderRect.Ammo;
using CeManualPatcher.RenderRect.Apparel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace CeManualPatcher.Misc.Manager
{
    internal class ApparelManager : MP_DefManagerBase
    {
        internal static ThingDef curApparelDef;
        internal static ApparelManager instance;

        private List<ApparelPatch> patches = new List<ApparelPatch>();
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

            rect_ApparelList.DoWindowContents(leftRect);
            rect_ApparelInfo.DoWindowContents(rightRect);
        }

        public ApparelPatch GetPatch(ThingDef thing)
        {
            ApparelPatch patch = patches.FirstOrDefault(x => x?.apparelDef == thing);
            if (patch == null)
            {
                try
                {
                    patch = new ApparelPatch(thing);
                    patches.Add(patch);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] ApparelManager Trying create patch for item {thing?.defName ?? "Null"}: {e}");
                }
            }

            return patch;
        }

        public bool HasPatch(ThingDef thing)
        {
            return patches.Any(x => x?.apparelDef == thing);
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref patches, "apparelPatches", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (patches == null)
                {
                    patches = new List<ApparelPatch>();
                }
            }
        }

        public override void PostLoadInit()
        {
            patches.ForEach(x => x?.PostLoadInit());
        }

        public override void Reset(ThingDef thing)
        {
            ApparelPatch patch = patches.FirstOrDefault(x => x?.apparelDef == thing);
            if (patch != null)
            {
                patch.Reset();
                patches.Remove(patch);
            }
        }

        public override void ResetAll()
        {
            foreach (var patch in patches)
            {
                patch.Reset();
            }
            patches.Clear();
        }

        public void ExportAll()
        {
            foreach (var patch in patches)
            {
               patch?.ExportPatch(MP_DefManagerBase.exportPath);
            }   
        }
    }
}
