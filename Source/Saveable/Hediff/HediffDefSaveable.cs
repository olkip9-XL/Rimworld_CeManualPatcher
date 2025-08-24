using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{

    internal class HediffStageExpo : IExposable
    {
        //props
        public float minSeverity = 0f;
        public float severityGainFactor = 1f;

        //statOffset
        public float statOffset_ArmorRating_Sharp = 0f;
        public float statOffset_ArmorRating_Blunt = 0f;
        public float statOffset_ArmorRating_Heat = 0f;

        //statFactors
        public float statFactor_ArmorRating_Sharp = 1f;
        public float statFactor_ArmorRating_Blunt = 1f;
        public float statFactor_ArmorRating_Heat = 1f;


        public HediffStageExpo() { }
        public HediffStageExpo(HediffStage stage)
        {
            this.minSeverity = stage.minSeverity;
            this.severityGainFactor = stage.severityGainFactor;

            this.statOffset_ArmorRating_Sharp = stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Sharp);
            this.statOffset_ArmorRating_Blunt = stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Blunt);
            this.statOffset_ArmorRating_Heat = stage.statOffsets.GetStatOffsetFromList(StatDefOf.ArmorRating_Heat);

            this.statFactor_ArmorRating_Sharp = stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Sharp);
            this.statFactor_ArmorRating_Blunt = stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Blunt);
            this.statFactor_ArmorRating_Heat = stage.statFactors.GetStatFactorFromList(StatDefOf.ArmorRating_Heat);
        }

        public void Apply(HediffStage stage)
        {
            stage.minSeverity = minSeverity;
            stage.severityGainFactor = severityGainFactor;

            //statOffset
            if(stage.statOffsets == null)
            {
                stage.statOffsets = new List<StatModifier>();
            }
            foreach (var statOffset in stage.statOffsets)
            {
                if (statOffset.stat == StatDefOf.ArmorRating_Sharp)
                {
                    statOffset.value = statOffset_ArmorRating_Sharp;
                }
                else if (statOffset.stat == StatDefOf.ArmorRating_Blunt)
                {
                    statOffset.value = statOffset_ArmorRating_Blunt;
                }
                else if (statOffset.stat == StatDefOf.ArmorRating_Heat)
                {
                    statOffset.value = statOffset_ArmorRating_Heat;
                }
            }
            if (!stage.statOffsets.Any(x => x.stat == StatDefOf.ArmorRating_Sharp))
            {
                stage.statOffsets.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Sharp, value = statOffset_ArmorRating_Sharp });
            }
            if (!stage.statOffsets.Any(x => x.stat == StatDefOf.ArmorRating_Blunt))
            {
                stage.statOffsets.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Blunt, value = statOffset_ArmorRating_Blunt });
            }
            if (!stage.statOffsets.Any(x => x.stat == StatDefOf.ArmorRating_Heat))
            {
                stage.statOffsets.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Heat, value = statOffset_ArmorRating_Heat });
            }

            //statFactor
            if(stage.statFactors == null)
            {
                stage.statFactors = new List<StatModifier>();
            }
            foreach (var statFactor in stage.statFactors)
            {
                if (statFactor.stat == StatDefOf.ArmorRating_Sharp)
                {
                    statFactor.value = statFactor_ArmorRating_Sharp;
                }
                else if (statFactor.stat == StatDefOf.ArmorRating_Blunt)
                {
                    statFactor.value = statFactor_ArmorRating_Blunt;
                }
                else if (statFactor.stat == StatDefOf.ArmorRating_Heat)
                {
                    statFactor.value = statFactor_ArmorRating_Heat;
                }
            }
            if (!stage.statFactors.Any(x => x.stat == StatDefOf.ArmorRating_Sharp))
            {
                stage.statFactors.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Sharp, value = statFactor_ArmorRating_Sharp });
            }
            if (!stage.statFactors.Any(x => x.stat == StatDefOf.ArmorRating_Blunt))
            {
                stage.statFactors.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Blunt, value = statFactor_ArmorRating_Blunt });
            }
            if (!stage.statFactors.Any(x => x.stat == StatDefOf.ArmorRating_Heat))
            {
                stage.statFactors.Add(new StatModifier() { stat = StatDefOf.ArmorRating_Heat, value = statFactor_ArmorRating_Heat });
            }

        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref minSeverity, "minSeverity", 0f);
            Scribe_Values.Look(ref severityGainFactor, "severityGainFactor", 1f);

            Scribe_Values.Look(ref statOffset_ArmorRating_Sharp, "statOffset_ArmorRating_Sharp", 0f);
            Scribe_Values.Look(ref statOffset_ArmorRating_Blunt, "statOffset_ArmorRating_Blunt", 0f);
            Scribe_Values.Look(ref statOffset_ArmorRating_Heat, "statOffset_ArmorRating_Heat", 0f);

            Scribe_Values.Look(ref statFactor_ArmorRating_Sharp, "statFactor_ArmorRating_Sharp", 1f);
            Scribe_Values.Look(ref statFactor_ArmorRating_Blunt, "statFactor_ArmorRating_Blunt", 1f);
            Scribe_Values.Look(ref statFactor_ArmorRating_Heat, "statFactor_ArmorRating_Heat", 1f);
        }

        public override bool Equals(object obj)
        {
            if(obj is HediffStageExpo other)
            {
                return minSeverity == other.minSeverity &&
                       severityGainFactor == other.severityGainFactor &&
                       statOffset_ArmorRating_Sharp == other.statOffset_ArmorRating_Sharp &&
                       statOffset_ArmorRating_Blunt == other.statOffset_ArmorRating_Blunt &&
                       statOffset_ArmorRating_Heat == other.statOffset_ArmorRating_Heat &&
                       statFactor_ArmorRating_Sharp == other.statFactor_ArmorRating_Sharp &&
                       statFactor_ArmorRating_Blunt == other.statFactor_ArmorRating_Blunt &&
                       statFactor_ArmorRating_Heat == other.statFactor_ArmorRating_Heat;
            }

            return false;
        }

    }


    internal class HediffDefSaveable : SaveableBase<HediffDef>
    {
        private List<HediffStageExpo> stages_save = new List<HediffStageExpo>();
        private List<HediffStageExpo> stages_original = new List<HediffStageExpo>();

        public bool Changed
        {
            get
            {
                if(stages_original.NullOrEmpty())
                    return false;

                for(int i = 0; i < stages_original.Count; i++)
                {
                    HediffStageExpo current = new HediffStageExpo(def.stages[i]);
                    HediffStageExpo original = stages_original[i];

                    if(!current.Equals(original))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public List<HediffStageExpo> OriginalData => stages_original;

        public HediffDefSaveable() { }
        public HediffDefSaveable(HediffDef def) : base(def)
        {
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (!NullCheck())
                {
                    stages_save = new List<HediffStageExpo>();
                    foreach (var stage in def.stages)
                    {
                        stages_save.Add(new HediffStageExpo(stage));
                    }
                }
            }

            Scribe_Collections.Look(ref stages_save, "stages", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (stages_save == null)
                {
                    stages_save = new List<HediffStageExpo>();
                }
            }

        }

        public override void Reset()
        {
            if (NullCheck())
                return;

            if (stages_original.Count != def.stages.Count)
            {
                Log.Error($"HediffDefSaveable: {def.defName} stages count mismatch. Expected: {def.stages.Count}, Actual: {stages_original.Count}");
                return;
            }

            for (int i = 0; i < def.stages.Count; i++)
            {
                stages_original[i].Apply(def.stages[i]);
            }
        }

        protected override void Apply()
        {
            if (NullCheck())
                return;

            if (stages_save.Count != def.stages.Count)
            {
                Log.Error($"HediffDefSaveable: {def.defName} stages count mismatch. Expected: {def.stages.Count}, Actual: {stages_save.Count}");
                return;
            }

            for (int i = 0; i < def.stages.Count; i++)
            {
                stages_save[i].Apply(def.stages[i]);
            }
        }

        protected override void InitOriginalData()
        {
            this.stages_original = new List<HediffStageExpo>();
            foreach (var stage in def.stages)
            {
                stages_original.Add(new HediffStageExpo(stage));
            }
        }

        protected override bool NullCheck()
        {
            return def == null || def.stages.NullOrEmpty();
        }
    }
}
