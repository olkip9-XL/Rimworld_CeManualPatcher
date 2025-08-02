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
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref statBase, "statBase");
            Scribe_Deep.Look(ref armorDurability, "armorDurability");
        }

        public override void Reset()
        {
            statBase?.Reset();
            armorDurability?.Reset();
        }

        public override void PostLoadInit()
        {
            if (targetDef == null)
            {
                return;
            }

            statBase?.PostLoadInit(targetDef);
            armorDurability?.PostLoadInit(targetDef);
        }

        //public override void ExportPatch(string dirPath)
        //{

        //    string folderPath = Path.Combine(dirPath, targetDef.modContentPack.Name);
        //    folderPath = Path.Combine(folderPath, "Race");
        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }

        //    string filePath = Path.Combine(folderPath, targetDef.defName + ".xml");

        //    XmlElement root = null;
        //    XmlDocument xmlDoc = XmlUtility.CreateBasePatchDoc(ref root, targetDef.modContentPack.Name);

        //    //stats
        //    XmlUtility.Replace_StatBase(xmlDoc, root, targetDef.defName, targetDef.statBases);

        //    //comps
        //    MakeCompPatch_ArmorDurability();

        //    xmlDoc.Save(filePath);

        //    void MakeCompPatch_ArmorDurability()
        //    {
        //        XmlElement valueElement = xmlDoc.CreateElement("li");
        //        valueElement.SetAttribute("Class", typeof(CompProperties_ArmorDurability).ToString());

        //        CompProperties_ArmorDurability defaultData = new CompProperties_ArmorDurability();
        //        CompProperties_ArmorDurability currentData = targetDef.GetCompProperties<CompProperties_ArmorDurability>();

        //        if (defaultData != null && currentData != null)
        //        {
        //            foreach (var fieldName in CompArmorDurabilitySaveable.propNames)
        //            {
        //                string defaultValue = PropUtility.GetPropValue(defaultData, fieldName).ToString();
        //                string currentValue = PropUtility.GetPropValue(currentData, fieldName).ToString();

        //                if (defaultValue != currentValue)
        //                {
        //                    XmlUtility.AddChildElement(xmlDoc, valueElement, fieldName, currentValue);
        //                }
        //            }

        //            if (!currentData.RepairIngredients.NullOrEmpty() && !currentData.RepairIngredients.NullOrEmpty())
        //            {
        //                XmlElement repairIngredientsElement = xmlDoc.CreateElement("RepairIngredients");
        //                foreach (var ingredient in currentData.RepairIngredients)
        //                {
        //                    XmlUtility.AddChildElement(xmlDoc, repairIngredientsElement, ingredient.thingDef.defName, ingredient.count.ToString());
        //                }
        //                valueElement.AppendChild(repairIngredientsElement);
        //            }
        //        }

        //        if (this.armorDurability.CompChanged)
        //        {
        //            root.AppendChild(XmlUtility.PatchAddCompRoot(xmlDoc, targetDef.defName));
        //            XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_ArmorDurability).ToString());
        //        }

        //    }
        //}

        protected override void MakePatch(XmlDocument xmlDoc, XmlElement root)
        {
            XmlUtility.Replace_StatBase(xmlDoc, root, targetDef.defName, targetDef.statBases);

            //comps
            MakeCompPatch_ArmorDurability();

            void MakeCompPatch_ArmorDurability()
            {
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

                if (this.armorDurability.CompChanged)
                {
                    root.AppendChild(XmlUtility.PatchAddCompRoot(xmlDoc, targetDef.defName));
                    XmlUtility.PatchComp(xmlDoc, root, valueElement, targetDef.defName, typeof(CompProperties_ArmorDurability).ToString());
                }

            }
        }
    }
}
