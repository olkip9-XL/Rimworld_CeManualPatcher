using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    struct ThingDefCountClassExpo
    {
        public string defName;
        public int count;
    }

    internal class ThingDefCountClassSaveable : SaveableBase
    {

        private List<ThingDefCountClassExpo> fragmentsExpo = new List<ThingDefCountClassExpo>();

        public List<ThingDefCountClass> fragments = new List<ThingDefCountClass>();
        ThingDefCountClassSaveable Original => base.originalData as ThingDefCountClassSaveable;

        public ThingDefCountClassSaveable() { }

        public ThingDefCountClassSaveable(ThingDef thingDef, bool isOriginal = false)
        {
            if (thingDef == null || !thingDef.HasComp<CompFragments>())
            {
                return;
            }

            CompProperties_Fragments compProps = thingDef.GetCompProperties<CompProperties_Fragments>();

            this.fragments.Clear();
            this.fragments.AddRange(compProps.fragments);

            if (isOriginal)
            {
                originalData = new ThingDefCountClassSaveable(thingDef, true);
            }
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null || !thingDef.HasComp<CompFragments>())
            {
                return;
            }
            CompProperties_Fragments compProps = thingDef.GetCompProperties<CompProperties_Fragments>();
            compProps.fragments.Clear();
            compProps.fragments.AddRange(this.fragments);
        }

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                fragmentsExpo.Clear();
                foreach (var fragment in fragments)
                {
                    fragmentsExpo.Add(new ThingDefCountClassExpo()
                    {
                        defName = fragment.thingDef.defName,
                        count = fragment.count
                    });
                }
            }

            Scribe_Collections.Look(ref fragmentsExpo, "fragments", LookMode.Value);
        }

        public override void Reset(ThingDef thingDef)
        {
            if (Original == null)
            {
                return;
            }

            Original.Apply(thingDef);
        }
    
        public override void PostLoadInit(ThingDef thingDef)
        {
            if(this.fragmentsExpo!= null)
            {
                this.fragments.Clear();
                foreach (var fragment in fragmentsExpo)
                {
                    ThingDef thing = DefDatabase<ThingDef>.GetNamed(fragment.defName, false);
                    if (thing != null)
                    {
                        this.fragments.Add(new ThingDefCountClass(thing, fragment.count));
                    }
                }
            }

            this.originalData = new ThingDefCountClassSaveable(thingDef, true);
        }
    }
}
