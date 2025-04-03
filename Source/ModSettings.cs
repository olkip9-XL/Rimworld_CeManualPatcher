using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.RenderRect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher
{
    public enum MP_SettingTab
    {
        Weapon,
        Ammo,
        Bionic
    }

    public class ModSetting_CEManualPatcher : ModSettings
    {

        //manager
        internal AmmoManager ammoManager = new AmmoManager();
        internal WeaponManager weaponManager = new WeaponManager();

        //CEPatcher
        internal List<CEPatcher> patchers = new List<CEPatcher>();


        //test
        public TestSaveable testSaveable;

        public override void ExposeData()
        {
            base.ExposeData();

            //Scribe_Deep.Look(ref ammoManager, "ammoManager");
            //Scribe_Deep.Look(ref weaponManager, "weaponManager");
            //Scribe_Collections.Look(ref patchers, "patchers", LookMode.Deep);

            Scribe_Deep.Look(ref testSaveable, "testSaveable");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(ammoManager == null)
                {
                    ammoManager = new AmmoManager();
                }
                if (weaponManager == null)
                {
                    weaponManager = new WeaponManager();
                }
                if (patchers == null)
                {
                    patchers = new List<CEPatcher>();
                }
            }
        }

        public void PostLoad()
        {
            //init
            //ammoManager?.PostLoadInit();
            //weaponManager?.PostLoadInit();
            //patchers?.ForEach(x => x.PostLoadInit());


            ////apply
            //ammoManager?.ApplyAll();
            //weaponManager?.ApplyAll();
            //patchers?.ForEach(x => x.ApplyCEPatch());

            testSaveable.PostLoadInit();
        }

    }

    public class Mod_CEManualPatcher : Mod
    {
        public static ModSetting_CEManualPatcher settings;

        private MP_SettingTab curTab = MP_SettingTab.Weapon;

        public Mod_CEManualPatcher(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_CEManualPatcher>();
        }
        public override string SettingsCategory()
        {
            return "CE Manual Patcher";
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            //inRect.height -= 20f;
            inRect.y += 20f;
            inRect.height -= 20f;

            List<TabRecord> tabRecords = new List<TabRecord>();

            foreach (MP_SettingTab tab in Enum.GetValues(typeof(MP_SettingTab)))
            {
                tabRecords.Add(new TabRecord(tab.GetLabel(), delegate
                {
                    this.curTab = tab;
                }, this.curTab == tab));
            }

            TabDrawer.DrawTabs<TabRecord>(inRect, tabRecords, 200f);

            inRect.y += 10f;
            inRect.height -= 10f;

            //switch (curTab)
            //{
            //    case MP_SettingTab.Weapon:
            //        settings.weaponManager.DoWindowContents(inRect);
            //        break;
            //    case MP_SettingTab.Ammo:
            //        settings.ammoManager.DoWindowContents(inRect);
            //        break;
            //    case MP_SettingTab.Bionic:
            //        break;
            //}

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            if (settings.testSaveable == null)
            {
                ThingDef weaponDef = DefDatabase<ThingDef>.GetNamed("Gun_AssaultRifle");
                settings.testSaveable = new TestSaveable(weaponDef);
            }

            foreach (var item in settings.testSaveable.verbWarppers)
            {
                listing.TestField(item);
            }

            listing.End();
        }
    
    }
}
