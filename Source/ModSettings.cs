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
        internal CEPatchManager patchManager = new CEPatchManager();
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref ammoManager, "ammoManager");
            Scribe_Deep.Look(ref weaponManager, "weaponManager");
            Scribe_Deep.Look(ref patchManager, "patchManager");

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
                if (patchManager == null)
                {
                    patchManager = new CEPatchManager();
                }

            }
        }

        public void PostLoad()
        {
            //init
            patchManager?.PostLoadInit();

            ammoManager?.PostLoadInit();
            weaponManager?.PostLoadInit();
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
            inRect.y += 20f;
            inRect.height -= 20f;

            List<TabRecord> tabRecords = new List<TabRecord>();

            foreach (MP_SettingTab tab in Enum.GetValues(typeof(MP_SettingTab)))
            {
                //skip for now
                if(tab == MP_SettingTab.Bionic)
                {
                    continue;
                }

                tabRecords.Add(new TabRecord(tab.GetLabel(), delegate
                {
                    this.curTab = tab;
                }, this.curTab == tab));
            }

            TabDrawer.DrawTabs<TabRecord>(inRect, tabRecords, 200f);

            inRect.y += 10f;
            inRect.height -= 10f;

            switch (curTab)
            {
                case MP_SettingTab.Weapon:
                    settings.weaponManager.DoWindowContents(inRect);
                    break;
                case MP_SettingTab.Ammo:
                    settings.ammoManager.DoWindowContents(inRect);
                    break;
                case MP_SettingTab.Bionic:
                    break;
            }

        }
    

        private void DebugLogAllButtonImage(Rect rect)
        {
            float width = 30f;

            float curY = rect.y;
            float curX = rect.x;

            //TexButton tex = new TexButton();

            //显示TexButton类下的所有按钮
            foreach (var field in typeof(Widgets).GetFields())
            {
                if (field.FieldType == typeof(Texture2D))
                {
                    Rect buttonRect = new Rect(curX, curY, width, width);
                    Widgets.ButtonImage(buttonRect, (Texture2D)field.GetValue(null));

                    TooltipHandler.TipRegion(buttonRect, field.Name);

                    curX += width + 5f;
                    if (curX > rect.xMax - width)
                    {
                        curX = rect.x;
                        curY += width + 5f;
                    }
                }
            }
        }
    }
}
