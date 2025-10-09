using CombatExtended;
using RimWorld;
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
    internal class VerbPropertiesCESaveable : SaveableBase<ThingDef>
    {
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                "ammoConsumedPerShotCount",
                "recoilAmount",
                "indirectFirePenalty",
                "circularError",
                "ticksToTruePosition",
                "ejectsCasings",
                "ignorePartialLoSBlocker",
                "interruptibleBurst",
                "hasStandardCommand",
                "warmupTime",
                "range",
                "burstShotCount",
                "ticksBetweenBurstShots",
                "muzzleFlashScale",
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        public RecoilPattern recoilPattern = RecoilPattern.None;

        private string defaultProjectileString;
        private ThingDef defaultProjectile
        {
            get => DefDatabase<ThingDef>.GetNamed(defaultProjectileString, false);
            set => defaultProjectileString = value?.defName ?? "null";
        }

        private string soundCastString = "";
        private SoundDef soundCast
        {
            get => DefDatabase<SoundDef>.GetNamed(soundCastString, false);
            set => soundCastString = value?.defName ?? "null";
        }

        private string soundCastTailString = "";
        private SoundDef soundCastTail
        {
            get => DefDatabase<SoundDef>.GetNamed(soundCastTailString, false);
            set => soundCastTailString = value?.defName ?? "null";
        }

        private Type verbClass;

        //私有
        private VerbPropertiesCE verbPropertiesCE
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                if (def.Verbs.NullOrEmpty())
                {
                    return null;
                }
                return def.Verbs[0] as VerbPropertiesCE;
            }
        }

        private VerbProperties originalData;

        public VerbProperties OriginalData => originalData;

        public bool NeedCEPatch => !(originalData is VerbPropertiesCE);
        public Type OriginalVerbClass => originalData?.verbClass;

        internal VerbPropertiesCESaveable() { }
        internal VerbPropertiesCESaveable(ThingDef thingDef)
        {
            this.def = thingDef;

            InitOriginalData();
        }
        protected override void Apply()
        {
            if (def.Verbs.NullOrEmpty())
            {
                return;
            }

            if (!(def.Verbs[0] is VerbPropertiesCE))
            {
                def.Verbs[0] = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(def.Verbs[0]);
            }

            //apply
            foreach (var item in propNames)
            {
                PropUtility.SetPropValueString(verbPropertiesCE, item, this.propDic[item]);
            }

            verbPropertiesCE.defaultProjectile = this.defaultProjectile;
            verbPropertiesCE.recoilPattern = this.recoilPattern;
            verbPropertiesCE.verbClass = this.verbClass;
            verbPropertiesCE.soundCast = this.soundCast;
            verbPropertiesCE.soundCastTail = this.soundCastTail;
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && verbPropertiesCE != null)
            {
                //保存数据
                if (propDic == null)
                {
                    propDic = new Dictionary<string, string>();
                }
                propDic.Clear();
                foreach (var item in propNames)
                {
                    this.propDic[item] = PropUtility.GetPropValue(verbPropertiesCE, item).ToString();
                }

                this.defaultProjectile = verbPropertiesCE.defaultProjectile;
                this.recoilPattern = verbPropertiesCE.recoilPattern;
                this.verbClass = verbPropertiesCE.verbClass;

                this.soundCast = verbPropertiesCE.soundCast;
                this.soundCastTail = verbPropertiesCE.soundCastTail;
            }

            Scribe_Values.Look(ref defaultProjectileString, "defaultProjectile");
            Scribe_Values.Look(ref recoilPattern, "recoilPattern");

            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (propDic == null)
                {
                    propDic = new Dictionary<string, string>();
                }
            }

            Scribe_Values.Look(ref verbClass, "verbClass", null);

            Scribe_Values.Look(ref soundCastString, "soundCast");
            Scribe_Values.Look(ref soundCastTailString, "soundCastTail");
        }
        public override void Reset()
        {
            if (def.Verbs.NullOrEmpty())
            {
                return;
            }

            def.Verbs[0] = this.originalData;
        }
        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;

            if (thingDef.Verbs.NullOrEmpty())
                return;

            InitOriginalData();

            if (!thingDef.Verbs.NullOrEmpty() && !(thingDef.Verbs[0] is VerbPropertiesCE))
            {
                thingDef.Verbs[0] = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(thingDef.Verbs[0]);
            }

            this.Apply();
        }
        protected override void InitOriginalData()
        {
            if (def == null || def.Verbs.NullOrEmpty())
            {
                originalData = null;
                return;
            }

            if (verbPropertiesCE != null)
            {
                this.originalData = new VerbPropertiesCE();
                PropUtility.CopyPropValue<VerbPropertiesCE>(def.Verbs[0] as VerbPropertiesCE, this.originalData as VerbPropertiesCE);
            }
            else
            {
                this.originalData = new VerbProperties();
                PropUtility.CopyPropValue<VerbProperties>(def.Verbs[0], this.originalData);
            }
        }

    }
}
