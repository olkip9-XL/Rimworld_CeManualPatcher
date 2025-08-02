using CeManualPatcher.Misc;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    class ThingDefCountClassExpo : IExposable
    {
        public string defName;
        public int count;
        public void ExposeData()
        {
            Scribe_Values.Look(ref defName, "defName");
            Scribe_Values.Look(ref count, "count");
        }
    }

    internal class ThingDefCountClassSaveable : SaveableBase<ThingDef>
    {

        private List<ThingDefCountClassExpo> fragmentsExpo = new List<ThingDefCountClassExpo>();

        //private
        private List<ThingDefCountClass> originalData = new List<ThingDefCountClass>();
        private CompProperties_Fragments compProps
        {
            get
            {
                if (def == null)
                {
                    return null;
                }
                if (!def.HasComp<CompFragments>())
                {
                    return null;
                }
                return def.GetCompProperties<CompProperties_Fragments>();
            }
        }
        public ThingDefCountClassSaveable() { }
        public ThingDefCountClassSaveable(ThingDef thingDef, bool isOriginal = false)
        {
            this.def = thingDef;
            if (compProps == null)
            {
                return;
            }

            InitOriginalData();
        }

        protected override void Apply()
        {
            if (compProps == null)
            {
                return;
            }

            compProps.fragments.Clear();
            foreach (var fragment in fragmentsExpo)
            {
                compProps.fragments.Add(new ThingDefCountClass()
                {
                    thingDef = DefDatabase<ThingDef>.GetNamed(fragment.defName, false),
                    count = fragment.count
                });
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && compProps != null)
            {
                fragmentsExpo.Clear();
                foreach (var fragment in compProps.fragments)
                {
                    fragmentsExpo.Add(new ThingDefCountClassExpo()
                    {
                        defName = fragment.thingDef.defName,
                        count = fragment.count
                    });
                }
            }

            Scribe_Collections.Look(ref fragmentsExpo, "fragments", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && fragmentsExpo == null)
            {
                fragmentsExpo = new List<ThingDefCountClassExpo>();
            }
        }

        public override void Reset()
        {
            if (compProps == null)
            {
                return;
            }

            compProps.fragments.Clear();
            compProps.fragments.AddRange(originalData);
            InitOriginalData();
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            //if (compProps == null)
            //{
            //    return;
            //}

            if (def == null)
            {
                return;
            }

            if (!def.HasComp<CompFragments>())
            {
                def.comps.Add(new CompProperties_Fragments());
            }

            InitOriginalData();
            this.Apply();
        }

        protected override void InitOriginalData()
        {
            if (compProps == null)
            {
                return;
            }
            originalData.Clear();
            foreach (var fragment in compProps.fragments)
            {
                ThingDefCountClass temp = new ThingDefCountClass();

                PropUtility.CopyPropValue(fragment, temp);

                originalData.Add(temp);
            }
        }

        public bool CompareOriginalData()
        {
            List<ThingDefCountClass> currentData = this.compProps?.fragments ?? null;

            if ((currentData?.Count ?? -1) != (originalData?.Count ?? -1))
            {
                return false;
            }

            if(currentData == null || originalData == null)
            {
                return true;
            }

            currentData.SortBy(x=>x.thingDef.defName);
            originalData.SortBy(x => x.thingDef.defName);

            for (int i = 0; i < currentData.Count; i++)
            {
                var currentFragment = currentData[i];
                var originalFragment = originalData[i];
                if (currentFragment.thingDef != originalFragment.thingDef || currentFragment.count != originalFragment.count)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
