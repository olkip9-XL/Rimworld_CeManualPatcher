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
    internal class HediffDefManager : MP_DefManagerBase<HediffDef>
    {
        public static HediffDef curHediffDef = null;

        public static HediffDefManager instance { get; private set; }

        //rect
        private Rect_HediffInfo rect_HediffInfo = new Rect_HediffInfo();
        private Rect_HediffList rect_HediffList = new Rect_HediffList();


        public HediffDefManager()
        {
            HediffDefManager.instance = this;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect rightRect = rect.RightPart(0.7f);
            Rect leftRect = rect.LeftPart(0.3f);
            leftRect.width -= 20f;

            try
            {
                rect_HediffInfo.DoWindowContents(rightRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("HediffDefManager hediff info error", e, curHediffDef);
            }

            try
            {
                rect_HediffList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("HediffDefManager hediff list error", e);
            }
        }

        protected override void NewPatch(ref PatchBase<HediffDef> patch, HediffDef def)
        {
            patch = new HediffDefPatch(def);
        }
       
    }
}
