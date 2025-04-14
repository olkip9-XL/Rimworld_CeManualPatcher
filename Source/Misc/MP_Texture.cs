using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc
{
    [StaticConstructorOnStartup]
    public class MP_Texture
    {
        public static Texture2D Reset = ContentFinder<Texture2D>.Get("CEPatcher/UI/Buttons/Reset");

        public static Texture2D CEPatch = ContentFinder<Texture2D>.Get("CEPatcher/UI/Buttons/CEPatch");
    }
}
