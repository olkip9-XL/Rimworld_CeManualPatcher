using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Patch
{
    internal class RacePatch : PatchBase<ThingDef>
    {
        //fields
        internal StatSaveable statBase;

        internal CompArmorDurabilitySaveable armorDurability;

        internal List<ToolCESaveable> tools = new List<ToolCESaveable>();
        private List<Tool> originalTools = new List<Tool>();


        public override string PatchName => "Race";

        public RacePatch() { }

        public RacePatch(ThingDef thingDef)
        {
            if (thingDef == null)
            {
                return;
            }
            targetDef = thingDef;

            statBase = new StatSaveable(thingDef);

            //comps
            if (thingDef.comps != null)
            {
                armorDurability = new CompArmorDurabilitySaveable(thingDef);
            }

            //tools
            if (thingDef.tools != null)
            {
                ToolCESaveable.InitTools(thingDef, ref originalTools);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving && targetDef != null)
            {
                this.tools.Clear();
                ToolCESaveable.InitSaveTools(targetDef, ref this.tools);
            }

            Scribe_Deep.Look(ref statBase, "statBase");
            Scribe_Deep.Look(ref armorDurability, "armorDurability");
            Scribe_Collections.Look(ref tools, "tools", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (tools == null)
                {
                    tools = new List<ToolCESaveable>();
                }
            }
        }

        public override void Reset()
        {
            statBase?.Reset();
            armorDurability?.Reset();

            if (targetDef.tools != null)
            {
                targetDef.tools = this.originalTools;
                ToolCESaveable.InitTools(targetDef, ref originalTools);
            }
        }

        public override void PostLoadInit()
        {
            if (targetDef == null)
            {
                return;
            }

            statBase?.PostLoadInit(targetDef);
            armorDurability?.PostLoadInit(targetDef);

            if (targetDef.tools != null)
            {
                ToolCESaveable.InitTools(targetDef, ref this.originalTools);
                targetDef.tools.Clear();
                this.tools.ForEach(x => x?.PostLoadInit(targetDef));
            }
        }
        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            XmlUtility.Replace_StatBase(xmlDoc, root, targetDef.defName, targetDef.statBases);

            XmlUtility.Replace_Tools(xmlDoc, root, targetDef.defName, targetDef.tools);

            //comps
            MakeCompPatch_ArmorDurability();

            void MakeCompPatch_ArmorDurability()
            {
                if (!this.armorDurability.CompChanged)
                    return;

                XmlElement valueElement = xmlDoc.CreateElement("li");
                valueElement.SetAttribute("Class", typeof(CompProperties_ArmorDurability).ToString());

                CompProperties_ArmorDurability defaultData = new CompProperties_ArmorDurability();
                CompProperties_ArmorDurability currentData = targetDef.GetCompProperties<CompProperties_ArmorDurability>();

                if (defaultData != null && currentData != null)
                {
                    foreach (var fieldName in CompArmorDurabilitySaveable.propNames)
                    {
                        string defaultValue = PropUtility.GetPropValue(defaultData, fieldName).ToString();
                        string currentValue = PropUtility.GetPropValue(currentData, fieldName).ToString();

                        if (defaultValue != currentValue)
                        {
                            XmlUtility.AddChildElement(xmlDoc, valueElement, fieldName, currentValue);
                        }
                    }

                    if (!currentData.RepairIngredients.NullOrEmpty() && !currentData.RepairIngredients.NullOrEmpty())
                    {
                        XmlElement repairIngredientsElement = xmlDoc.CreateElement("RepairIngredients");
                        foreach (var ingredient in currentData.RepairIngredients)
                        {
                            XmlUtility.AddChildElement(xmlDoc, repairIngredientsElement, ingredient.thingDef.defName, ingredient.count.ToString());
                        }
                        valueElement.AppendChild(repairIngredientsElement);
                    }
                }

                root.AppendChild(XmlUtility.PatchAddCompRoot(xmlDoc, targetDef.defName));
                XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_ArmorDurability).ToString());

            }
        }
    }
}
