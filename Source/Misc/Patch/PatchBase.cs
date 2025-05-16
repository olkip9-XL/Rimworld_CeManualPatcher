using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc.Patch
{
    public abstract class PatchBase<T> : IExposable where T : Def
    {
        protected string targetDefString = "null";
        private T targetDefInt;
        public T targetDef
        {
            get
            {
                if (targetDefInt == null)
                {
                    targetDefInt = DefDatabase<T>.GetNamed(targetDefString, false);
                }
                return targetDefInt;
            }
            set
            {
                targetDefString = value?.defName ?? "Null";
                targetDefInt = value;
            }


        }
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref targetDefString, "def", "Null");
        }
        public abstract void Reset();
        public abstract void PostLoadInit();
        public abstract void ExportPatch(string dirPath);
    }
}
