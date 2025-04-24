using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Manager;
using CeManualPatcher.Saveable;
using CeManualPatcher.Saveable.Apparel;
using CeManualPatcher.Saveable.Weapon;
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

namespace CeManualPatcher.Patch
{
    internal class ApparelPatch : PatchBase<ThingDef>
    {

        private StatSaveable stats;
        private StatOffsetSaveable statOffsets;
        private PartialArmorExtSaveable partialArmorExtSaveable;

        private ApparelManager manager => ApparelManager.instance;

        public ApparelPatch() { }
        public ApparelPatch(ThingDef apparelDef)
        {
            if (apparelDef == null)
            {
                return;
            }

            this.targetDef = apparelDef;

            stats = new StatSaveable(apparelDef);
            statOffsets = new StatOffsetSaveable(apparelDef);
            partialArmorExtSaveable = new PartialArmorExtSaveable(apparelDef);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref apparelDefString, "apparelDef");
            Scribe_Deep.Look(ref stats, "statBase");
            Scribe_Deep.Look(ref statOffsets, "statOffsets");
            Scribe_Deep.Look(ref partialArmorExtSaveable, "partialArmorExt");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string oldDefString = "";
                Scribe_Values.Look(ref oldDefString, "apparelDef");
                if (!oldDefString.NullOrEmpty())
                {
                    targetDefString = oldDefString;
                }
            }
        }

        public override void PostLoadInit()
        {
            if(targetDef == null)
            {
                return;
            }

            stats?.PostLoadInit(targetDef);
            statOffsets?.PostLoadInit(targetDef);
            partialArmorExtSaveable?.PostLoadInit(targetDef);

            if (stats != null)
            {
                stats = new StatSaveable(targetDef);
            }
        }

        public override void Reset()
        {
            stats?.Reset();
            statOffsets?.Reset();
            partialArmorExtSaveable?.Reset();
        }

        public override void ExportPatch(string dirPath)
        {
            string folderPath = Path.Combine(dirPath, targetDef.modContentPack.Name);
            folderPath = Path.Combine(folderPath, "Apparel");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, targetDef.defName + ".xml");

            // 创建XML文档
            XmlElement rootPatchElement = null;                                                                                                                                                                           
            XmlDocument xmlDoc = XMLUtility.CreateBasePatchDoc(ref rootPatchElement);

            XMLUtility.Replace_StatBase(xmlDoc, rootPatchElement, targetDef.defName, targetDef.statBases);
            XMLUtility.AddModExt_PartialArmor(xmlDoc, rootPatchElement, targetDef.defName, targetDef.GetModExtension<PartialArmorExt>());
            XMLUtility.Replace_StatOffsets(xmlDoc, rootPatchElement, targetDef.defName, targetDef.equippedStatOffsets);

            xmlDoc.Save(filePath);
        }
    }
}
