using CeManualPatcher.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Comps
{
    internal abstract class CompSaveableBase<TComp> : SaveableBase<ThingDef> where TComp : CompProperties
    {
        protected abstract TComp compProps { get; }

        public CompSaveableBase() { }

        public CompSaveableBase(ThingDef thingDef) : base(thingDef)
        {
        }

    }
}
