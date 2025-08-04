using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class WeaponTagsSaveable : SaveableBase<ThingDef>
    {
        public static readonly string tagOneHandWeaponCE = "CE_OneHandedWeapon";

        //字段
        List<string> saveData = new List<string>();

        List<String> WeaponTags
        {
            get
            {
                if (def == null || def.weaponTags == null)
                {
                    return null;
                }
                return def.weaponTags;
            }
        }

        List<string> originalData = new List<string>();

        public List<string> OriginalTags => originalData;

        public WeaponTagsSaveable() { }
        public WeaponTagsSaveable(ThingDef thingDef) : base(thingDef)
        {
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

        protected override bool NullCheck()
        {
            return this.WeaponTags == null;
        }


        //help function

        //bipod
        public static void SetBipod(List<string> weaponTags, BipodCategoryDef bipodDef)
        {
            if (weaponTags == null)
            {
                return;
            }

            weaponTags.RemoveWhere(x => MP_Options.BipodId.Contains(x));
            if (bipodDef != null)
                weaponTags.Add(bipodDef.bipod_id);
        }
        public static BipodCategoryDef GetBipod(List<string> weaponTags)
        {
            if (weaponTags.NullOrEmpty())
            {
                return null;
            }

            string bipodId = weaponTags.FirstOrDefault(x => MP_Options.BipodId.Contains(x));

            return MP_Options.bipodCategoryDefs.FirstOrDefault(x => x.bipod_id == bipodId);
        }

        //One hand
        public static bool GetOneHandWeapon(List<string> weaponTags)
        {
            if (weaponTags == null)
            {
                return false;
            }
            return weaponTags.Contains(tagOneHandWeaponCE);
        }
        public static void SetOneHandWeapon(List<string> weaponTags, bool isOneHand)
        {
            if (weaponTags == null)
                return;

            // Update tag
            if (isOneHand)
                weaponTags.AddDistinct(tagOneHandWeaponCE);
            else
                weaponTags.Remove(tagOneHandWeaponCE);

            // Update stat
            //var stat = thing.statBases.FirstOrDefault(x => x.stat == CE_StatDefOf.OneHandedness);
            //if (stat == null && isOneHand)
            //{
            //    thing.statBases.Add(new StatModifier
            //    {
            //        stat = CE_StatDefOf.OneHandedness,
            //        value = 1f
            //    });
            //}
            //else if (stat != null)
            //{
            //    stat.value = isOneHand ? 1f : 0f;
            //}
        }

        //combat role
        public static ReadOnlyCollection<string> combatRoleTags = new List<string>()
        {
            "CE_AI_AssaultWeapon",
            "CE_AI_Pistol",
            "CE_AI_Rifle",
            "CE_AI_Suppressive",
            "CE_AI_Grenade",
            "CE_AI_Nonlethal",
            "CE_AI_Launcher"
        }.AsReadOnly();
        public static string GetCombatRole(List<string> weaponTags)
        {
            if (weaponTags == null)
            {
                return null;
            }

            string combatRole = weaponTags.FirstOrDefault(x => combatRoleTags.Contains(x));
            if (combatRole == null)
            {
                return null;
            }
            return combatRole;
        }

        public static void SetCombatRole(List<string> weaponTags, string combatRole)
        {
            if (weaponTags == null)
            {
                return;
            }
            // Remove all combat role tags
            weaponTags.RemoveAll(x => combatRoleTags.Contains(x));
            // Add the new combat role tag
            if (combatRole != null)
            {
                weaponTags.Add(combatRole);
            }
        }

    }
}
