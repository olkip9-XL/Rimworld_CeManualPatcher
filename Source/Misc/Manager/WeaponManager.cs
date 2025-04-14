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
        public static WeaponManager instance { get; private set; }

        private CEPatchManager patchManager => CEPatchManager.instance;

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
            WeaponPatch result = weaponPatches.Find(x => x?.weaponDef == thingDef);

            if (result == null)
            {
                try
                {
                    result = new WeaponPatch(thingDef);
                    weaponPatches.Add(result);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] WeaponManager Trying create patch for item {thingDef?.defName ?? "Null"}: {e}");
                }
            }

            return result;
        }
        public bool HasWeaponPatch(ThingDef thingDef)
        {
            return weaponPatches.Any(x => x?.weaponDef == thingDef);
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
            Scribe_Collections.Look(ref weaponPatches, "weaponPatches", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (weaponPatches == null)
                {
                    weaponPatches = new List<WeaponPatch>();
                }
            }
        }

        public override void Reset(ThingDef thing)
        {
            WeaponPatch weaponPatch = weaponPatches.Find(x => x.weaponDef == thing);
            if (weaponPatch != null)
            {
                weaponPatch.Reset();
                weaponPatches.Remove(weaponPatch);
                patchManager.Reset(thing);
            }
        }

        public override void ResetAll()
        {
            weaponPatches.ForEach(x => x.Reset());
            weaponPatches.Clear();
            patchManager.ResetAll();
        }

        public override void PostLoadInit()
        {
            foreach(var item in this.weaponPatches)
            {
                try
                {
                    item.PostLoadInit();
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] WeaponManager PostLoadInit error on item {item.weaponDef?.defName ?? "Null"} : {e}");
                }
            }

        }
    }
}
