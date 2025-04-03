using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    struct ExtraDamageExpo
    {
        public float amount;
        public string defString;
    }

    internal class ToolCESaveable : SaveableBase
    {
        //CE
        internal float armorPenetrationBlunt;
        internal float armorPenetrationSharp;

        //vanilla
        internal float power;
        internal float cooldownTime;
        //internal SurpriseAttackProps surpriseAttack;
        private List<string> capacitiesString = new List<string>();
        internal List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();

        internal BodyPartGroupDef linkedBodyPartsGroup;
        internal bool alwaysTreatAsWeapon;
        internal float chanceFactor;

        private List<ExtraDamageExpo> surpriseAttackSave = new List<ExtraDamageExpo>();
        internal List<ExtraDamage> surpriseAttack = new List<ExtraDamage>();

        internal string label;
        internal string id;
        internal bool needDelete = false;
        private ToolCESaveable Original => base.originalData as ToolCESaveable;

        public ToolCESaveable()
        {
        }

        public ToolCESaveable(ToolCE toolCE, bool isOrginal = false)
        {
            id = toolCE.id;
            label = toolCE.label;

            armorPenetrationBlunt = toolCE.armorPenetrationBlunt;
            armorPenetrationSharp = toolCE.armorPenetrationSharp;
            power = toolCE.power;
            cooldownTime = toolCE.cooldownTime;

            if (toolCE.surpriseAttack != null)
            {
                surpriseAttack.Clear();
                surpriseAttack.AddRange(toolCE.surpriseAttack.extraMeleeDamages);
            }

            capacities.Clear();
            capacities.AddRange(toolCE.capacities);

            linkedBodyPartsGroup = toolCE.linkedBodyPartsGroup;
            alwaysTreatAsWeapon = toolCE.alwaysTreatAsWeapon;
            chanceFactor = toolCE.chanceFactor;

            if (!isOrginal)
            {
                originalData = new ToolCESaveable(toolCE, true);
            }
        }

        public override void Apply(ThingDef thingDef)
        {
            ToolCE tool = thingDef.tools.FirstOrDefault(t => t.id == id) as ToolCE;

            if (tool == null)
            {
                if (needDelete)
                {
                    return;
                }
                tool = new ToolCE();
                tool.id = id;
                tool.label = label;
                thingDef.tools.Add(tool);

                if (Original != null)
                    Original.needDelete = true;
            }

            if (needDelete)
            {
                thingDef.tools.Remove(tool);
                return;
            }
            else
            {
                tool.armorPenetrationBlunt = armorPenetrationBlunt;
                tool.armorPenetrationSharp = armorPenetrationSharp;
                tool.power = power;
                tool.cooldownTime = cooldownTime;

                if (tool.surpriseAttack != null)
                {
                    tool.surpriseAttack.extraMeleeDamages.Clear();
                    tool.surpriseAttack.extraMeleeDamages.AddRange(surpriseAttack);
                }

                tool.capacities.Clear();
                tool.capacities.AddRange(capacities);

                tool.linkedBodyPartsGroup = linkedBodyPartsGroup;
                tool.alwaysTreatAsWeapon = alwaysTreatAsWeapon;
                tool.chanceFactor = chanceFactor;
            }
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.capacitiesString.Clear();
                this.capacities.ForEach(x => this.capacitiesString.Add(x.defName));

                this.surpriseAttackSave.Clear();
                this.surpriseAttack.ForEach(x =>
                {
                    surpriseAttackSave.Add(new ExtraDamageExpo()
                    {
                        amount = x.amount,
                        defString = x.def.defName
                    });
                });
            }

            Scribe_Values.Look(ref armorPenetrationBlunt, "armorPenetrationBlunt");
            Scribe_Values.Look(ref armorPenetrationSharp, "armorPenetrationSharp");
            Scribe_Values.Look(ref power, "power");
            Scribe_Values.Look(ref cooldownTime, "cooldownTime");

            Scribe_Collections.Look(ref capacitiesString, "capacities", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars && capacitiesString == null)
            {
                this.capacitiesString = new List<string>();
            }
            Scribe_Collections.Look(ref surpriseAttackSave, "surpriseAttack", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars && surpriseAttackSave == null)
            {
                this.surpriseAttackSave = new List<ExtraDamageExpo>();
            }

            Scribe_Defs.Look(ref linkedBodyPartsGroup, "linkedBodyPartsGroup");
            Scribe_Values.Look(ref alwaysTreatAsWeapon, "alwaysTreatAsWeapon");
            Scribe_Values.Look(ref chanceFactor, "chanceFactor");

            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref needDelete, "needDelete");
        }
        public override void Reset(ThingDef thingDef)
        {
            if (Original == null || Original == null)
            {
                return;
            }

            this.needDelete = true;

            Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.capacities.Clear();
            if (this.capacitiesString != null)
            {
                foreach (var item in this.capacitiesString)
                {
                    var def = DefDatabase<ToolCapacityDef>.GetNamed(item, false);
                    if (def != null)
                    {
                        this.capacities.Add(def);
                    }
                }
            }

            this.surpriseAttack.Clear();
            if (this.surpriseAttackSave != null)
            {
                foreach (var item in this.surpriseAttackSave)
                {
                    var def = DefDatabase<DamageDef>.GetNamed(item.defString, false);
                    if (def != null)
                    {
                        this.surpriseAttack.Add(new ExtraDamage()
                        {
                            amount = item.amount,
                            def = def
                        });
                    }
                }
            }

            ToolCE tool = thingDef.tools.FirstOrDefault(t => t.id == id) as ToolCE;
            if (tool != null)
            {
                this.originalData = new ToolCESaveable(tool);
            }

        }

    }
}
