using CeManualPatcher.RenderRect;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    internal class WeaponManager : MP_DefManagerBase
    {

        public static ThingDef curWeaponDef = null;
        public static WeaponManager instance;

        //字段
        private List<WeaponPatch> weaponPatches = new List<WeaponPatch>();

        //rect
        private Rect_WeaponInfo rect_WeaponInfo = new Rect_WeaponInfo();
        private Rect_WeaponList rect_WeaponList = new Rect_WeaponList();

        public WeaponManager()
        {
            WeaponManager.instance = this;
        }

        public WeaponPatch GetWeaponPatch(ThingDef thingDef)
        {
            WeaponPatch result = weaponPatches.Find(x => x?.Def == thingDef);

            if (result == null)
            {
                result = new WeaponPatch(thingDef);
                weaponPatches.Add(result);
            }

            needApply = true;
            return result;
        }


        public override void Apply(ThingDef thing)
        {
            weaponPatches.Find(x => x.Def == thing)?.Apply();
        }

        public override void ApplyAll()
        {
            weaponPatches.ForEach(x => x.Apply());
        }

        public override void DoWindowContents(Rect rect)
        {

            Rect rightRect = rect.RightPart(0.7f);
            Rect leftRect = rect.LeftPart(0.3f);
            leftRect.width -= 20f;

            rect_WeaponInfo.DoWindowContents(rightRect);
            rect_WeaponList.DoWindowContents(leftRect);

            if (needApply)
            {
                Apply(curWeaponDef);
                needApply = false;
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref weaponPatches, "weaponPatches", LookMode.Deep);
        }

        public override void Reset(ThingDef thing)
        {
            WeaponPatch weaponPatch = weaponPatches.Find(x => x.Def == thing);
            if (weaponPatch != null)
            {
                weaponPatch.Reset();
                weaponPatches.Remove(weaponPatch);
            }
        }

        public override void ResetAll()
        {
            weaponPatches.ForEach(x => x.Reset());
            weaponPatches.Clear();
        }

        public override void PostLoadInit()
        {
            this.weaponPatches.ForEach(x => x.PostLoadInit());
        }
    }
}
