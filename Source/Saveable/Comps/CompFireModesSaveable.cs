using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class CompFireModesSaveable : SaveableBase
    {
        public int aimedBurstShotCount = -1;

        public bool aiUseBurstMode = false;

        public bool noSingleShot = false;

        public bool noSnapshot = false;

        public AimMode aiAimMode = AimMode.AimedShot;

        private CompFireModesSaveable Original => originalData as CompFireModesSaveable;
        public CompFireModesSaveable() { }

        public CompFireModesSaveable(ThingDef thingDef, bool isOriginal = false) : base(thingDef, isOriginal)
        {
            if(thingDef == null || !thingDef.HasComp<CompFireModes>())
            {
                return;
            }

            CompProperties_FireModes props = thingDef.GetCompProperties<CompProperties_FireModes>();

            aimedBurstShotCount = props.aimedBurstShotCount;
            aiUseBurstMode = props.aiUseBurstMode;
            noSingleShot = props.noSingleShot;
            noSnapshot = props.noSnapshot;
            aiAimMode = props.aiAimMode;

            if (!isOriginal)
                originalData = new CompFireModesSaveable(thingDef, true);
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null || !thingDef.HasComp<CompFireModes>())
            {
                return;
            }
            CompProperties_FireModes props = thingDef.GetCompProperties<CompProperties_FireModes>();
            
            props.aimedBurstShotCount = aimedBurstShotCount;
            props.aiUseBurstMode = aiUseBurstMode;
            props.noSingleShot = noSingleShot;
            props.noSnapshot = noSnapshot;
            props.aiAimMode = aiAimMode;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref aimedBurstShotCount, "aimedBurstShotCount", -1);
            Scribe_Values.Look(ref aiUseBurstMode, "aiUseBurstMode", false);
            Scribe_Values.Look(ref noSingleShot, "noSingleShot", false);
            Scribe_Values.Look(ref noSnapshot, "noSnapshot", false);
            Scribe_Values.Look(ref aiAimMode, "aiAimMode", AimMode.AimedShot);
        }

        public override void Reset(ThingDef thingDef)
        {
            if(Original == null || thingDef == null)
            {
                return;
            }

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {

            this.originalData = new CompFireModesSaveable(thingDef, true);
        }
    }
}
