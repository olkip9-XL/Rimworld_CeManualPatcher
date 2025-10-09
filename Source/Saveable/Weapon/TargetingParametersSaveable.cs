using CeManualPatcher.Misc;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class TargetingParametersSaveable : SaveableBase<ThingDef>
    {

        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
            "canTargetLocations",
            "canTargetSelf",
            "canTargetPawns",
            "canTargetFires",
            "canTargetBuildings",
            "canTargetItems",
            "canTargetAnimals",
            "canTargetHumans",
            "canTargetMechs",
            "canTargetPlants",
            "canTargetSubhumans",
            "canTargetEntities",
            "canTargetCorpses",
            "canTargetBloodfeeders",

            "mustBeSelectable",

            "neverTargetDoors",
            "neverTargetIncapacitated",
            "neverTargetHostileFaction",

            "onlyTargetFlammables",
            "onlyTargetSameIdeo",
            "onlyTargetThingsAffectingRegions",
            "onlyTargetDamagedThings",
            "mapObjectTargetsMustBeAutoAttackable",
            "onlyTargetIncapacitatedPawns",
            "onlyTargetColonistsOrPrisoners",
            "onlyTargetColonistsOrPrisonersOrSlaves",
            "onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks",
            "onlyTargetControlledPawns",
            "onlyTargetColonists",
            "onlyTargetPrisonersOfColony",
            "onlyTargetPsychicSensitive",
            "onlyTargetAnimaTrees",
            "onlyRepairableMechs",
            "onlyTargetDoors",
            "onlyTargetCorpses",
            "mapBoundsContractedBy"
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();


        //private
        private TargetingParameters originalData = null;

        public TargetingParametersSaveable() { }
        public TargetingParametersSaveable(ThingDef def) : base(def) { }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && !NullCheck())
            {
                if (propDic == null)
                {
                    propDic = new Dictionary<string, string>();
                }
                propDic.Clear();
                foreach (var item in propNames)
                {
                    this.propDic[item] = PropUtility.GetPropValue(def.Verbs[0].targetParams, item).ToString();
                }
            }

            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);

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
            def.Verbs[0].targetParams = originalData;
        }

        protected override void Apply()
        {
            foreach (var item in propNames)
            {
                if (propDic.ContainsKey(item))
                    PropUtility.SetPropValueString(def.Verbs[0].targetParams, item, this.propDic[item]);
            }
        }

        protected override void InitOriginalData()
        {
            this.originalData = new TargetingParameters();
            if (def.Verbs[0].targetParams != null)
                PropUtility.CopyPropValue<TargetingParameters>(def.Verbs[0].targetParams, this.originalData);
        }

        protected override bool NullCheck()
        {
            return def == null || def.Verbs.NullOrEmpty();
        }
    }
}
