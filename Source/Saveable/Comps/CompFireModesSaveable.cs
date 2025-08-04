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
    internal class CompFireModesSaveable : CompSaveableBase<CompProperties_FireModes>
    {

        //字段
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
               "aimedBurstShotCount",
                "aiUseBurstMode",
                "noSingleShot",
                "noSnapshot",
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        public AimMode aiAimMode = AimMode.AimedShot;


        protected override CompProperties_FireModes compProps
        {
            get
            {
                if (def == null || !def.HasComp<CompFireModes>())
                {
                    return null;
                }
                return def.GetCompProperties<CompProperties_FireModes>();
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

                if (originalData.aiAimMode != compProps.aiAimMode)
                {
                    return true;
                }
                return false;
            }
        }

        public CompFireModesSaveable() { }

        public CompFireModesSaveable(ThingDef thingDef) : base(thingDef)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if(Scribe.mode == LoadSaveMode.LoadingVars && base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "compFireModes", LookMode.Value, LookMode.Value);
                if (!propDic.NullOrEmpty())
                {
                    base.compIsNull = false;
                }
            }


            if (!base.compIsNull)
            {
                Scribe_Collections.Look(ref propDic, "compFireModes", LookMode.Value, LookMode.Value);
                Scribe_Values.Look(ref aiAimMode, "aiAimMode", AimMode.AimedShot);

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
            this.aiAimMode = compProps.aiAimMode;
        }

        protected override CompProperties_FireModes ReadData()
        {
            CompProperties_FireModes newComp = new CompProperties_FireModes();

            foreach (var item in propNames)
            {
                if (propDic.ContainsKey(item))
                {
                    PropUtility.SetPropValueString(newComp, item, this.propDic[item]);
                }
            }

            newComp.aiAimMode = this.aiAimMode;

            return newComp;
        }

        protected override CompProperties_FireModes MakeCopy(CompProperties_FireModes original)
        {
            if (original == null)
            {
                return null;
            }

            CompProperties_FireModes copy = new CompProperties_FireModes();

            PropUtility.CopyPropValue(original, copy);
            copy.aiAimMode = original.aiAimMode;

            return copy;
        }
    }
}
