using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{

    internal class HediffToolExpo : IExposable
    {
        //字段
        private string id;

        public float armorPenetrationSharp;

        public float armorPenetrationBlunt;

        public float power;

        public HediffToolExpo() { }
        public HediffToolExpo(Tool tool)
        {
            this.power = tool.power;

            if (tool is ToolCE toolCE)
            {
                this.armorPenetrationSharp = toolCE.armorPenetrationSharp;
                this.armorPenetrationBlunt = toolCE.armorPenetrationBlunt;
            }
            else
            {
                this.armorPenetrationSharp = tool.armorPenetration;
                this.armorPenetrationBlunt = tool.armorPenetration;
            }
        }

        public void Apply(Tool tool)
        {
            if (!(tool is ToolCE))
            {
                return;
            }

            ToolCE toolCE = tool as ToolCE;

            toolCE.power = this.power;
            toolCE.armorPenetrationSharp = this.armorPenetrationSharp;
            toolCE.armorPenetrationBlunt = this.armorPenetrationBlunt;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref armorPenetrationSharp, "armorPenetrationSharp", 0f);
            Scribe_Values.Look(ref armorPenetrationBlunt, "armorPenetrationBlunt", 0f);
            Scribe_Values.Look(ref power, "power", 0f);
        }

        public override bool Equals(object obj)
        {
            if (obj is HediffToolExpo other)
            {
                return armorPenetrationSharp == other.armorPenetrationSharp &&
                       armorPenetrationBlunt == other.armorPenetrationBlunt &&
                       power == other.power;
            }
            return false;
        }
    }

    internal class HediffComp_VerbGiverSaveable : SaveableBase<HediffDef>
    {
        private HediffCompProperties_VerbGiver verbGiver
        {
            get
            {
                if (def == null || def.comps == null)
                {
                    return null;
                }

                return def.comps.FirstOrDefault(c => c is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;
            }
        }

        private List<HediffToolExpo> tools_save = new List<HediffToolExpo>();
        private List<HediffToolExpo> tools_original = new List<HediffToolExpo>();

        public List<HediffToolExpo> OriginalData => tools_original;
        public bool Changed
        {
            get
            {
                if (NullCheck())
                {
                    return false;
                }

                for (int i = 0; i < verbGiver.tools.Count; i++)
                {
                    HediffToolExpo current = new HediffToolExpo(verbGiver.tools[i]);
                    HediffToolExpo original = tools_original[i];

                    if (!current.Equals(original))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        public HediffComp_VerbGiverSaveable() { }
        public HediffComp_VerbGiverSaveable(HediffDef def) : base(def)
        {
        }


        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (!NullCheck())
                {
                    tools_save.Clear();
                    foreach (var tool in verbGiver.tools)
                    {
                        tools_save.Add(new HediffToolExpo(tool));
                    }
                }
            }

            Scribe_Collections.Look(ref tools_save, "tools_save", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (tools_save == null)
                {
                    tools_save = new List<HediffToolExpo>();
                }
            }
        }

        public override void Reset()
        {
            if (NullCheck())
            {
                return;
            }

            if (tools_original.Count != verbGiver.tools.Count)
            {
                Log.Error($"Error while resetting HediffComp_VerbGiver: tools_original count {tools_original.Count} does not match verbGiver.tools count {verbGiver.tools.Count}");

                return;
            }

            for (int i = 0; i < verbGiver.tools.Count; i++)
            {
                tools_original[i].Apply(verbGiver.tools[i]);
            }
        }

        protected override void Apply()
        {
            if (NullCheck())
            {
                return;
            }

            if (tools_save.Count != verbGiver.tools.Count)
            {
                Log.Error($"Error while applying HediffComp_VerbGiverSaveable: tools_save count {tools_save.Count} does not match verbGiver.tools count {verbGiver.tools.Count}");
                return;
            }

            if(verbGiver.tools.Any(t => !(t is ToolCE)))
            {
                List<Tool> newTools = new List<Tool>();

                foreach(var tool in verbGiver.tools)
                {
                   newTools.Add(PropUtility.ConvertToChild<Tool, ToolCE>(tool));
                }

                verbGiver.tools = newTools;
            }


            for (int i = 0; i < verbGiver.tools.Count; i++)
            {
                tools_save[i].Apply(verbGiver.tools[i]);
            }
        }

        protected override void InitOriginalData()
        {
            if (NullCheck())
            {
                return;
            }

            tools_original.Clear();
            foreach (var tool in verbGiver.tools)
            {
                tools_original.Add(new HediffToolExpo(tool));
            }
        }

        protected override bool NullCheck()
        {
            return def == null || verbGiver == null || verbGiver.tools == null;
        }
    }
}
