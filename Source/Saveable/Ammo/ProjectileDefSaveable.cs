﻿using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using System.Reflection;

using CeManualPatcher.Saveable.Ammo;
using System.Collections.ObjectModel;
using CeManualPatcher.Misc;

namespace CeManualPatcher.Saveable
{

    internal class ProjectileDefSaveable : SaveableBase
    {
        //字段
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                "armorPenetrationSharp",
                "armorPenetrationBlunt",
                "explosionRadius",
                "suppressionFactor",
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private string damageDefString;
        private DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value?.defName ?? "null";
        }
        private GasType? postExplosionGasType;

        private int damageAmountBase;

        private FieldInfo fieldInfo_damageAmountBase => typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance);

        private SecondaryExplosionSaveable secondaryExplosion;

        private SecondaryDamageSaveable secondaryDamage;

        private ThingDefCountClassSaveable fragments;

        private string label;

        //private
        private ProjectilePropertiesCE projectile
        {
            get
            {
                if (thingDef == null || thingDef.projectile == null)
                {
                    return null;
                }
                return thingDef.projectile as ProjectilePropertiesCE;
            }
        }

        //original
        private ProjectilePropertiesCE originalData;
        private string originalLabel;

        public ProjectileDefSaveable() { }
        public ProjectileDefSaveable(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (projectile == null)
            {
                return;
            }

            InitOriginalData();

            this.secondaryDamage = new SecondaryDamageSaveable(thingDef);

            if (thingDef.HasComp<CompExplosiveCE>())
            {
                this.secondaryExplosion = new SecondaryExplosionSaveable(thingDef);
            }

            if(thingDef.HasComp<CompFragments>())
            {
                this.fragments = new ThingDefCountClassSaveable(thingDef);
            }
        }

        protected override void Apply()
        {
            if (projectile == null)
            {
                return;
            }

            foreach (var item in propNames)
            {
                PropUtility.SetPropValueString(projectile, item, this.propDic[item]);
            }

            projectile.damageDef = damageDef;
            projectile.postExplosionGasType = postExplosionGasType;
            fieldInfo_damageAmountBase.SetValue(projectile, damageAmountBase);

            thingDef.label = this.label;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && projectile != null)
            {
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(projectile, item).ToString();
                }

                this.damageDef = projectile.damageDef;
                this.postExplosionGasType = projectile.postExplosionGasType;
                this.damageAmountBase = (int)fieldInfo_damageAmountBase.GetValue(projectile);

                this.label = thingDef.label;
            }

            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (propDic == null)
                    propDic = new Dictionary<string, string>();
            }
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref damageDefString, "damageDef");
            Scribe_Values.Look(ref postExplosionGasType, "postExplosionGasType");
            Scribe_Values.Look(ref damageAmountBase, "damageAmountBase");

            Scribe_Deep.Look(ref secondaryDamage, "secondaryDamage");
            Scribe_Deep.Look(ref fragments, "fragments");
            Scribe_Deep.Look(ref secondaryExplosion, "secondaryExplosion");
        }

        public override void Reset()
        {
            if (projectile == null)
            {
                return;
            }

            PropUtility.CopyPropValue(originalData, projectile);

            thingDef.label = originalLabel;

            //damageAmountBase
            fieldInfo_damageAmountBase.SetValue(projectile, fieldInfo_damageAmountBase.GetValue(originalData));

            this.secondaryExplosion?.Reset();
            this.secondaryDamage?.Reset();
            this.fragments?.Reset();
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (projectile == null)
            {
                return;
            }

            InitOriginalData();

            secondaryExplosion?.PostLoadInit(thingDef);
            secondaryDamage?.PostLoadInit(thingDef);
            fragments?.PostLoadInit(thingDef);

            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (projectile == null)
            {
                return;
            }
            originalData = new ProjectilePropertiesCE();
            PropUtility.CopyPropValue(projectile, originalData);

            this.originalLabel = thingDef.label;

            //damageAmountBase
            fieldInfo_damageAmountBase.SetValue(originalData, fieldInfo_damageAmountBase.GetValue(projectile));
        }
    }
}
