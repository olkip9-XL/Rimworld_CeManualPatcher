using CeManualPatcher.Misc;
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
    internal abstract class MP_DefManagerBase<T> : IExposable where T : Def
    {
        protected List<PatchBase<T>> patches = new List<PatchBase<T>>();

        public static readonly string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches/CE/Patches");

        public abstract void DoWindowContents(Rect rect);

        protected abstract void NewPatch(ref PatchBase<T> patch, T def);


        public virtual PatchBase<T> GetPatch(T def)
        {
            PatchBase<T> patch = patches.FirstOrDefault(x => x?.targetDef == def);
            if (patch == null)
            {
                try
                {
                    NewPatch(ref patch, def);
                    this.patches.Add(patch);
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Trying create {typeof(T).Name} patch for item {def?.defName ?? "Null"}", e);
                }
            }
            return patch;
        }

        public virtual void ExposeData()
        {
            Scribe_Collections.Look(ref patches, "patches", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (patches == null)
                {
                    patches = new List<PatchBase<T>>();
                }
            }
        }
        public virtual void Reset(T thing)
        {
            PatchBase<T> patch = patches.FirstOrDefault(x => x?.targetDef == thing);
            if (patch != null)
            {
                try
                {
                    patch.Reset();
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Resetting {typeof(T).Name} patch {patch?.targetDef?.defName} failed", e);
                }
                patches.Remove(patch);
            }

            WidgetsUtility.ResetTextFieldBuffer();
        }

        public virtual void ResetAll()
        {
            foreach (var patch in patches)
            {
                if (patch == null || patch.targetDef == null)
                {
                    MP_Log.Warning($"{typeof(T).Name} patch is null or targetDef is null, skipping reset.");
                    continue;
                }

                try
                {
                    patch.Reset();
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Resetting {typeof(T).Name} patch {patch?.targetDef?.defName} failed", e);
                }
            }
            patches.Clear();
            WidgetsUtility.ResetTextFieldBuffer();
        }


        public virtual void PostLoadInit()
        {
            foreach (var patch in patches)
            {
                try
                {
                    patch?.PostLoadInit();
                }
                catch (Exception e)
                {
                    MP_Log.Error($"PostLoadInit {typeof(T).Name} patch {patch?.targetDef?.defName} failed", e);
                }
            }
        }

        public virtual void ExportAll()
        {
            foreach (var patch in patches)
            {
                try
                {
                    patch?.ExportPatch(exportPath);
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Exporting {typeof(T).Name} patch {patch?.targetDef?.defName} failed", e);
                }
            }
        }
    }
}
