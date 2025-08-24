using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace CeManualPatcher.Extension
{
    internal static class List_StatModifierExtension
    {
        public static void SetStatValueToList(this List<StatModifier> list, StatDef stat, float value)
        {
            if (list == null)
            {
                return;
            }

            var existingModifier = list.FirstOrDefault(m => m.stat == stat);
            if (existingModifier != null)
            {
                existingModifier.value = value;
            }
            else
            {
                list.Add(new StatModifier { stat = stat, value = value });
            }
        }
    }
}
