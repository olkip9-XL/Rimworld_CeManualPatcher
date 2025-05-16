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

namespace CeManualPatcher.Saveable.Apparel
{
    internal class PartialArmorExtItem : SaveableBase<ThingDef>
    {
        public static ReadOnlyCollection<string> propNames = new ReadOnlyCollection<string>(new List<string>()
        {
            "mult",
            "staticValue",
            "useStatic",
        });
        private Dictionary<string, string> propDic = new Dictionary<string, string>();

        private string statDefName;
        public StatDef statDef
        {
            get => DefDatabase<StatDef>.GetNamed(statDefName, false);
            set => statDefName = value?.defName ?? "Null";
        }

        List<string> bodyPartsDefString = new List<string>();

        ApparelPartialStat stat;

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

        public PartialArmorExtItem() { }
        public PartialArmorExtItem(ApparelPartialStat stat)
        {
            this.stat = stat;
            if (stat == null)
            {
                return;
            }
        }
        public override void ExposeData()
        {

            if (Scribe.mode == LoadSaveMode.Saving && stat != null)
            {
                propDic.Clear();
                foreach (var propName in propNames)
                {
                    propDic[propName] = PropUtility.GetPropValue(stat, propName).ToString();
                }

                this.statDef = stat.stat;
                foreach (var item in stat.parts)
                {
                    bodyPartsDefString.Add(item.defName);
                }
            }

            Scribe_Values.Look(ref statDefName, "stat");
            Scribe_Collections.Look(ref bodyPartsDefString, "parts", LookMode.Value);
            Scribe_Collections.Look(ref propDic, "props", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (bodyPartsDefString == null)
                {
                    bodyPartsDefString = new List<string>();
                }
                if (propDic == null)
                {
                    propDic = new Dictionary<string, string>();
                }
            }

        }
        public override void Reset()
        {
            //Do nothing
        }
        protected override void Apply()
        {
            if(partialArmorExt == null)
            {
                return;
            }

            ApparelPartialStat statReady = new ApparelPartialStat()
            {
                stat = this.statDef,
                parts = new List<BodyPartDef>(),
            };
    

            foreach(var item in propNames)
            {
                if (propDic.ContainsKey(item))
                {
                    PropUtility.SetPropValueString(statReady, item, propDic[item]);
                }
            }

            foreach (var item in bodyPartsDefString)
            {
                if (DefDatabase<BodyPartDef>.GetNamed(item, false) is BodyPartDef bodyPartDef)
                {
                    statReady.parts.Add(bodyPartDef);
                }
            }

            partialArmorExt.stats.Add(statReady);
        }
        protected override void InitOriginalData()
        {
            //Do nothing
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            if(partialArmorExt == null)
            {
                return;
            }

            this.Apply();
        }
    }
}
