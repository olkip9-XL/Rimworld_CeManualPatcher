using CeManualPatcher.Misc;
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
    internal class CEPatchManager : MP_DefManagerBase
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

        public override void ExposeData()
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

        public override void Reset(ThingDef thing)
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

        public override void ResetAll()
        {
            this.patchers.Clear();
        }

        public override void DoWindowContents(Rect rect)
        {
        }

        public override void PostLoadInit()
        {
            patchers?.ForEach(x => x?.PostLoadInit());
        }

        public void ExportAll()
        {
            foreach (var item in this.patchers)
            {
                //XMLUtility.CreateCEPatch(item.thingDef);
                item?.ExportToFile(MP_DefManagerBase.exportPath);
            }
        }

    }
}
