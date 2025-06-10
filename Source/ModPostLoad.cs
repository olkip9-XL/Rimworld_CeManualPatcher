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
            //init
            Mod_CEManualPatcher.settings.PostLoad();

            //log All mod id
            //foreach(var item in LoadedModManager.RunningMods)
            //{
            //    Log.Warning($"Mod ID: {item.PackageId} - Name: {item.Name}");
            //}
        }
    }
}
