using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    internal static class CopyUtility
    {

        public static List<StatModifier> CopyStats(ThingDef thing)
        {
            if (thing == null || thing.statBases == null)
            {
                return null;
            }

            List<StatModifier> statModifiers = new List<StatModifier>();
            foreach (var item in thing.statBases)
            {
                StatModifier statModifier = new StatModifier();
                PropUtility.CopyPropValue(item, statModifier);
                statModifiers.Add(statModifier);
            }

            return statModifiers;
        }

        public static VerbProperties CopyVerb(ThingDef thing, bool convertToCE = false)
        {
            if (thing == null || thing.Verbs == null || thing.Verbs.Count == 0)
            {
                return null;
            }

            VerbProperties verb = new VerbProperties();

            if(thing.Verbs[0] is VerbPropertiesCE verbPropertiesCE)
            {
                verb = new VerbPropertiesCE();
                PropUtility.CopyPropValue(verbPropertiesCE, verb);
            }
            else
            {
                if (convertToCE)
                {
                    verb = new VerbPropertiesCE();
                    verb = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(thing.Verbs[0]);
                }
                else
                {
                    PropUtility.CopyPropValue(thing.Verbs[0], verb);
                }
            }

            return verb;
        }

        public static List<Tool> CopyTools(ThingDef thing, bool convertToCE = false)
        {
            if (thing == null || thing.tools == null)
            {
                return null;
            }
            List<Tool> tools = new List<Tool>();
            foreach (var item in thing.tools)
            {
                Tool tool = new Tool();
                if (item is ToolCE)
                {
                    tool = new ToolCE();
                    PropUtility.CopyPropValue(item, tool);
                }
                else
                {
                    if (convertToCE)
                    {
                        tool = new ToolCE();
                        tool = PropUtility.ConvertToChild<Tool, ToolCE>(item);
                    }
                    else
                    {
                        PropUtility.CopyPropValue(item, tool);
                    }
                }

                //capability
                if(item.capacities != null)
                {
                    tool.capacities = new List<ToolCapacityDef>();
                    tool.capacities.AddRange(item.capacities);
                }

                //surpriseAttack
                if(item.surpriseAttack != null)
                {
                    tool.surpriseAttack = new SurpriseAttackProps()
                    {
                        extraMeleeDamages = new List<ExtraDamage>(),
                    };

                    foreach(var extraDamage in item.surpriseAttack.extraMeleeDamages)
                    {
                        ExtraDamage extraDamageCopy = new ExtraDamage();
                        PropUtility.CopyPropValue(extraDamage, extraDamageCopy);
                        tool.surpriseAttack.extraMeleeDamages.Add(extraDamageCopy);
                    }
                }

                //extra melee damage
                if (item.extraMeleeDamages != null)
                {
                    tool.extraMeleeDamages = new List<ExtraDamage>();
                    foreach (var extraDamage in item.extraMeleeDamages)
                    {
                        ExtraDamage extraDamageCopy = new ExtraDamage();
                        PropUtility.CopyPropValue(extraDamage, extraDamageCopy);
                        tool.extraMeleeDamages.Add(extraDamageCopy);
                    }
                }

                tools.Add(tool);
            }
            return tools;

        }
        public static void CopyComp<T>(T copyProps, T targetProps) where T : CompProperties
        {
            if (copyProps == null || targetProps == null)
            {
                return;
            }
            PropUtility.CopyPropValue(copyProps, targetProps);
        }

        public static void CopyModExtension<T>(T copyProps, T targetProps) where T : DefModExtension
        {
            if (copyProps == null || targetProps == null)
            {
                return;
            }

            if(copyProps is PartialArmorExt)
            {
                CopyModExtension_PartialArmor(copyProps as PartialArmorExt, targetProps as PartialArmorExt);
                return;
            }

            PropUtility.CopyPropValue(copyProps, targetProps);
        }

        private static void CopyModExtension_PartialArmor(PartialArmorExt copyProps, PartialArmorExt targetProps)
        {
            if (copyProps == null || targetProps == null)
            {
                return;
            }

            targetProps.stats = new List<ApparelPartialStat>();

            foreach(var item in copyProps.stats)
            {
                ApparelPartialStat apparelPartialStat = new ApparelPartialStat();
                PropUtility.CopyPropValue(item, apparelPartialStat);

                List<BodyPartDef> bodyPartDefs = new List<BodyPartDef>(item.parts);
                apparelPartialStat.parts = bodyPartDefs;

                targetProps.stats.Add(apparelPartialStat);
            }

        }

    }
}
