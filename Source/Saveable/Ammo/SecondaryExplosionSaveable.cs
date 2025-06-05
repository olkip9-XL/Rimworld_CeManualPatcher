using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    internal class SecondaryExplosionSaveable : SaveableBase<ThingDef>
    {
        //字段
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
            "explosiveRadius",
            "damageAmountBase",
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private string damageDefString;
        private DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value?.defName ?? "null";
        }

        private GasType? postExplosionGasType;

        //private
        private CompProperties_ExplosiveCE compProps
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                if (!def.HasComp<CompExplosiveCE>())
                {
                    return null;
                }
                return def.GetCompProperties<CompProperties_ExplosiveCE>();
            }
        }

        private CompProperties_ExplosiveCE originalData;

        public SecondaryExplosionSaveable() { }
        public SecondaryExplosionSaveable(ThingDef thingDef)
        {
            this.def = thingDef;
            if (compProps == null)
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
            compProps.postExplosionGasType = postExplosionGasType;
            compProps.explosiveDamageType = damageDef;

            foreach (var item in propNames)
            {
                PropUtility.SetPropValueString(compProps, item, this.propDic[item]);
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && compProps != null)
            {
                //保存
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(compProps, item).ToString();
                }

                this.damageDef = compProps.explosiveDamageType;
                this.postExplosionGasType = compProps.postExplosionGasType;
            }

            Scribe_Values.Look(ref damageDefString, "damageDef");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");
            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars && propDic == null)
            {
                propDic = new Dictionary<string, string>();
            }
        }

        public override void Reset()
        {
            if (compProps == null)
            {
                return;
            }
            //重置
            PropUtility.CopyPropValue(originalData, compProps);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            //if (compProps == null)
            //{
            //    return;
            //}

            if (def == null)
            {
                return;
            }

            if (!def.HasComp<CompExplosiveCE>())
            {
                def.comps.Add(new CompProperties_ExplosiveCE());
            }

            InitOriginalData();
            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (compProps == null)
            {
                return;
            }
            originalData = new CompProperties_ExplosiveCE();
            PropUtility.CopyPropValue(compProps, originalData);
        }

        public bool CompareOriginalData()
        {
            CompProperties_ExplosiveCE currentData = compProps;

            foreach (var item in propNames)
            {
                object originalValue = PropUtility.GetPropValue(originalData, item);
                object currentValue = PropUtility.GetPropValue(currentData, item);

                if (!object.Equals(originalValue, currentValue))
                {
                    return false;
                }
            }

            if (compProps.explosiveDamageType != originalData.explosiveDamageType)
            {
                return false;
            }

            if (compProps.postExplosionGasType != originalData.postExplosionGasType)
            {
                return false;
            }

            return true;
        }
    }
}
