using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Manager;
using CeManualPatcher.Misc.Patch;
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
        protected override void NewPatch(ref PatchBase<ThingDef> patch, ThingDef def)
        {
            patch = new WeaponPatch(def);
        }

        public WeaponPatch GetWeaponPatch(ThingDef thingDef)
        {
            return base.GetPatch(thingDef) as WeaponPatch;
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

            try
            {
                rect_WeaponInfo.DoWindowContents(rightRect);
            }
            catch (Exception e)
            {
                //Log.ErrorOnce($"[CeManualPatcher] WeaponManager weapon info error on thing {curWeaponDef?.defName ?? "null"} form {curWeaponDef?.modContentPack?.Name ?? "null"} : {e}", curWeaponDef?.GetHashCode() ?? e.GetHashCode());
                MP_Log.Error("WeaponManager weapon info error", e, curWeaponDef);
            }

            try
            {
                rect_WeaponList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                //Log.ErrorOnce($"[CeManualPatcher] WeaponManager weapon list error : {e}", curWeaponDef?.GetHashCode() ?? e.GetHashCode());
                MP_Log.Error("WeaponManager weapon list error", e);
            }

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
                    foreach (var item in weaponPatches)
                    {
                        patches.Add(item);
                    }
                }
            }
        }
    }
}
