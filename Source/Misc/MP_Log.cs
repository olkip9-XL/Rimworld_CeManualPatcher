using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    internal static class MP_Log
    {
        public static void Error(string message, Exception e = null, Def def = null)
        {
            string errorMessage = $"[CE Manual Patcher] {message}";

            if (def != null)
                errorMessage += $" on Def {def.defName} From {def.modContentPack.Name}";

            if (e != null)
                errorMessage += $" : {e}";

            if (def != null)
            {
                Log.ErrorOnce(errorMessage, def.GetHashCode());
            }
            else
            {
                Log.Error(errorMessage);
            }

        }

        public static void Warning(string message)
        {
            string warningMessage = $"[CE Manual Patcher] {message}";
            
            Log.Warning(warningMessage);
        }

    }
}
