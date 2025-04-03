using CeManualPatcher.Extension;
using CeManualPatcher.Misc;
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
                }
                return statDefsInt;
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
                    allDamageDefsEXInt.AddRange(allDamageDefs.Where(x=> x.soundExplosion != null));
                }
                return allDamageDefsEXInt.AsReadOnly();
            }
        }

    }
}
