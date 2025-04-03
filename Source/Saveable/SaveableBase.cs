using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal abstract class SaveableBase : IExposable
    {
        protected SaveableBase originalData;

        public SaveableBase() { }
        public SaveableBase(ThingDef thingDef, bool isOriginal = false)
        {

        }
        public abstract void Apply(ThingDef thingDef);
        public abstract void Reset(ThingDef thingDef);
        public abstract void ExposeData();
        public abstract void PostLoadInit(ThingDef thingDef);
    }
}
