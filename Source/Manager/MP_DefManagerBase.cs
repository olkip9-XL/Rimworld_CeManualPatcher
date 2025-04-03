﻿using CeManualPatcher.Patch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    internal abstract class MP_DefManagerBase : IExposable
    {
        protected bool needApply = false;
        public abstract void ExposeData();

        public abstract void Apply(ThingDef thing);

        public abstract void ApplyAll();

        public abstract void Reset(ThingDef thing);

        public abstract void ResetAll();

        public abstract void DoWindowContents(Rect rect);

        public abstract void PostLoadInit();
    }
}
