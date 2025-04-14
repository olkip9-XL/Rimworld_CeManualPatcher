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
        protected ThingDef thingDef;
        public SaveableBase() { }
       
        protected abstract void Apply();
        public abstract void Reset();
        public abstract void ExposeData();
        public virtual void PostLoadInit(ThingDef thingDef)
        {
            this.thingDef = thingDef;
        }

        protected abstract void InitOriginalData();
    }
}
