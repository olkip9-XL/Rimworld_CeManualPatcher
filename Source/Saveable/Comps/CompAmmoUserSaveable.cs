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
    internal class CompAmmoUserSaveable : SaveableBase
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
                if (thingDef == null || !thingDef.HasComp<CompAmmoUser>())
                {
                    return null;
                }
                return thingDef.GetCompProperties<CompProperties_AmmoUser>();
            }
        }

        private CompProperties_AmmoUser originalData;

        public CompAmmoUserSaveable() { }

        public CompAmmoUserSaveable(ThingDef thingDef, bool forceAdd = false)
        {
            this.thingDef = thingDef;
            if(compProps == null && !forceAdd)
            {
                return;
            }

            InitOriginalData();
        }

        protected override void Apply()
        {
            if(compProps == null)
            {
                return;
            }

            foreach (var item in propNames)
            {
               PropUtility.SetPropValueString(compProps, item, this.propDic[item]);
            }

            compProps.ammoSet = ammoSet;
        }
        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving && compProps != null)
            {
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(compProps, item).ToString();
                }

                this.ammoSet = compProps.ammoSet;
            }

            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref ammoSetString, "ammoSet", null);

            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (propDic == null)
                {
                    propDic = new Dictionary<string, string>();
                }
            }
        }

        public override void Reset()
        {
            if(compProps == null)
            {
                return;
            }

            if(originalData == null)
            {
                thingDef.comps.RemoveWhere(x => x is CompProperties_AmmoUser);
            }
            else
            {
                PropUtility.CopyPropValue(originalData, compProps);
            }
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.thingDef = thingDef;
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
        }

    }
}
