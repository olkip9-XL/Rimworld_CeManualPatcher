using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class CompArmorDurabilitySaveable : CompSaveableBase<CompProperties_ArmorDurability>
    {
        protected override CompProperties_ArmorDurability compProps
        {
            get
            {
                if (def == null || !def.HasComp<CompArmorDurability>())
                {
                    return null;
                }
                return def.GetCompProperties<CompProperties_ArmorDurability>();
            }
        }

        public override bool CompChanged
        {
            get
            {
                if (originalData == null || compProps == null)
                {
                    return false;
                }

                foreach (var fieldName in propNames)
                {
                    string originalValue = PropUtility.GetPropValue(originalData, fieldName).ToString();
                    string currentValue = PropUtility.GetPropValue(compProps, fieldName).ToString();

                    if (originalValue != currentValue)
                    {
                        return true;
                    }
                }

                if (!originalData.RepairIngredients.NullOrEmpty() && !compProps.RepairIngredients.NullOrEmpty() && originalData.RepairIngredients.Count == compProps.RepairIngredients.Count)
                {
                    List<ThingDefCountClass> originalList = originalData.RepairIngredients;
                    List<ThingDefCountClass> currentList = compProps.RepairIngredients;

                    originalList.SortBy(x => x.thingDef.defName);
                    currentList.SortBy(x => x.thingDef.defName);

                    for (int i = 0; i < originalList.Count; i++)
                    {
                        if (originalList[i].thingDef != currentList[i].thingDef ||
                            originalList[i].count != currentList[i].count)
                            return true;
                    }
                }

                return false;
            }
        }

        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
               "Durability",
                "Regenerates",
                "RegenInterval",
                "RegenValue",
                "Repairable",
                "RepairTime",
                "RepairValue",
                "MinArmorValueSharp",
                "MinArmorValueBlunt",
                "MinArmorValueHeat",
                "MinArmorValueElectric",
                "MinArmorPct",
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private List<ThingDefCountClassExpo> repairIngredients = new List<ThingDefCountClassExpo>();

        public CompArmorDurabilitySaveable() { }
        public CompArmorDurabilitySaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if(Scribe.mode == LoadSaveMode.LoadingVars && base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "compArmorDurability", LookMode.Value, LookMode.Value);
                if (!propDic.NullOrEmpty())
                {
                    base.compIsNull = false;
                }
            }

            if (!base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "compArmorDurability", LookMode.Value, LookMode.Value);
                Scribe_Collections.Look(ref repairIngredients, "repairIngredients", LookMode.Deep);

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (propDic == null)
                    {
                        propDic = new Dictionary<string, string>();
                    }
                }
            }
        }

        protected override CompProperties_ArmorDurability MakeCopy(CompProperties_ArmorDurability original)
        {
            if (original == null)
            {
                return null;
            }

            CompProperties_ArmorDurability copy = new CompProperties_ArmorDurability();
            PropUtility.CopyPropValue(original, copy);

            if (original.RepairIngredients != null)
            {
                copy.RepairIngredients = new List<ThingDefCountClass>();
                foreach (var ingredient in original.RepairIngredients)
                {
                    ThingDefCountClass temp = new ThingDefCountClass
                    {
                        thingDef = ingredient.thingDef,
                        count = ingredient.count
                    };
                    copy.RepairIngredients.Add(temp);
                }
            }

            return copy;
        }

        protected override CompProperties_ArmorDurability ReadData()
        {
            CompProperties_ArmorDurability newComp = new CompProperties_ArmorDurability();
            foreach (var item in propNames)
            {
                PropUtility.SetPropValueString(newComp, item, this.propDic[item]);
            }

            if (!repairIngredients.NullOrEmpty())
            {
                newComp.RepairIngredients = new List<ThingDefCountClass>();
                foreach (var item in repairIngredients)
                {
                    newComp.RepairIngredients.Add(new ThingDefCountClass
                    {
                        thingDef = DefDatabase<ThingDef>.GetNamed(item.defName, false),
                        count = item.count
                    });
                }
            }

            return newComp;
        }

        protected override void SaveData()
        {
            foreach (var item in propNames)
            {
                propDic[item] = PropUtility.GetPropValue(compProps, item).ToString();
            }

            repairIngredients = new List<ThingDefCountClassExpo>();
            if (compProps.RepairIngredients != null)
            {
                foreach (var ingredient in compProps.RepairIngredients)
                {
                    repairIngredients.Add(new ThingDefCountClassExpo
                    {
                        defName = ingredient.thingDef.defName,
                        count = ingredient.count
                    });
                }
            }
        }
    }
}
