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
    class ExtraDamageExpo : IExposable
    {
        public float amount;
        public string defString;
        public float chance;
        public void ExposeData()
        {
            Scribe_Values.Look(ref amount, "amount");
            Scribe_Values.Look(ref defString, "defString");
            Scribe_Values.Look(ref chance, "chance");
        }
    }

    internal class ToolCESaveable : SaveableBase<ThingDef>
    {
        public static ReadOnlyCollection<string> propNames = new List<string>()
        {
                "armorPenetrationBlunt",
                "armorPenetrationSharp",
                "power",
                "cooldownTime",
                "alwaysTreatAsWeapon",
                "chanceFactor",
                "ensureLinkedBodyPartsGroupAlwaysUsable"
        }.AsReadOnly();
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private List<string> capacitiesString = new List<string>();
        private string linkedBodyPartsGroupString;
        internal BodyPartGroupDef linkedBodyPartsGroup
        {
            get => DefDatabase<BodyPartGroupDef>.GetNamed(linkedBodyPartsGroupString, false);
            set => linkedBodyPartsGroupString = value?.defName ?? "null";
        }

        private List<ExtraDamageExpo> surpriseAttackSave = new List<ExtraDamageExpo>();
        private List<ExtraDamageExpo> extraDamages = new List<ExtraDamageExpo>();

        private string label;
        internal string id;
        private ToolCE tool
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                if (def.tools.NullOrEmpty())
                {
                    return null;
                }
                return def.tools.FirstOrDefault(t => t.id == id) as ToolCE;
            }
        }

        public ToolCESaveable() { }

        public ToolCESaveable(ThingDef thingDef, string id)
        {
            this.id = id;
            this.def = thingDef;
            if (tool == null)
            {
                return;
            }
            InitOriginalData();
        }

        protected override void Apply()
        {
            if (tool == null)
            {
                def.tools.Add(new ToolCE()
                {
                    id = this.id,
                    label = this.label
                });
            }

            foreach (var item in propNames)
            {
                if (propDic.ContainsKey(item))
                {
                    PropUtility.SetPropValueString(tool, item, propDic[item]);
                }
            }

            //capacities
            foreach (var item in this.capacitiesString)
            {
                var def = DefDatabase<ToolCapacityDef>.GetNamed(item, false);
                if (def != null)
                {
                    tool.capacities.Add(def);
                }
            }

            //surprise attack
            if (tool.surpriseAttack == null)
            {
                tool.surpriseAttack = new SurpriseAttackProps();
                tool.surpriseAttack.extraMeleeDamages = new List<ExtraDamage>();
            }
            tool.surpriseAttack.extraMeleeDamages.Clear();
            foreach (var item in this.surpriseAttackSave)
            {
                var def = DefDatabase<DamageDef>.GetNamed(item.defString, false);
                if (def != null)
                {
                    tool.surpriseAttack.extraMeleeDamages.Add(new ExtraDamage()
                    {
                        amount = item.amount,
                        def = def,
                        chance = item.chance
                    });
                }
            }

            //extra Melee Damages
            if (tool.extraMeleeDamages == null)
            {
                tool.extraMeleeDamages = new List<ExtraDamage>();
            }
            tool.extraMeleeDamages.Clear();
            foreach (var item in this.extraDamages)
            {
                var def = DefDatabase<DamageDef>.GetNamed(item.defString, false);
                if (def != null)
                {
                    tool.extraMeleeDamages.Add(new ExtraDamage()
                    {
                        amount = item.amount,
                        def = def,
                        chance = item.chance
                    });
                }
            }

            tool.linkedBodyPartsGroup = this.linkedBodyPartsGroup;
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving &&
                tool != null)
            {
                //save
                foreach (var item in propNames)
                {
                    propDic[item] = PropUtility.GetPropValue(tool, item).ToString();
                }

                this.capacitiesString.Clear();
                tool.capacities?.ForEach(x => this.capacitiesString.Add(x.defName));
                if (tool.capacities.NullOrEmpty())
                {
                    Log.Error($"[CeManualPatcher] Capacities (Damage types) is Empty, this could cause some errors, thing: {def.defName} ({def.LabelCap}), tool: {tool.label}");
                }

                //surprise attack
                this.surpriseAttackSave.Clear();
                tool.surpriseAttack?.extraMeleeDamages.ForEach(x =>
                {
                    surpriseAttackSave.Add(new ExtraDamageExpo()
                    {
                        amount = x.amount,
                        defString = x.def.defName,
                        chance = x.chance
                    });
                });

                //extra Melee Damages
                this.extraDamages.Clear();
                tool.extraMeleeDamages?.ForEach(x =>
                {
                    extraDamages.Add(new ExtraDamageExpo()
                    {
                        amount = x.amount,
                        defString = x.def.defName,
                        chance = x.chance
                    });
                });

                this.linkedBodyPartsGroup = tool.linkedBodyPartsGroup;

                this.label = tool.label;
            }

            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref label, "label");

            Scribe_Values.Look(ref linkedBodyPartsGroupString, "linkedBodyPartsGroup", "null");

            Scribe_Collections.Look(ref capacitiesString, "capacities", LookMode.Value);
            Scribe_Collections.Look(ref surpriseAttackSave, "surpriseAttack", LookMode.Deep);
            Scribe_Collections.Look(ref extraDamages, "extraDamages", LookMode.Deep);
            Scribe_Collections.Look(ref propDic, "propDic", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (this.capacitiesString == null)
                {
                    this.capacitiesString = new List<string>();
                }

                if (this.surpriseAttackSave == null)
                {
                    this.surpriseAttackSave = new List<ExtraDamageExpo>();
                }
                if (this.propDic == null)
                {
                    this.propDic = new Dictionary<string, string>();
                }
                if (this.extraDamages == null)
                {
                    this.extraDamages = new List<ExtraDamageExpo>();
                }
            }
        }
        public override void Reset()
        {
            //do nothing
        }
        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            InitOriginalData();
            this.Apply();
        }
        protected override void InitOriginalData()
        {
            //do nothing
        }

        //static methods
        public static void InitTools(ThingDef def, ref List<Tool> originalTools)
        {
            if (def == null || def.tools == null)
            {
                return;
            }
            originalTools = new List<Tool>();
            foreach (var item in def.tools)
            {
                Tool tool = new Tool();

                if (item is ToolCE)
                {
                    tool = new ToolCE();
                    PropUtility.CopyPropValue<ToolCE>(item as ToolCE, tool as ToolCE);
                }
                else
                {
                    PropUtility.CopyPropValue<Tool>(item, tool);
                }

                List<ToolCapacityDef> toolCapacityDefs = new List<ToolCapacityDef>(item.capacities);
                tool.capacities = toolCapacityDefs;

                //extra melee damages
                List<ExtraDamage> extraDamages = new List<ExtraDamage>();
                item.extraMeleeDamages?.ForEach(x => extraDamages.Add(new ExtraDamage()
                {
                    amount = x.amount,
                    def = x.def,
                    chance = x.chance,
                    armorPenetration = x.armorPenetration
                }));
                tool.extraMeleeDamages = extraDamages;

                //surprise attack
                if (item.surpriseAttack != null)
                {
                    tool.surpriseAttack = new SurpriseAttackProps();
                    List<ExtraDamage> surpriseAttackExtraDamages = new List<ExtraDamage>();
                    item.surpriseAttack.extraMeleeDamages?.ForEach(x => surpriseAttackExtraDamages.Add(new ExtraDamage()
                    {
                        amount = x.amount,
                        def = x.def,
                        chance = x.chance,
                        armorPenetration = x.armorPenetration
                    }));
                    tool.surpriseAttack.extraMeleeDamages = surpriseAttackExtraDamages;
                }

                originalTools.Add(tool);
            }
        }

        public static void InitSaveTools(ThingDef def, ref List<ToolCESaveable> saveTools)
        {
            if (def == null || def.tools == null)
            {
                return;
            }

            saveTools = new List<ToolCESaveable>();
            foreach (var item in def.tools)
            {
                if (item is ToolCE)
                {
                    saveTools.Add(new ToolCESaveable(def, item.id));
                }
            }
        }
    }

}
