using CeManualPatcher.Misc.CustomAmmoMisc;
using CeManualPatcher.RenderRect;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace CeManualPatcher.Misc.Manager
{
    internal class CustomAmmoManager : IExposable
    {
        public static CustomAmmoManager instance;

        public static CustomAmmoSet curAmmoSet;

        List<CustomAmmoSet> ammoSets = new List<CustomAmmoSet>();

        private Rect_CustomAmmoList rect_CustomAmmoList = new Rect_CustomAmmoList();
        private Rect_CustomAmmoInfo rect_CustomAmmoInfo = new Rect_CustomAmmoInfo();

        public List<CustomAmmoSet> AllSets => ammoSets;

        public CustomAmmoManager()
        {
            instance = this;
        }
        public void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                ammoSets.RemoveAll(x => x == null);

                if (!ammoSets.Any())
                {
                    SaveToLocal();
                }
            }

            Scribe_Collections.Look(ref ammoSets, "ammoSets", LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ammoSets == null)
                {
                    ammoSets = new List<CustomAmmoSet>();
                }
            }
        }

        public void AddAmmoSet(CustomAmmoSet ammoSet)
        {
            ammoSets.Add(ammoSet);
        }

        public void DoWindowContents(Rect rect)
        {
            Rect leftRect = rect.LeftPart(0.3f);
            Rect righRect = rect.RightPart(0.7f);


            try
            {
                rect_CustomAmmoList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                //Log.Error($"[CeManualPatcher] Custom Ammo manager ammo info error: {e}");
                MP_Log.Error("Custom Ammo manager ammo list error", e);
            }

            try
            {
                rect_CustomAmmoInfo.DoWindowContents(righRect);
            }
            catch (Exception e)
            {
                //Log.Error($"[CeManualPatcher] Custom Ammo manager ammo info error: {e}");
                MP_Log.Error("Custom Ammo manager ammo info error", e);
            }

        }

        public void ExportAll()
        {
            string dirPath =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches", "CE", "Defs", "Ammo");

            foreach(var item in ammoSets)
            {
                try
                {
                    item.ExportToFile(dirPath);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Custom Ammo manager error while exporting ammo set {item?.defNameBase ?? "null"} to {dirPath}: {e}");
                }
            }
        }

        public bool SaveToLocal()
        {
            bool result = true;

            //remove ammo dir
            string modAmmoPath = CustomAmmoSet.ModDefDir;
            if (Directory.Exists(modAmmoPath))
            {
                try
                {
                    Directory.Delete(modAmmoPath, true);
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Custom Ammo manager error while deleting ammo dir {modAmmoPath}: {e}");
                    result = false;
                }
            }

            //save
            foreach (var item in ammoSets)
            {
                try
                {
                    item.ExportToFile();
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Custom Ammo manager error while saving ammo set {item?.defNameBase ?? "null"} to local mod dirctory: {e}");
                    result = false;
                }
            }

            return result;

        }

        public void PostLoadInit()
        {
            foreach (var item in ammoSets)
            {
                try
                {
                    item.PostLoadInit();
                }
                catch (Exception e)
                {
                    Log.Error($"[CeManualPatcher] Custom Ammo manager error while loading ammo set {item?.defNameBase ?? "null"}: {e}");

                }
            }
        }
    }
}
