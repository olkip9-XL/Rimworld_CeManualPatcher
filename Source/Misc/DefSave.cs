using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    internal class DefSave<T> where T : Def, new()
    {

        public string name;

        private string defString = "null";
        private T defInt;
        public T def
        {
            get
            {
                if (defInt == null)
                {
                    defInt = DefDatabase<T>.GetNamed(defString);
                }
                return defInt;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref defString, name, "null");
        }

    }
}
