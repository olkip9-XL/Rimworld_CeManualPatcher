using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Weapon
{
    internal class WeaponTagsSaveable : SaveableBase
    {
        public static readonly string tagOneHandWeaponCE = "CE_OneHandedWeapon";

        List<string> saveData = new List<string>();

        List<String> WeaponTags
        {
            get
            {
                if (thingDef == null || thingDef.weaponTags == null)
                {
                    return null;
                }
                return thingDef.weaponTags;
            }
        }

        List<string> originalData = new List<string>();

        public WeaponTagsSaveable() { }
        public WeaponTagsSaveable(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (thingDef == null || thingDef.weaponTags == null)
            {
                return;
            }

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && WeaponTags != null)
            {
                saveData.Clear();
                saveData.AddRange(WeaponTags);
            }

            Scribe_Collections.Look(ref saveData, "weaponTags", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (saveData == null)
                {
                    saveData = new List<string>();
                }
            }

        }

        public override void Reset()
        {
            if (WeaponTags == null)
                return;

            WeaponTags.Clear();
            WeaponTags.AddRange(originalData);
        }

        protected override void Apply()
        {
            if (WeaponTags == null)
            {
                return;
            }

            WeaponTags.Clear();
            WeaponTags.AddRange(saveData);
        }

        protected override void InitOriginalData()
        {
            if (WeaponTags == null)
            {
                return;
            }

            originalData.Clear();
            originalData.AddRange(WeaponTags);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (WeaponTags == null)
            {
                return;
            }

            InitOriginalData();
            this.Apply();
        }


        //help function
        public static void SetBipod(ThingDef thing, BipodCategoryDef bipodDef)
        {
            if (thing == null && thing.weaponTags == null)
            {
                return;
            }

            thing.weaponTags.RemoveWhere(x => MP_Options.BipodId.Contains(x));
            if (bipodDef != null)
                thing.weaponTags.Add(bipodDef.bipod_id);
        }

        public static BipodCategoryDef GetBipod(ThingDef thing)
        {
            if (thing == null || thing.weaponTags.NullOrEmpty())
            {
                return null;
            }

            string bipodId = thing.weaponTags.FirstOrDefault(x => MP_Options.BipodId.Contains(x));

            return MP_Options.bipodCategoryDefs.FirstOrDefault(x => x.bipod_id == bipodId);
        }
        public static bool GetOneHandWeapon(ThingDef thing)
        {
            if (thing == null || thing.weaponTags == null)
            {
                return false;
            }
            return thing.weaponTags.Contains(tagOneHandWeaponCE);
        }
        public static void SetOneHandWeapon(ThingDef thing, bool isOneHand)
        {
            if (thing == null) return;

            // Update tag
            if (isOneHand)
                thing.weaponTags.AddDistinct(tagOneHandWeaponCE);
            else
                thing.weaponTags.Remove(tagOneHandWeaponCE);

            // Update stat
            var stat = thing.statBases.FirstOrDefault(x => x.stat == CE_StatDefOf.OneHandedness);
            if (stat == null && isOneHand)
            {
                thing.statBases.Add(new StatModifier
                {
                    stat = CE_StatDefOf.OneHandedness,
                    value = 1f
                });
            }
            else if (stat != null)
            {
                stat.value = isOneHand ? 1f : 0f;
            }
        }


    }
}
