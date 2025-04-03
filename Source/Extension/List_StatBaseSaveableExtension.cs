using CeManualPatcher.Saveable;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeManualPatcher.Extension
{
    internal static class List_StatBaseSaveableExtension
    {
        public static void AddStat(this List<Saveable.StatSaveable> statBase, StatDef statDef, float value = 0)
        {
            StatSaveable statSaveable = statBase.Find(x => x.StatDef == statDef);
            if(statSaveable != null)
            {
                if(statSaveable.needDelete)
                {
                    statSaveable.needDelete = false;
                }
                else
                {
                    statSaveable.value = value;
                }
            }
            else
            {
                StatModifier statModifier = new StatModifier();
                statModifier.stat = statDef;
                statModifier.value = value;
                statBase.Add(new StatSaveable(statModifier));
            }
        }

        public static void DeleteStat(this List<Saveable.StatSaveable> statBase, StatDef statDef)
        {
            StatSaveable statSaveable = statBase.Find(x => x.StatDef == statDef);
            if (statSaveable != null)
            {
                statSaveable.needDelete = true;
            }
        }
    }
}
