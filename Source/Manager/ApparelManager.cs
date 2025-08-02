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
                //Log.ErrorOnce($"[CeManualPatcher] ApparelManager apparel info error on thing {curApparelDef?.defName ?? "null"} form {curApparelDef?.modContentPack?.Name ?? "null"} : {e}", e.GetHashCode());
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
            //Scribe_Collections.Look(ref patches, "apparelPatches", LookMode.Deep);
            //if (Scribe.mode == LoadSaveMode.LoadingVars)
            //{
            //    if (patches == null)
            //    {
            //        patches = new List<ApparelPatch>();
            //    }
            //}
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

        //public override void PostLoadInit()
        //{
        //    foreach (var patch in patches)
        //    {
        //        try
        //        {
        //            patch?.PostLoadInit();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] PostLoadInit apparel patch {patch?.apparelDef?.defName} failed : {e}");
        //        }
        //    }
        //}

        //public override void Reset(ThingDef thing)
        //{
        //    ApparelPatch patch = patches.FirstOrDefault(x => x?.apparelDef == thing);
        //    if (patch != null)
        //    {
        //        try
        //        {
        //            patch.Reset();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] Resetting apparel patch {patch?.apparelDef?.defName} failed : {e}");
        //        }
        //        patches.Remove(patch);
        //    }
        //}

        //public override void ResetAll()
        //{
        //    foreach (var patch in patches)
        //    {
        //        try
        //        {
        //            patch.Reset();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] Resetting apparel patch {patch?.apparelDef?.defName} failed : {e}");
        //        }
        //    }
        //    patches.Clear();
        //}

        //public void ExportAll()
        //{
        //    foreach (var patch in patches)
        //    {
        //        try
        //        {
        //            patch?.ExportPatch(MP_DefManagerBase.exportPath);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] Exporting apparel patch {patch?.apparelDef?.defName} failed : {e}");
        //        }
        //    }
        //}
    }
}
