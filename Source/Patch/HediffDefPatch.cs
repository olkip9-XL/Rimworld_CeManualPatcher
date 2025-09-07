using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Patch
{
    internal class HediffDefPatch : PatchBase<HediffDef>
    {
        public override string PatchName => "Hediff";

        //字段
        private HediffDefSaveable saveData;
        private HediffComp_VerbGiverSaveable verbGiverSaveable;


        public HediffDefPatch() { }
        public HediffDefPatch(HediffDef hediffDef)
        {
            if (hediffDef == null)
            {
                return;
            }

            targetDef = hediffDef;

            saveData = new HediffDefSaveable(hediffDef);

            if (hediffDef.HasComp(typeof(HediffComp_VerbGiver)))
            {
                verbGiverSaveable = new HediffComp_VerbGiverSaveable(hediffDef);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref saveData, "saveData");
            Scribe_Deep.Look(ref verbGiverSaveable, "verbGiver");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (saveData == null)
                {
                    saveData = new HediffDefSaveable();
                }
            }
        }

        public override void PostLoadInit()
        {
            try
            {
                saveData?.PostLoadInit(targetDef);
                verbGiverSaveable?.PostLoadInit(targetDef);
            }
            catch (Exception e)
            {
                Log.Error($"[CE Manual Patcher] HediffPatch PostLoadInit error: {e}");
            }
        }

        public override void Reset()
        {
            try
            {
                saveData?.Reset();
                verbGiverSaveable?.Reset();
            }
            catch (Exception e)
            {
                Log.Error($"[CE Manual Patcher] HediffPatch Reset error: {e}");
            }
        }

        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            //stages
            if (saveData.Changed)
            {
                string xpathBase = $"Defs/HediffDef[defName=\"{targetDef.defName}\"]/stages";

                for (int i = 0; i < targetDef.stages.Count; i++)
                {
                    HediffStageExpo curStage = new HediffStageExpo(targetDef.stages[i]);
                    HediffStageExpo originalStage = saveData.OriginalData[i];

                    if (curStage.severityGainFactor != originalStage.severityGainFactor)
                        root.AppendChild(XmlUtility.PatchReplaceValue(xmlDoc, $"{xpathBase}/li[{i + 1}]", "severityGainFactor", curStage.severityGainFactor.ToString("F4")));

                    if (curStage.minSeverity != originalStage.minSeverity)
                        root.AppendChild(XmlUtility.PatchReplaceValue(xmlDoc, $"{xpathBase}/li[{i + 1}]", "minSeverity", curStage.minSeverity.ToString("F4")));

                    //offset
                    string xpath_offset = $"{xpathBase}/li[{i + 1}]/statOffsets";

                    root.AppendChild(XmlUtility.PatchConditional(xmlDoc, xpath_offset, null, XmlUtility.PatchAdd(xmlDoc, $"{xpathBase}/li[{i + 1}]", xmlDoc.CreateElement("statOffsets"), "nomatch")));
                    
                    XmlElement valueElement_offset = xmlDoc.CreateElement("statOffsets");

                    foreach(var offset in curStage.offsetDic)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement_offset, offset.Key.defName, offset.Value.ToString("F4"));
                    }

                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, xpath_offset, valueElement_offset));

                    //factor
                    string xpath_factor = $"{xpathBase}/li[{i + 1}]/statFactors";
                    root.AppendChild(XmlUtility.PatchConditional(xmlDoc, xpath_factor, null, XmlUtility.PatchAdd(xmlDoc, $"{xpathBase}/li[{i + 1}]", xmlDoc.CreateElement("statFactors"), "nomatch")));

                    XmlElement valueElement_factor = xmlDoc.CreateElement("statFactors");
                    foreach (var factor in curStage.factorDic)
                    {
                        XmlUtility.AddChildElement(xmlDoc, valueElement_factor, factor.Key.defName, factor.Value.ToString("F4"));
                    }
                    root.AppendChild(XmlUtility.PatchReplace(xmlDoc, xpath_factor, valueElement_factor));
                }


            }

            //verbGiver
            if (verbGiverSaveable != null && verbGiverSaveable.Changed)
            {
                string xpath = $"Defs/HediffDef[defName=\"{targetDef.defName}\"]/comps/li[@Class=\"HediffCompProperties_VerbGiver\"]/tools";

                HediffCompProperties_VerbGiver verbGiver = targetDef.comps.FirstOrDefault(x => x is HediffCompProperties_VerbGiver) as HediffCompProperties_VerbGiver;

                if (verbGiver != null && verbGiver.tools != null)
                {
                    for (int i = 0; i < verbGiver.tools.Count; i++)
                    {
                        ToolCE tool = verbGiver.tools[i] as ToolCE;
                        HediffToolExpo originalTool = verbGiverSaveable.OriginalData[i];
                        if (tool == null)
                        {
                            continue;
                        }

                        //set attribute
                        root.AppendChild(XmlUtility.PatchSetAttribute(xmlDoc, xpath + $"/li[{i + 1}]", "Class", "CombatExtended.ToolCE"));

                        if (tool.armorPenetrationSharp != originalTool.armorPenetrationSharp)
                            root.AppendChild(XmlUtility.PatchReplaceValue(xmlDoc, xpath + $"/li[{i + 1}]", "armorPenetrationSharp", tool.armorPenetrationSharp.ToString()));
                        if (tool.armorPenetrationBlunt != originalTool.armorPenetrationBlunt)
                            root.AppendChild(XmlUtility.PatchReplaceValue(xmlDoc, xpath + $"/li[{i + 1}]", "armorPenetrationBlunt", tool.armorPenetrationBlunt.ToString()));
                        if (tool.power != originalTool.power)
                            root.AppendChild(XmlUtility.PatchReplaceValue(xmlDoc, xpath + $"/li[{i + 1}]", "power", tool.power.ToString()));
                    }
                }
            }
        }

    }
}
