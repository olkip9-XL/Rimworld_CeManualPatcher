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

        public static ReadOnlyCollection<string> NoneCommonPropNames = new List<string>()
        {
                "armorPenetrationSharp",
                "armorPenetrationBlunt",
                "explosionRadius",
                "preExplosionSpawnChance",
                "preExplosionSpawnThingCount",
                "postExplosionSpawnChance",
                "postExplosionSpawnThingCount",

                "preExplosionSpawnThingDef",
                "postExplosionSpawnThingDef",

                "postExplosionGasType",
                "damageDef",
                "damageAmountBase"
        }.AsReadOnly();


        //字段
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                //vanilla
                "speed",
                "flyOverhead",
                "alwaysFreeIntercept",
                "arcHeightFactor",
                "shadowSize",
                "spinRate",
                "explosionDelay",
                "ai_IsIncendiary",

                "armorPenetrationSharp",
                "armorPenetrationBlunt",
                "explosionRadius",
                "suppressionFactor",
                "stoppingPower",

                "speedGain",
                "fuelTicks",
                "pelletCount",
                "spreadMult",
                "damageAdjacentTiles",
                "dropsCasings",
                "gravityFactor",
                "isInstant",
                "damageFalloff",
                "castShadow",
                "airborneSuppressionFactor",
                "dangerFactor",
                "detonateEffectsScaleOverride",

                "fuze_delay",
                "HP_penetration",
                "HP_penetration_ratio",

                "armingDelay",
                "aimHeightOffset",
                "empShieldBreakChance",
                "collideDistance",
                "impactChance",

                //1.6 add
                "recoilMultiplier",
                "recoilOffset",
                "warmupMultiplier",
                "warmupOffset",
                "effectiveRangeMultiplier",
                "effectiveRangeOffset",
                "muzzleFlashMultiplier",
                "muzzleFlashOffset",

                "preExplosionSpawnChance",
                "preExplosionSpawnThingCount",
                "postExplosionSpawnChance",
                "postExplosionSpawnThingCount",

                //testDef
                "preExplosionSpawnThingDef",
                "postExplosionSpawnThingDef",

                "postExplosionGasType",
                "damageDef",
                "damageAmountBase"

        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

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

            def.label = this.label;
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && projectile != null)
            {
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValueString(projectile, item);
                }

                this.label = def.label;
            }

            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (propDic == null)
                    propDic = new Dictionary<string, string>();

                //old save
                string valueString = null;
                Scribe_Values.Look(ref valueString, "damageDef");
                if (!string.IsNullOrEmpty(valueString))
                {
                    propDic["damageDef"] = valueString;
                }

                valueString = null;
                Scribe_Values.Look(ref valueString, "postExplosionGasType");
                if (!string.IsNullOrEmpty(valueString))
                {
                    propDic["postExplosionGasType"] = valueString;
                }

                valueString = null;
                Scribe_Values.Look(ref valueString, "damageAmountBase");
                if (!string.IsNullOrEmpty(valueString))
                {
                    propDic["damageAmountBase"] = valueString;
                }
            }
            Scribe_Values.Look(ref label, "label");

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
        }
    }
}
