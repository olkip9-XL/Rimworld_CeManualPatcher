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

        public Dictionary<StatDef, float> offsetDic = new Dictionary<StatDef, float>();
        private Dictionary<string, float> offsetDicString = new Dictionary<string, float>();

        public Dictionary<StatDef, float> factorDic = new Dictionary<StatDef, float>();
        private Dictionary<string, float> factorDicString = new Dictionary<string, float>();

        public HediffStageExpo() { }
        public HediffStageExpo(HediffStage stage)
        {
            this.minSeverity = stage.minSeverity;
            this.severityGainFactor = stage.severityGainFactor;

            if (stage.statOffsets != null)
                foreach (var statOffset in stage.statOffsets)
                {
                    offsetDic[statOffset.stat] = statOffset.value;
                }

            if (stage.statFactors != null)
                foreach (var statFactor in stage.statFactors)
                {
                    factorDic[statFactor.stat] = statFactor.value;
                }
        }

        public void Apply(HediffStage stage)
        {
            //construct dic for save
            if (offsetDicString.Any())
            {
                offsetDic.Clear();
                foreach (var kvp in offsetDicString)
                {
                    StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(kvp.Key);
                    if (statDef != null)
                    {
                        offsetDic[statDef] = kvp.Value;
                    }
                }
                offsetDicString.Clear();
            }

            if(factorDicString.Any())
            {
                factorDic.Clear();
                foreach (var kvp in factorDicString)
                {
                    StatDef statDef = DefDatabase<StatDef>.GetNamedSilentFail(kvp.Key);
                    if (statDef != null)
                    {
                        factorDic[statDef] = kvp.Value;
                    }
                }
                factorDicString.Clear();
            }

            stage.minSeverity = minSeverity;
            stage.severityGainFactor = severityGainFactor;

            //statOffset
            if (stage.statOffsets == null)
            {
                stage.statOffsets = new List<StatModifier>();
            }
            stage.statOffsets.Clear();

            foreach (var kvp in offsetDic)
            {
                if (stage.statOffsets.Any(x => x.stat == kvp.Key))
                {
                    stage.statOffsets.First(x => x.stat == kvp.Key).value = kvp.Value;
                }
                else
                {
                    stage.statOffsets.Add(new StatModifier() { stat = kvp.Key, value = kvp.Value });
                }
            }

            //statFactor
            if (stage.statFactors == null)
            {
                stage.statFactors = new List<StatModifier>();
            }
            stage.statFactors.Clear();

            foreach (var kvp in factorDic)
            {
                if (stage.statFactors.Any(x => x.stat == kvp.Key))
                {
                    stage.statFactors.First(x => x.stat == kvp.Key).value = kvp.Value;
                }
                else
                {
                    stage.statFactors.Add(new StatModifier() { stat = kvp.Key, value = kvp.Value });
                }
            }

        }
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                offsetDicString.Clear();
                foreach (var kvp in offsetDic)
                {
                    offsetDicString[kvp.Key.defName] = kvp.Value;
                }

                factorDicString.Clear();
                foreach (var kvp in factorDic)
                {
                    factorDicString[kvp.Key.defName] = kvp.Value;
                }
            }

            Scribe_Values.Look(ref minSeverity, "minSeverity", 0f);
            Scribe_Values.Look(ref severityGainFactor, "severityGainFactor", 1f);

            Scribe_Collections.Look(ref offsetDicString, "offsetDic", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref factorDicString, "factorDic", LookMode.Value, LookMode.Value);

            //old save
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (offsetDicString == null)
                {
                    offsetDicString = new Dictionary<string, float>();
                }
                if (factorDicString == null)
                {
                    factorDicString = new Dictionary<string, float>();
                }

                float val = 0f;
                Scribe_Values.Look(ref val, "statOffset_ArmorRating_Sharp", 0f);
                if (val != 0f)
                {
                    offsetDicString["ArmorRating_Sharp"] = val;
                }

                val = 0f;
                Scribe_Values.Look(ref val, "statOffset_ArmorRating_Blunt", 0f);
                if (val != 0f)
                {
                    offsetDicString["ArmorRating_Blunt"] = val;
                }

                val = 0f;
                Scribe_Values.Look(ref val, "statOffset_ArmorRating_Heat", 0f);
                if (val != 0f)
                {
                    offsetDicString["ArmorRating_Heat"] = val;
                }

                val = 1f;
                Scribe_Values.Look(ref val, "statFactor_ArmorRating_Sharp", 1f);
                if (val != 1f)
                {
                    offsetDicString["ArmorRating_Sharp"] = val;
                }

                val = 1f;
                Scribe_Values.Look(ref val, "statFactor_ArmorRating_Blunt", 1f);
                if (val != 1f)
                {
                    offsetDicString["ArmorRating_Blunt"] = val;
                }

                val = 1f;
                Scribe_Values.Look(ref val, "statFactor_ArmorRating_Heat", 1f);
                if (val != 1f)
                {
                    offsetDicString["ArmorRating_Heat"] = val;
                }
            }

        }

        public override bool Equals(object obj)
        {
            //if(obj is HediffStageExpo other)
            //{
            //    return minSeverity == other.minSeverity &&
            //           severityGainFactor == other.severityGainFactor &&
            //           statOffset_ArmorRating_Sharp == other.statOffset_ArmorRating_Sharp &&
            //           statOffset_ArmorRating_Blunt == other.statOffset_ArmorRating_Blunt &&
            //           statOffset_ArmorRating_Heat == other.statOffset_ArmorRating_Heat &&
            //           statFactor_ArmorRating_Sharp == other.statFactor_ArmorRating_Sharp &&
            //           statFactor_ArmorRating_Blunt == other.statFactor_ArmorRating_Blunt &&
            //           statFactor_ArmorRating_Heat == other.statFactor_ArmorRating_Heat;
            //}
            if (obj is HediffStageExpo other)
            {
                if (minSeverity != other.minSeverity ||
                   severityGainFactor != other.severityGainFactor)
                {
                    return false;
                }


                foreach (var kvp in offsetDic)
                {
                    if (!other.offsetDic.TryGetValue(kvp.Key, out float otherValue) || kvp.Value != otherValue)
                    {
                        return false;
                    }
                }
                foreach (var kvp in other.offsetDic)
                {
                    if (!offsetDic.TryGetValue(kvp.Key, out float thisValue) || kvp.Value != thisValue)
                    {
                        return false;
                    }
                }

                foreach (var kvp in factorDic)
                {
                    if (!other.factorDic.TryGetValue(kvp.Key, out float otherValue) || kvp.Value != otherValue)
                    {
                        return false;
                    }
                }
                foreach (var kvp in other.factorDic)
                {
                    if (!factorDic.TryGetValue(kvp.Key, out float thisValue) || kvp.Value != thisValue)
                    {
                        return false;
                    }
                }

                return true;
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
                if (stages_original.NullOrEmpty())
                    return false;

                for (int i = 0; i < stages_original.Count; i++)
                {
                    HediffStageExpo current = new HediffStageExpo(def.stages[i]);
                    HediffStageExpo original = stages_original[i];

                    if (!current.Equals(original))
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
