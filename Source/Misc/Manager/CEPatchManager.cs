using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Manager;
using RimWorld;
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
    internal class CEPatchManager : IExposable
    {
        public static CEPatchManager instance { get; private set; }

        private List<CEPatcher> patchers = new List<CEPatcher>();
        private WeaponManager weaponManager => WeaponManager.instance;
        public CEPatchManager()
        {
            instance = this;
        }

        public bool HasPatcher(ThingDef thingDef)
        {
            return patchers.Any(x => x?.thingDef == thingDef);
        }

        public void AddPatcher(CEPatcher patcher)
        {
            if (HasPatcher(patcher.thingDef))
            {
                Log.Warning($"Patcher for {patcher.thingDef} already exists");
                return;
            }

            patchers.Add(patcher);
        }

        public CEPatcher GetPatcher(ThingDef thingDef)
        {
            return patchers.FirstOrDefault(x => x?.thingDef == thingDef);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref patchers, "patchers", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (patchers == null)
                {
                    patchers = new List<CEPatcher>();
                }
            }
        }

        public void Reset(ThingDef thing)
        {
            if (thing == null)
            {
                return;
            }

            CEPatcher patcher = GetPatcher(thing);
            if (patcher == null)
            {
                return;
            }

            patchers.Remove(patcher);
        }

        public void ResetAll()
        {
            this.patchers.Clear();
        }

     
        public void PostLoadInit()
        {
            patchers?.ForEach(x => x?.PostLoadInit());
        }

        public void ExportAll()
        {
            foreach (var item in this.patchers)
            {
                try
                {
                    item?.ExportToFile(MP_DefManagerBase<Def>.exportPath);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Exporting weapon patch {item?.thingDef?.defName} failed : {e}");
                }
            }
        }

    }
}
