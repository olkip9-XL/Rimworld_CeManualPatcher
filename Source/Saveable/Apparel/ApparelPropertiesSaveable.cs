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
    internal class ApparelPropertiesSaveable : SaveableBase<ThingDef>
    {
        List<BodyPartGroupDef> originalBodyPartGroups = new List<BodyPartGroupDef>();
        List<ApparelLayerDef> originalLayers = new List<ApparelLayerDef>();

        List<string> bodyPartGroupsSave = new List<string>();
        List<string> layersSave = new List<string>();

        public bool BodyPartGroupsChanged => !Apparel.bodyPartGroups.ToHashSet().SetEquals(originalBodyPartGroups);
        public bool LayersChanged => !Apparel.layers.ToHashSet().SetEquals(originalLayers);

        public ApparelProperties Apparel => def.apparel;

        public ApparelPropertiesSaveable() { }

        public ApparelPropertiesSaveable(ThingDef def) : base(def)
        {
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (def.apparel != null)
                {
                    bodyPartGroupsSave = new List<string>();
                    foreach (var item in def.apparel.bodyPartGroups)
                    {
                        bodyPartGroupsSave.Add(item.defName);
                    }

                    layersSave = new List<string>();
                    foreach (var item in def.apparel.layers)
                    {
                        layersSave.Add(item.defName);
                    }
                }
            }

            Scribe_Collections.Look(ref bodyPartGroupsSave, "bodyPartGroupsSave", LookMode.Value);
            Scribe_Collections.Look(ref layersSave, "layersSave", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (bodyPartGroupsSave == null)
                {
                    bodyPartGroupsSave = new List<string>();
                }

                if (layersSave == null)
                {
                    layersSave = new List<string>();
                }
            }
        }

        public override void Reset()
        {
            if (def.apparel == null)
            {
                return;
            }
            Apparel.bodyPartGroups = originalBodyPartGroups;
            Apparel.layers = originalLayers;
        }

        protected override void Apply()
        {
            if (def.apparel == null)
            {
                return;
            }

            Apparel.bodyPartGroups = new List<BodyPartGroupDef>();
            foreach (var item in bodyPartGroupsSave)
            {
                BodyPartGroupDef itemDef = DefDatabase<BodyPartGroupDef>.GetNamed(item, false);
                if (itemDef != null)
                {
                    Apparel.bodyPartGroups.Add(itemDef);
                }
            }

            Apparel.layers = new List<ApparelLayerDef>();
            foreach (var item in layersSave)
            {
                ApparelLayerDef itemDef = DefDatabase<ApparelLayerDef>.GetNamed(item, false);
                if (itemDef != null)
                {
                    Apparel.layers.Add(itemDef);
                }
            }
        }

        protected override void InitOriginalData()
        {
            if (def.apparel == null)
            {
                return;
            }

            originalBodyPartGroups = new List<BodyPartGroupDef>(def.apparel.bodyPartGroups);
            originalLayers = new List<ApparelLayerDef>(def.apparel.layers);
        }
    }
}
