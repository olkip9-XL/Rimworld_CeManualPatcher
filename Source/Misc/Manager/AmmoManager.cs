using CeManualPatcher.Misc;
using CeManualPatcher.Patch;
using CeManualPatcher.RenderRect.Ammo;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    internal class AmmoManager : MP_DefManagerBase<ThingDef>
    {
        public static MP_AmmoSet curAmmoSet;
        public static AmmoManager instance;

        private Rect_AmmoList rect_AmmoList = new Rect_AmmoList();
        private Rect_AmmoInfo rect_AmmoInfo = new Rect_AmmoInfo();

        public AmmoManager()
        {
            AmmoManager.instance = this;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect leftRect = rect.LeftPart(0.3f);
            Rect rightRect = rect.RightPart(0.7f);
            leftRect.width -= 20f;

            rect_AmmoList.DoWindowContents(leftRect);
            rect_AmmoInfo.DoWindowContents(rightRect);
        }

        protected override void NewPatch(ref PatchBase<ThingDef> patch, ThingDef def)
        {
        }


        public AmmoPatch GetAmmoPatch(MP_Ammo ammo)
        {
            AmmoPatch result =(AmmoPatch)patches.Find(x => x?.targetDef == ammo.projectile);
            if (result == null)
            {
                try
                {
                    result = new AmmoPatch(ammo.projectile, ammo.ammo);
                    patches.Add(result);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] AmmoManager Trying create patch for item {ammo.projectile?.defName ?? "Null"}, ammo: {ammo.ammo?.defName ?? "Null"}: {e}");
                }
            }
            return result;
        }

        public bool HasAmmoPatch(MP_AmmoSet ammoSet)
        {
            return patches.Any(x => ammoSet.ammoList.Any(y => y.projectile == x.targetDef));
        }

        public bool HasAmmoPatch(MP_Ammo ammo)
        {
            return patches.Any(x => x.targetDef == ammo.projectile);
        }



        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Collections.Look(ref ammoPatches, "ammoPatches", LookMode.Deep);
            //if (Scribe.mode == LoadSaveMode.LoadingVars)
            //{
            //    if (ammoPatches == null)
            //    {
            //        ammoPatches = new List<AmmoPatch>();
            //    }
            //}
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<AmmoPatch> ammoPatches = new List<AmmoPatch>();
                Scribe_Collections.Look(ref ammoPatches, "ammoPatches", LookMode.Deep);
                if (!ammoPatches.NullOrEmpty())
                {
                    patches.AddRange(ammoPatches);
                }
            }
        }

        //public override void Reset(ThingDef thing)
        //{
        //    AmmoPatch ammoPatch = this.ammoPatches.Find(x => x?.targetDef == thing);
        //    if (ammoPatch != null)
        //    {
        //        try
        //        {
        //            ammoPatch.Reset();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] Resetting ammo patch {ammoPatch.targetDef?.defName ?? "Null"} failed : {e}");
        //        }
        //        ammoPatches.Remove(ammoPatch);
        //    }
        //}

        //public override void ResetAll()
        //{
        //    foreach(var item in this.ammoPatches)
        //    {
        //        try
        //        {
        //            item?.Reset();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] Resetting ammo patch {item.targetDef?.defName ?? "Null"} failed : {e}");
        //        }
        //    }
        //    ammoPatches.Clear();
        //}

        //public override void PostLoadInit()
        //{
        //    foreach(var item in this.ammoPatches)
        //    {
        //        try
        //        {
        //            item?.PostLoadInit();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error($"[CeManualPatcher] AmmoManager PostLoadInit error on item {item.targetDef?.defName ?? "Null"}: {e}");
        //        }
        //    }
        //}
    }
}
