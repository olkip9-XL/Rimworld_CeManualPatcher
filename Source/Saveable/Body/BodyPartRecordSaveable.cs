using CeManualPatcher.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Body
{
    internal class BodyPartRecordSaveable : SaveableBase<BodyDef>
    {
        //字段
        private BodyPartHeight height;

        private BodyPartDepth depth;

        private float coverage = 1f;

        private string customLabel;

        private bool coverByArmor = false;

        private BodyDef body => base.def as BodyDef;

        public List<int> path = new List<int>();
        private BodyPartRecord originalData = null;
        private bool originalDataCoverByArmor = false;
        public BodyPartRecord OriginalData => originalData;
        public bool OriginalDataCoverByArmor => originalDataCoverByArmor;

        public BodyPartRecord bodyPart
        {
            get
            {
                BodyPartRecord curPart = null;
                if (body == null || body.corePart == null)
                {
                    return null;
                }

                curPart = body.corePart;

                foreach (var index in path)
                {
                    if (curPart.parts == null || curPart.parts.Count <= index)
                    {
                        Log.Error($"[CE Manual Patcher] BodyPartRecordSaveable path error: {index} {curPart.parts.Count}");
                        return null;
                    }

                    curPart = curPart.parts[index];
                }

                return curPart;
            }
        }

        public BodyPartRecordSaveable() { }
        public BodyPartRecordSaveable(BodyDef bodyDef, List<int> path)
        {
            this.def = bodyDef;
            if (NullCheck())
            {
                return;
            }
            
            this.path.Clear();
            this.path.AddRange(path);

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && bodyPart != null)
            {
                height = bodyPart.height;
                depth = bodyPart.depth;
                coverage = bodyPart.coverage;
                customLabel = bodyPart.customLabel;
                coverByArmor = GetCoverByArmor(bodyPart);
            }

            Scribe_Values.Look(ref height, "height", BodyPartHeight.Undefined);
            Scribe_Values.Look(ref depth, "depth", BodyPartDepth.Undefined);
            Scribe_Values.Look(ref coverage, "coverage", 1f);
            Scribe_Values.Look(ref customLabel, "customLabel", null);
            Scribe_Values.Look(ref coverByArmor, "coverByArmor", false);
            Scribe_Collections.Look(ref path, "path", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (path == null)
                {
                    path = new List<int>();
                }
            }

        }

        public override void Reset()
        {
            if (originalData == null || bodyPart == null)
                return;

            //PropUtility.CopyPropValue(originalData, bodyPart);

            bodyPart.height = originalData.height;
            bodyPart.depth = originalData.depth;
            bodyPart.coverage = originalData.coverage;
            bodyPart.customLabel = originalData.customLabel;

            SetCoverByArmor(bodyPart, originalDataCoverByArmor);
        }

        protected override void Apply()
        {
            bodyPart.height = height;
            bodyPart.depth = depth;
            bodyPart.coverage = coverage;
            bodyPart.customLabel = customLabel;

            SetCoverByArmor(bodyPart, coverByArmor);
        }

        protected override void InitOriginalData()
        {
            if (bodyPart == null)
                return;

            originalData = new BodyPartRecord();
            //PropUtility.CopyPropValue(bodyPart, originalData);

            originalData.height = bodyPart.height;
            originalData.depth = bodyPart.depth;
            originalData.coverage = bodyPart.coverage;
            originalData.customLabel = bodyPart.customLabel;

            originalDataCoverByArmor = GetCoverByArmor(bodyPart);
        }

        public static void SetCoverByArmor(BodyPartRecord record, bool cover)
        {
            if (record == null)
                return;

            if (record.groups == null)
                return;

            BodyPartGroupDef groupDef = DefDatabase<BodyPartGroupDef>.GetNamed("CoveredByNaturalArmor");

            if (record.groups.Any(x => x == groupDef))
            {
                if (!cover)
                {
                    record.groups.Remove(groupDef);
                }
            }
            else
            {
                if (cover)
                {
                    record.groups.Add(groupDef);
                }
            }
        }

        public static bool GetCoverByArmor(BodyPartRecord record)
        {
            if (record == null)
                return false;

            if (record.groups == null)
                return false;

            BodyPartGroupDef groupDef = DefDatabase<BodyPartGroupDef>.GetNamed("CoveredByNaturalArmor");

            return record.groups.Any(x => x == groupDef);
        }

    }
}
