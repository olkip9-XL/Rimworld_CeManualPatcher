using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    internal class DefSaveable<T> : IExposable where T : Def
    {
        string defString;
        public T ToDef()
        {
            T def = DefDatabase<T>.GetNamedSilentFail(defString);
            return def;
        }

        public DefSaveable()
        {
            defString = "null";
        }

        public DefSaveable(T def)
        {
            defString = def?.defName ?? "null";
        }


        public void ExposeData()
        {
            Scribe_Values.Look(ref defString, "defString");
        }
    }
}
