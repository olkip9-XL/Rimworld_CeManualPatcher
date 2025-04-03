using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CeManualPatcher.Extension;
using RimWorld;
using Verse;


namespace CeManualPatcher
{
    [StaticConstructorOnStartup]
    public static class ModPostLoad
    {
        static ModPostLoad()
        {
            Log.Message("CE Manual Patcher: Post-load patching...");

            //init
            Mod_CEManualPatcher.settings.PostLoad();
        }
    }
}
