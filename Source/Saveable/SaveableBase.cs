using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal abstract class SaveableBase<T> : IExposable where T : Def
    {
        protected T def;
        public SaveableBase() { }
        public SaveableBase(T def)
        {
            this.def = def;
            if (NullCheck())
            {
                return;
            }

            InitOriginalData();
        }
        protected abstract void Apply();
        public abstract void Reset();
        public abstract void ExposeData();
        public virtual void PostLoadInit(T def)
        {
            this.def = def;
            if (NullCheck())
            {
                return;
            }

            InitOriginalData();
            this.Apply();
        }

        protected abstract void InitOriginalData();

        protected virtual bool NullCheck()
        {
            return def == null;
        }

    }
}
