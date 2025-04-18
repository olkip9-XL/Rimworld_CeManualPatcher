﻿using CeManualPatcher.Extension;
using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    internal static class MP_Options
    {
        private static List<SoundDef> soundCastInt = null;
        public static List<SoundDef> soundCast
        {
            get
            {
                if (soundCastInt == null)
                {
                    soundCastInt = new List<SoundDef>();
                    foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (item.Verbs != null)
                        {
                            foreach (var verb in item.Verbs)
                            {
                                if (verb.soundCast != null)
                                {
                                    soundCastInt.AddDistinct(verb.soundCast);
                                }
                            }
                        }
                    }
                }
                return soundCastInt;
            }
        }


        private static List<SoundDef> soundCastTailInt = null;
        public static List<SoundDef> soundCastTail
        {
            get
            {
                if (soundCastTailInt == null)
                {
                    soundCastTailInt = new List<SoundDef>();
                    foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (item.Verbs != null)
                        {
                            foreach (var verb in item.Verbs)
                            {
                                if (verb.soundCastTail != null)
                                {
                                    soundCastTailInt.AddDistinct(verb.soundCastTail);
                                }
                            }
                        }
                    }
                }
                return soundCastTailInt;
            }
        }


        private static List<StatDef> statDefsInt = null;
        public static List<StatDef> statDefs
        {
            get
            {
                if (statDefsInt == null)
                {
                    statDefsInt = new List<StatDef>();

                    statDefsInt.AddRange(DefDatabase<StatDef>.AllDefs.Where(x =>
                    x.category == StatCategoryDefOf.BasicsNonPawn ||
                    x.category == StatCategoryDefOf.BasicsNonPawnImportant ||
                    x.category == StatCategoryDefOf.Basics ||
                    x.category == StatCategoryDefOf.BasicsImportant ||
                    x.category == StatCategoryDefOf.Weapon ||
                    x.category == StatCategoryDefOf.Weapon_Ranged ||
                    x.category == StatCategoryDefOf.Weapon_Melee ||
                    x.category == StatCategoryDefOf.StuffStatFactors
                    ));

                    statDefsInt.RemoveWhere(x => exceptStatDefs.Contains(x));
                }
                return statDefsInt;
            }
        }

        private static List<StatDef> exceptStatDefsInt = null;
        public static List<StatDef> exceptStatDefs
        {
            get
            {
                if (exceptStatDefsInt == null)
                {
                    exceptStatDefsInt = new List<StatDef>()
                    {
                        StatDefOf.AccuracyTouch,
                        StatDefOf.AccuracyShort,
                        StatDefOf.AccuracyMedium,
                        StatDefOf.AccuracyLong,
                        StatDefOf.ShootingAccuracyFactor_Touch,
                        StatDefOf.ShootingAccuracyFactor_Short,
                        StatDefOf.ShootingAccuracyFactor_Medium,
                        StatDefOf.ShootingAccuracyFactor_Long,
                    };
                }
                return exceptStatDefsInt;
            }
        }

        private static List<BodyPartGroupDef> bodyPartGroupDefsInt = null;
        public static List<BodyPartGroupDef> bodyPartGroupDefs
        {
            get
            {
                if (bodyPartGroupDefsInt == null)
                {
                    bodyPartGroupDefsInt = new List<BodyPartGroupDef>();

                    List<BodyPartGroupDef> groupsWithBody = new List<BodyPartGroupDef>();
                    foreach (var item in DefDatabase<BodyDef>.AllDefs)
                    {
                        groupsWithBody.AddRange(item.AllBodyPartGroup());
                    }
                    groupsWithBody.RemoveDuplicates();

                    bodyPartGroupDefsInt.AddRange(DefDatabase<BodyPartGroupDef>.AllDefs.Except(groupsWithBody));
                }
                return bodyPartGroupDefsInt;
            }
        }


        private static List<ToolCapacityDef> toolCapacityDefsInt = null;
        public static List<ToolCapacityDef> toolCapacityDefs
        {
            get
            {
                if (toolCapacityDefsInt == null)
                {
                    toolCapacityDefsInt = new List<ToolCapacityDef>();
                    toolCapacityDefs.AddRange(DefDatabase<ToolCapacityDef>.AllDefsListForReading);
                }
                return toolCapacityDefsInt;
            }
        }


        private static List<MP_AmmoCategory> ammoCategoriesInt = null;
        public static List<MP_AmmoCategory> ammoCategories
        {
            get
            {
                if (ammoCategoriesInt == null)
                {
                    ammoCategoriesInt = new List<MP_AmmoCategory>();

                    foreach (var item in DefDatabase<ThingCategoryDef>.AllDefs.Where(x => x.parent == MP_ThingCategoryDefOf.Ammo))
                    {
                        ammoCategoriesInt.Add(new MP_AmmoCategory(item));
                    }

                    ammoCategoriesInt.Add(new MP_AmmoCategory("Grenades"));
                    ammoCategoriesInt.Add(new MP_AmmoCategory("Uncategorized"));
                }
                return ammoCategoriesInt;
            }
        }


        private static List<ThingDef> fragmentsInt;
        public static ReadOnlyCollection<ThingDef> fragments
        {
            get
            {
                if (fragmentsInt == null)
                {
                    fragmentsInt = new List<ThingDef>()
                    {
                        DefDatabase<ThingDef>.GetNamed("Fragment_Small"),
                        DefDatabase<ThingDef>.GetNamed("Fragment_Medium"),
                        DefDatabase<ThingDef>.GetNamed("Fragment_Large"),
                        DefDatabase<ThingDef>.GetNamed("Fragment_Bomblet")
                    };
                }
                return fragmentsInt.AsReadOnly();
            }
        }


        private static List<DamageDef> allDamageDefsInt;
        private static List<DamageDef> allDamageDefsEXInt;
        public static ReadOnlyCollection<DamageDef> allDamageDefs
        {
            get
            {
                if (allDamageDefsInt == null)
                {
                    allDamageDefsInt = new List<DamageDef>();
                    allDamageDefsInt.AddRange(DefDatabase<DamageDef>.AllDefsListForReading);
                }
                return allDamageDefsInt.AsReadOnly();
            }
        }
        public static ReadOnlyCollection<DamageDef> allDamageDefsEX
        {
            get
            {
                if (allDamageDefsEXInt == null)
                {
                    allDamageDefsEXInt = new List<DamageDef>();
                    allDamageDefsEXInt.AddRange(allDamageDefs.Where(x => x.soundExplosion != null));
                }
                return allDamageDefsEXInt.AsReadOnly();
            }
        }



        private static List<Type> VerbClassesInt = null;
        public static List<Type> VerbClasses
        {
            get
            {
                if (VerbClassesInt == null)
                {
                    VerbClassesInt = new List<Type>()
                    {
                        typeof(Verb_Shoot),
                        typeof(Verb_ShootOneUse),
                        typeof(Verb_LaunchProjectile),
                        typeof(CombatExtended.Verb_ShootCE),
                        typeof(CombatExtended.Verb_ShootCEOneUse),
                        typeof(CombatExtended.Verb_LaunchProjectileCE),
                        typeof(CombatExtended.Verb_ShootCEOneUseStatic),
                        typeof(CombatExtended.Verb_ShootMortarCE),
                        typeof(CombatExtended.Verb_ShootFlareCE),
                        typeof(CombatExtended.Verb_ShootMortarCE),
                    };
                }
                return VerbClassesInt;
            }
        }

        private static List<string> bipodIdInt = null;
        public static List<string> BipodId
        {
            get
            {
                if (bipodIdInt == null)
                {
                    bipodIdInt = new List<string>();
                    foreach (var item in DefDatabase<BipodCategoryDef>.AllDefs)
                    {
                        bipodIdInt.Add(item.bipod_id);
                    }
                }
                return bipodIdInt;
            }
        }
        public static List<BipodCategoryDef> bipodCategoryDefs => DefDatabase<BipodCategoryDef>.AllDefsListForReading;
            

        private static ReadOnlyCollection<StatDef> statDefs_ApparelInt = null;

        public static ReadOnlyCollection<StatDef> statDefs_Apparel
        {
            get
            {
                if (statDefs_ApparelInt == null)
                {
                    List<StatCategoryDef> categoryDefs = new List<StatCategoryDef>();

                    foreach(var item in DefDatabase<ThingDef>.AllDefs.Where(x=> x.IsApparel))
                    {
                        if (item.statBases != null)
                        {
                            foreach (var stat in item.statBases)
                            {
                                if (!categoryDefs.Contains(stat.stat.category))
                                {
                                    categoryDefs.Add(stat.stat.category);
                                }
                            }
                        }
                    }

                    categoryDefs.Add(StatCategoryDefOf.StuffStatFactors);

                    List<StatDef> list = new List<StatDef>();
                    foreach (var item in DefDatabase<StatDef>.AllDefsListForReading)
                    {
                        if (categoryDefs.Contains(item.category))
                        {
                            list.Add(item);
                        }
                    }
                    statDefs_ApparelInt = list.AsReadOnly();
                }
                return statDefs_ApparelInt;
            }
        }


        private static ReadOnlyCollection<StatDef> statDefs_WeaponInt = null;
        public static ReadOnlyCollection<StatDef> statDefs_Weapon
        {
            get
            {
                if (statDefs_WeaponInt == null)
                {
                    List<StatCategoryDef> categoryDefs = new List<StatCategoryDef>();
                    foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(x => x.IsWeapon))
                    {
                        if (item.statBases != null)
                        {
                            foreach (var stat in item.statBases)
                            {
                                if (!categoryDefs.Contains(stat.stat.category))
                                {
                                    categoryDefs.Add(stat.stat.category);
                                }
                            }
                        }
                    }
                    List<StatDef> list = new List<StatDef>();
                    foreach (var item in DefDatabase<StatDef>.AllDefsListForReading)
                    {
                        if (categoryDefs.Contains(item.category))
                        {
                            list.Add(item);
                        }
                    }

                    list.Remove(StatDefOf.AccuracyLong);
                    list.Remove(StatDefOf.AccuracyMedium);
                    list.Remove(StatDefOf.AccuracyShort);
                    list.Remove(StatDefOf.AccuracyTouch);
                    list.Remove(StatDefOf.ShootingAccuracyFactor_Long);
                    list.Remove(StatDefOf.ShootingAccuracyFactor_Medium);
                    list.Remove(StatDefOf.ShootingAccuracyFactor_Short);
                    list.Remove(StatDefOf.ShootingAccuracyFactor_Touch);

                    statDefs_WeaponInt = list.AsReadOnly();
                }
                return statDefs_WeaponInt;
            }
        }

    }
}
