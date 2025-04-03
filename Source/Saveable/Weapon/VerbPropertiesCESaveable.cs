using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class VerbPropertiesCESaveable : SaveableBase
    {
        //From CE
        public RecoilPattern recoilPattern = RecoilPattern.None;

        public int ammoConsumedPerShotCount = 1;

        public float recoilAmount = 0f;

        public float indirectFirePenalty = 0f;

        public float circularError = 0f;

        public int ticksToTruePosition = 5;

        public bool ejectsCasings = true;

        public bool ignorePartialLoSBlocker = false;

        public bool interruptibleBurst = true;

        //From Vanilla
        public bool hasStandardCommand = true;

        public ThingDef defaultProjectile = null;

        public float warmupTime = 0f;

        public float range = 0f;

        public int burstShotCount;

        public int ticksBetweenBurstShots;

        public SoundDef soundCast = null;

        public SoundDef soundCastTail = null;

        public float muzzleFlashScale = 0f;

        //私有
        private VerbPropertiesCESaveable Original => (VerbPropertiesCESaveable)originalData;
        internal VerbPropertiesCESaveable() { }
        internal VerbPropertiesCESaveable(VerbPropertiesCE verbPropertiesCE, bool isOrignal = false)
        {
            //CE
            this.recoilPattern = verbPropertiesCE.recoilPattern;
            this.ammoConsumedPerShotCount = verbPropertiesCE.ammoConsumedPerShotCount;
            this.recoilAmount = verbPropertiesCE.recoilAmount;
            this.indirectFirePenalty = verbPropertiesCE.indirectFirePenalty;
            this.circularError = verbPropertiesCE.circularError;
            this.ticksToTruePosition = verbPropertiesCE.ticksToTruePosition;
            this.ejectsCasings = verbPropertiesCE.ejectsCasings;
            this.ignorePartialLoSBlocker = verbPropertiesCE.ignorePartialLoSBlocker;
            this.interruptibleBurst = verbPropertiesCE.interruptibleBurst;

            //vanilla
            this.hasStandardCommand = verbPropertiesCE.hasStandardCommand;
            this.defaultProjectile = verbPropertiesCE.defaultProjectile;
            this.warmupTime = verbPropertiesCE.warmupTime;
            this.range = verbPropertiesCE.range;
            this.burstShotCount = verbPropertiesCE.burstShotCount;
            this.ticksBetweenBurstShots = verbPropertiesCE.ticksBetweenBurstShots;
            this.soundCast = verbPropertiesCE.soundCast;
            this.soundCastTail = verbPropertiesCE.soundCastTail;
            this.muzzleFlashScale = verbPropertiesCE.muzzleFlashScale;


            if (!isOrignal)
                this.originalData = new VerbPropertiesCESaveable(verbPropertiesCE, true);
        }

        public override void Apply(ThingDef thingDef)
        {
            VerbPropertiesCE verbProperties = thingDef.Verbs[0] as VerbPropertiesCE;
            if (verbProperties == null)
            {
                Log.Error("VerbPropertiesCE not found for thing " + thingDef.defName);
                return;
            }

            //CE
            verbProperties.recoilPattern = this.recoilPattern;
            verbProperties.ammoConsumedPerShotCount = this.ammoConsumedPerShotCount;
            verbProperties.recoilAmount = this.recoilAmount;
            verbProperties.indirectFirePenalty = this.indirectFirePenalty;
            verbProperties.circularError = this.circularError;
            verbProperties.ticksToTruePosition = this.ticksToTruePosition;
            verbProperties.ejectsCasings = this.ejectsCasings;
            verbProperties.ignorePartialLoSBlocker = this.ignorePartialLoSBlocker;
            verbProperties.interruptibleBurst = this.interruptibleBurst;

            //vanilla
            verbProperties.hasStandardCommand = this.hasStandardCommand;
            verbProperties.defaultProjectile = this.defaultProjectile;
            verbProperties.warmupTime = this.warmupTime;
            verbProperties.range = this.range;
            verbProperties.burstShotCount = this.burstShotCount;
            verbProperties.ticksBetweenBurstShots = this.ticksBetweenBurstShots;
            verbProperties.soundCast = this.soundCast;
            verbProperties.soundCastTail = this.soundCastTail;
            verbProperties.muzzleFlashScale = this.muzzleFlashScale;
        }

        public override void ExposeData()
        {
            //CE
            Scribe_Values.Look(ref this.recoilPattern, "recoilPattern", RecoilPattern.None);
            Scribe_Values.Look(ref this.ammoConsumedPerShotCount, "ammoConsumedPerShotCount", 1);
            Scribe_Values.Look(ref this.recoilAmount, "recoilAmount", 0f);
            Scribe_Values.Look(ref this.indirectFirePenalty, "indirectFirePenalty", 0f);
            Scribe_Values.Look(ref this.circularError, "circularError", 0f);
            Scribe_Values.Look(ref this.ticksToTruePosition, "ticksToTruePosition", 5);
            Scribe_Values.Look(ref this.ejectsCasings, "ejectsCasings", true);
            Scribe_Values.Look(ref this.ignorePartialLoSBlocker, "ignorePartialLoSBlocker", false);
            Scribe_Values.Look(ref this.interruptibleBurst, "interruptibleBurst", true);

            //vanilla
            Scribe_Values.Look(ref this.hasStandardCommand, "hasStandardCommand", true);
            Scribe_Defs.Look(ref this.defaultProjectile, "defaultProjectile");
            Scribe_Values.Look(ref this.warmupTime, "warmupTime", 0f);
            Scribe_Values.Look(ref this.range, "range", 0f);
            Scribe_Values.Look(ref this.burstShotCount, "burstShotCount", 0);
            Scribe_Values.Look(ref this.ticksBetweenBurstShots, "ticksBetweenBurstShots", 0);
            Scribe_Defs.Look(ref this.soundCast, "soundCast");
            Scribe_Defs.Look(ref this.soundCastTail, "soundCastTail");
            Scribe_Values.Look(ref this.muzzleFlashScale, "muzzleFlashScale", 0f);
        }

        public override void Reset(ThingDef thingDef)
        {
            if (this.Original == null)
            {
                return;
            }

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            if (!thingDef.Verbs.NullOrEmpty())
            {
                this.originalData = new VerbPropertiesCESaveable(thingDef.Verbs[0] as VerbPropertiesCE, true);
            }
        }
    }
}
