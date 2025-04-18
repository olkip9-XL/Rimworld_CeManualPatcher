using CeManualPatcher.Patch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    internal abstract class MP_DefManagerBase : IExposable
    {
        public static readonly string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches/CE/Patches");
        public abstract void ExposeData();
        public abstract void Reset(ThingDef thing);

        public abstract void ResetAll();

        public abstract void DoWindowContents(Rect rect);

        public abstract void PostLoadInit();
    }
}
