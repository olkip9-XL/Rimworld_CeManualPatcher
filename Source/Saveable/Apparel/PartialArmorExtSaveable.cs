using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CeManualPatcher.Saveable.Apparel
{
    internal class PartialArmorExtSaveable : SaveableBase<ThingDef>
    {

        List<PartialArmorExtItem> stats = new List<PartialArmorExtItem>();


        private PartialArmorExt partialArmorExt
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                if (def.HasModExtension<PartialArmorExt>())
                {
                    return def.GetModExtension<PartialArmorExt>();
                }
                else
                {
                    return null;
                }
            }
        }

        private List<ApparelPartialStat> originalApparelPartialStats = new List<ApparelPartialStat>();

        public PartialArmorExtSaveable() { }

        public PartialArmorExtSaveable(ThingDef thingDef)
        {
            this.def = thingDef;
            if (partialArmorExt == null)
            {
                return;
            }

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && partialArmorExt != null)
            {
                this.stats.Clear();
                foreach (var item in partialArmorExt.stats)
                {
                    this.stats.Add(new PartialArmorExtItem(item));
                    if (item.parts.NullOrEmpty())
                    {
                        Log.Error($"[CeManualPatcher] Body parts of Partial Armor is Empty, this could cause some errors, thing: {def.defName} ({def.LabelCap})");
                    }
                }
            }

            Scribe_Collections.Look(ref stats, "stats", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (stats == null)
                {
                    stats = new List<PartialArmorExtItem>();
                }
            }
        }

        public override void Reset()
        {
            if (partialArmorExt == null)
            {
                return;
            }

            partialArmorExt.stats = this.originalApparelPartialStats;
            InitOriginalData();
        }

        protected override void Apply()
        {
            //Do nothing
        }

        protected override void InitOriginalData()
        {
            originalApparelPartialStats = new List<ApparelPartialStat>();
            foreach (var item in partialArmorExt.stats)
            {
                ApparelPartialStat apparelPartialStat = new ApparelPartialStat();
                PropUtility.CopyPropValue(item, apparelPartialStat);

                List<BodyPartDef> bodyPartDefs = new List<BodyPartDef>();
                bodyPartDefs.AddRange(item.parts);
                apparelPartialStat.parts = bodyPartDefs;

                originalApparelPartialStats.Add(apparelPartialStat);
            }
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            if (partialArmorExt == null)
            {
                thingDef.AddModExtension(new PartialArmorExt()
                {
                    stats = new List<ApparelPartialStat>()
                });
            }
            InitOriginalData();

            this.partialArmorExt.stats.Clear();
            this.stats?.ForEach(x => x?.PostLoadInit(thingDef));
        }
    }
}
