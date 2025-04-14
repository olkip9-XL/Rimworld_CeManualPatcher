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
    internal class AmmoManager : MP_DefManagerBase
    {
        public static MP_AmmoSet curAmmoSet;
        public static AmmoManager instance;

        private List<AmmoPatch> ammoPatches = new List<AmmoPatch>();

        private Rect_AmmoList rect_AmmoList = new Rect_AmmoList();
        private Rect_AmmoInfo rect_AmmoInfo = new Rect_AmmoInfo();

        public AmmoPatch GetAmmoPatch(MP_Ammo ammo)
        {
            AmmoPatch result = ammoPatches.Find(x => x?.projectileDef == ammo.projectile);
            if (result == null)
            {
                try
                {
                    result = new AmmoPatch(ammo.projectile, ammo.ammo);
                    ammoPatches.Add(result);
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
            return ammoPatches.Any(x => ammoSet.ammoList.Any(y => y.projectile == x.projectileDef));
        }

        public bool HasAmmoPatch(MP_Ammo ammo)
        {
            return ammoPatches.Any(x => x.projectileDef == ammo.projectile);
        }

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


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref ammoPatches, "ammoPatches", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ammoPatches == null)
                {
                    ammoPatches = new List<AmmoPatch>();
                }
            }
        }

        public override void Reset(ThingDef thing)
        {
            AmmoPatch ammoPatch = this.ammoPatches.Find(x => x?.projectileDef == thing);
            if (ammoPatch != null)
            {
                ammoPatch.Reset();
            }
            ammoPatches.Remove(ammoPatch);
        }

        public override void ResetAll()
        {
            ammoPatches.ForEach(x => x.Reset());
            ammoPatches.Clear();
        }

        public override void PostLoadInit()
        {
            foreach(var item in this.ammoPatches)
            {
                try
                {
                    item?.PostLoadInit();
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] AmmoManager PostLoadInit error on item {item.projectileDef?.defName ?? "Null"}: {e}");
                }
            }
        }
    }
}
