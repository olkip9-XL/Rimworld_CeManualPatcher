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
    internal class CompAmmoUserSaveable : CompSaveableBase<CompProperties_AmmoUser>
    {
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                "magazineSize",
                "AmmoGenPerMagOverride",
                "reloadTime",
                "reloadOneAtATime",
                "throwMote",
                "loadedAmmoBulkFactor"

        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private string ammoSetString;

        protected override CompProperties_AmmoUser compProps
        {
            get
            {
                if (def == null || !def.HasComp<CompAmmoUser>())
                {
                    return null;
                }
                return def.GetCompProperties<CompProperties_AmmoUser>();
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

                if (originalData.ammoSet != compProps.ammoSet)
                {
                    return true;
                }

                return false;
            }
        }

        public CompAmmoUserSaveable() { }

        public CompAmmoUserSaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //old save
            if(Scribe.mode == LoadSaveMode.LoadingVars && base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
                if (!propDic.NullOrEmpty())
                {
                    base.compIsNull = false;
                }
            }

            if (!base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
                Scribe_Values.Look(ref ammoSetString, "ammoSet", null);

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    if (propDic == null)
                    {
                        propDic = new Dictionary<string, string>();
                    }
                }
            }
        }
        protected override void SaveData()
        {
            foreach (var item in propNames)
            {
                propDic[item] = PropUtility.GetPropValue(compProps, item).ToString();
            }

            ammoSetString = compProps.ammoSet?.defName ?? "null";
        }

        protected override CompProperties_AmmoUser ReadData()
        {
            CompProperties_AmmoUser newComp = new CompProperties_AmmoUser();
            foreach (var item in propNames)
            {
                if (propDic.ContainsKey(item))
                {
                    PropUtility.SetPropValueString(newComp, item, propDic[item]);
                }
            }

            if (!ammoSetString.NullOrEmpty())
            {
                newComp.ammoSet = DefDatabase<AmmoSetDef>.GetNamed(ammoSetString, false);
            }

            if(newComp.ammoSet == null)
            {
                return null;
            }
            else
            {
                return newComp;
            }
        }

        protected override CompProperties_AmmoUser MakeCopy(CompProperties_AmmoUser original)
        {
            if(original == null)
            {
                return null;
            }

            CompProperties_AmmoUser copy = new CompProperties_AmmoUser();
            PropUtility.CopyPropValue(original, copy);

            copy.ammoSet = original.ammoSet;

            return copy;
        }
    }
}
