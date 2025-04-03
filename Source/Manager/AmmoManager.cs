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

        private ThingDef curProjectile = null;
        public AmmoPatch GetAmmoPatch(ThingDef thing)
        {
            AmmoPatch result = ammoPatches.Find(x => x?.projectileDef == thing);
            if (result == null)
            {
                result = new AmmoPatch(thing);
                ammoPatches.Add(result);
            }
            needApply = true;
            curProjectile = thing;
            return result;
        }

        public bool HasAmmoPatch(MP_AmmoSet ammoSet)
        {
            return ammoPatches.Any(x => ammoSet.ammoList.Any(y=> y.projectile == x.projectileDef));
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

            if(needApply)
            {
                Apply(curProjectile);
                needApply = false;
                curProjectile = null;
            }
        }

        public override void Apply(ThingDef thing)
        {
            AmmoPatch ammoPatch = this.ammoPatches.Find(x => x?.projectileDef == thing);
            if(ammoPatch != null)
            {
                ammoPatch.Apply();
            }
        }

        public override void ApplyAll()
        {
            ammoPatches.ForEach(x => x.Apply());
        }

        public override void ExposeData()
        {
            //todo 
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
            foreach (var item in ammoPatches)
            {
                item.PostLoadInit();
            }
        }
    }
}
