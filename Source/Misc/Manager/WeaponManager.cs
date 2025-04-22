using CeManualPatcher.Patch;
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
    internal class WeaponManager : MP_DefManagerBase<ThingDef>
    {

        public static ThingDef curWeaponDef = null;
        public static WeaponManager instance { get; private set; }

        //rect
        private Rect_WeaponInfo rect_WeaponInfo = new Rect_WeaponInfo();
        private Rect_WeaponList rect_WeaponList = new Rect_WeaponList();

        public WeaponManager()
        {
            WeaponManager.instance = this;
        }

        public WeaponPatch GetWeaponPatch(ThingDef thingDef)
        {
            return GetPatch(thingDef) as WeaponPatch;
        }
        protected override void NewPatch(ref PatchBase<ThingDef> patch, ThingDef def)
        {
            patch = new WeaponPatch(def);
        }

        public bool HasWeaponPatch(ThingDef thingDef)
        {
            return patches.Any(x => x?.targetDef == thingDef);
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect rightRect = rect.RightPart(0.7f);
            Rect leftRect = rect.LeftPart(0.3f);
            leftRect.width -= 20f;

            rect_WeaponInfo.DoWindowContents(rightRect);
            rect_WeaponList.DoWindowContents(leftRect);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<WeaponPatch> weaponPatches = new List<WeaponPatch>();
                Scribe_Collections.Look(ref weaponPatches, "weaponPatches", LookMode.Deep);
                if (!weaponPatches.NullOrEmpty())
                {
                    foreach(var item in weaponPatches)
                    {
                        patches.Add(item);
                    }
                }
            }
        }
    }
}
