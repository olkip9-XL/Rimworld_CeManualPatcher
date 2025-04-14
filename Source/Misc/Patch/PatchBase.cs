using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    public abstract class PatchBase : IExposable
    {
        //public abstract void Apply();
        public abstract void Reset();
        public abstract void PostLoadInit();   
        public abstract void ExposeData();
    }
}
