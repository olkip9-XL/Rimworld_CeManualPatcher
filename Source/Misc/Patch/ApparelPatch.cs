using CeManualPatcher.Saveable;
using CeManualPatcher.Saveable.Apparel;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Misc.Patch
{
    internal class ApparelPatch : PatchBase
    {
        private string apparelDefString;
        public ThingDef apparelDef
        {
            get => DefDatabase<ThingDef>.GetNamed(apparelDefString, false);
            set => apparelDefString = value?.defName ?? "Null";
        }

        private StatSaveable statSaveable;
        private PartialArmorExtSaveable partialArmorExtSaveable;

        public ApparelPatch() { }
        public ApparelPatch(ThingDef apparelDef)
        {
            if (apparelDef == null)
            {
                return;
            }

            this.apparelDef = apparelDef;

            statSaveable = new StatSaveable(apparelDef);
            partialArmorExtSaveable = new PartialArmorExtSaveable(apparelDef);
        }


        public override void ExposeData()
        {
            Scribe_Values.Look(ref apparelDefString, "apparelDef");
            Scribe_Deep.Look(ref statSaveable, "statBase");
            Scribe_Deep.Look(ref partialArmorExtSaveable, "partialArmorExt");
        }

        public override void PostLoadInit()
        {
            if(apparelDef == null)
            {
                return;
            }

            statSaveable?.PostLoadInit(apparelDef);
            partialArmorExtSaveable?.PostLoadInit(apparelDef);
        }

        public override void Reset()
        {
            statSaveable?.Reset();
            partialArmorExtSaveable?.Reset();
        }

        public void ExportPatch(string dirPath)
        {
            string folderPath = Path.Combine(dirPath, apparelDef.modContentPack.Name);
            folderPath = Path.Combine(folderPath, "Apparel");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, apparelDef.defName + ".xml");

            // 创建XML文档
            XmlElement rootPatchElement = null; 
            XmlDocument xmlDoc = XMLUtility.CreateBasePatchDoc(ref rootPatchElement);

            XMLUtility.Replace_StatBase(xmlDoc, rootPatchElement, apparelDef.defName, apparelDef.statBases);
            XMLUtility.AddModExt_PartialArmor(xmlDoc, rootPatchElement, apparelDef.defName, apparelDef.GetModExtension<PartialArmorExt>());

            xmlDoc.Save(filePath);
        }
    }
}
