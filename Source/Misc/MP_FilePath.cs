using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    public static class MP_FilePath
    {

        private static string localModDirInt = null;
        public static string localModDir
        {
            get
            {
                if (localModDirInt == null)
                {
                    ModContentPack mod = LoadedModManager.RunningMods.FirstOrDefault(x => x.PackageId == "LotusLand.CEManualPatcher".ToLower());

                    if (mod == null)
                    {
                        throw new Exception($"[CE Manual Patcher] Cannot find mod directory for CEManualPatcher");
                    }

                    localModDirInt = mod?.RootDir;
                }
                return localModDirInt;
            }
        }


    }
}
