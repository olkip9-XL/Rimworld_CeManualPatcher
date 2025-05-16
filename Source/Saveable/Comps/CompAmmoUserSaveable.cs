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
    internal class CompAmmoUserSaveable : SaveableBase<ThingDef>
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
        public AmmoSetDef ammoSet
        {
            get => DefDatabase<AmmoSetDef>.GetNamed(ammoSetString, false);
            set => ammoSetString = value?.defName ?? "null";
        }

        //private
        private CompProperties_AmmoUser compProps
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

        private CompProperties_AmmoUser originalData;

        public CompProperties_AmmoUser reservedData;

        public CompAmmoUserSaveable() { }

        public CompAmmoUserSaveable(ThingDef thingDef, bool forceAdd = false)
        {
            this.def = thingDef;
            if (compProps == null && !forceAdd)
            {
                return;
            }

            InitOriginalData();
        }

        protected override void Apply()
        {
            if (compProps == null)
            {
                return;
            }

            foreach (var item in propNames)
            {
                PropUtility.SetPropValueString(compProps, item, this.propDic[item]);
            }

            compProps.ammoSet = ammoSet;

            if (ammoSet == null)
            {
                def.comps.RemoveWhere(x => x is CompProperties_AmmoUser);
            }

        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && (compProps != null || reservedData != null))
            {
                if (reservedData == null)
                    reservedData = compProps;

                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(reservedData, item).ToString();
                }

                //this.ammoSet = compProps.ammoSet;
                if (compProps != null && reservedData.ammoSet == null)
                {
                    def.comps.RemoveWhere(x => x is CompProperties_AmmoUser);
                }
                else if (compProps == null && reservedData.ammoSet != null)
                {
                    def.comps.Add(reservedData);
                }

                this.ammoSet = reservedData.ammoSet;
            }

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

        public override void Reset()
        {
            if (compProps == null && reservedData != null)
            {
                def.comps.Add(reservedData);
            }

            if (compProps == null)
            {
                return;
            }

            if (originalData == null)
            {
                def.comps.RemoveWhere(x => x is CompProperties_AmmoUser);
            }
            else
            {
                PropUtility.CopyPropValue(originalData, compProps);
            }
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            InitOriginalData();

            if (!thingDef.HasComp<CompAmmoUser>())
            {
                thingDef.comps.Add(new CompProperties_AmmoUser());
            }
            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (compProps == null)
            {
                return;
            }
            originalData = new CompProperties_AmmoUser();
            PropUtility.CopyPropValue(compProps, originalData);

            reservedData = compProps;
        }

    }
}
