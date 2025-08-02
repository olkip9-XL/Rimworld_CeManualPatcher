using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using System.Reflection;

using System.Collections.ObjectModel;
using CeManualPatcher.Misc;

namespace CeManualPatcher.Saveable
{

    internal class ProjectileDefSaveable : SaveableBase<ThingDef>
    {
        //字段
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                "armorPenetrationSharp",
                "armorPenetrationBlunt",
                "explosionRadius",
                "suppressionFactor",
                "stoppingPower"
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
                if (def == null || def.projectile == null)
                {
                    return null;
                }
                return def.projectile as ProjectilePropertiesCE;
            }
        }

        //original
        private ProjectilePropertiesCE originalData;
        private string originalLabel;

        public string OriginalLabel => originalLabel;
        public ProjectilePropertiesCE OriginalData => originalData;

        public bool SecondaryExplosionChanged => !(secondaryExplosion?.CompareOriginalData() ?? true);
        public bool SecondaryDamageChanged => !(secondaryDamage?.CompareOriginalData() ?? true);
        public bool FragmentsChanged => !(fragments?.CompareOriginalData() ?? true);

        public ProjectileDefSaveable() { }
        public ProjectileDefSaveable(ThingDef thingDef)
        {
            this.def = thingDef;
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

            if (thingDef.HasComp<CompFragments>())
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
                if (propDic.ContainsKey(item))
                {
                    PropUtility.SetPropValueString(projectile, item, this.propDic[item]);
                }
            }

            projectile.damageDef = damageDef;
            projectile.postExplosionGasType = postExplosionGasType;
            fieldInfo_damageAmountBase.SetValue(projectile, damageAmountBase);

            def.label = this.label;
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

                this.label = def.label;
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

            def.label = originalLabel;

            //damageAmountBase
            fieldInfo_damageAmountBase.SetValue(projectile, fieldInfo_damageAmountBase.GetValue(originalData));

            this.secondaryExplosion?.Reset();
            this.secondaryDamage?.Reset();
            this.fragments?.Reset();
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            //if (projectile == null)
            //{
            //    return;
            //}
            if (def == null || def.projectile == null)
            {
                return;
            }

            if (!(def.projectile is ProjectilePropertiesCE))
            {
                def.projectile = PropUtility.ConvertToChild<ProjectileProperties, ProjectilePropertiesCE>(def.projectile);
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

            this.originalLabel = def.label;

            //damageAmountBase
            fieldInfo_damageAmountBase.SetValue(originalData, fieldInfo_damageAmountBase.GetValue(projectile));
        }
    }
}
