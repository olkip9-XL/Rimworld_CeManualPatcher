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
    internal class CompFireModesSaveable : SaveableBase<ThingDef>
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


        //private
        private CompProperties_FireModes compProps
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

        private CompProperties_FireModes originalData;
        public CompFireModesSaveable() { }

        public CompFireModesSaveable(ThingDef thingDef, bool forceAdd = false)
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

            compProps.aiAimMode = aiAimMode;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && compProps!=null)
            {
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(compProps, item).ToString();
                }
                this.aiAimMode = compProps.aiAimMode;
            }
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

        public override void Reset()
        {
            if (compProps == null)
            {
                return;
            }

            if(originalData == null)
            {
                def.comps.RemoveWhere(x => x is CompProperties_FireModes);
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

            if(!thingDef.HasComp<CompFireModes>())
            {
                thingDef.comps.Add(new CompProperties_FireModes());
            }

            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (compProps == null)
            {
                return;
            }
            originalData = new CompProperties_FireModes();
            PropUtility.CopyPropValue(compProps, originalData);
        }
    }
}
