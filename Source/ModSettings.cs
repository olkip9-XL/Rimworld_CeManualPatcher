using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Misc.Manager;
using CeManualPatcher.RenderRect;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
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
        CustomAmmo,
        Bionic,
        Apparel,
        Body,
    }

    public class ModSetting_CEManualPatcher : ModSettings
    {

        //manager
        internal AmmoManager ammoManager = new AmmoManager();
        internal WeaponManager weaponManager = new WeaponManager();
        internal ApparelManager apparelManager = new ApparelManager();
        internal BodyDefManager bodyDefManager = new BodyDefManager();

        internal CustomAmmoManager customAmmoManager = new CustomAmmoManager();

        //CEPatcher
        internal CEPatchManager patchManager = new CEPatchManager();
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref ammoManager, "ammoManager");
            Scribe_Deep.Look(ref weaponManager, "weaponManager");
            Scribe_Deep.Look(ref patchManager, "patchManager");
            Scribe_Deep.Look(ref apparelManager, "apparelManager");
            Scribe_Deep.Look(ref customAmmoManager, "customAmmoManager");
            Scribe_Deep.Look(ref bodyDefManager, "bodyDefManager");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (ammoManager == null)
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
                if (apparelManager == null)
                {
                    apparelManager = new ApparelManager();
                }
                if (customAmmoManager == null)
                {
                    customAmmoManager = new CustomAmmoManager();
                }
                if (bodyDefManager == null)
                {
                    bodyDefManager = new BodyDefManager();
                }
            }
        }

        public void PostLoad()
        {
            //init
            patchManager?.PostLoadInit();
            customAmmoManager?.PostLoadInit();

            ammoManager?.PostLoadInit();
            weaponManager?.PostLoadInit();
            apparelManager?.PostLoadInit();
            bodyDefManager?.PostLoadInit();
        }

        public void ExportPatch()
        {
            XmlUtility.CreateBasicFolders();

            patchManager?.ExportAll();
            apparelManager?.ExportAll();
            customAmmoManager?.ExportAll();
            bodyDefManager?.ExportAll();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches");
            Messages.Message($"MP_CEPatchExportMsg".Translate(path), MessageTypeDefOf.NeutralEvent);
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
                if (tab == MP_SettingTab.Bionic)
                {
                    continue;
                }

                tabRecords.Add(new TabRecord(tab.GetLabel(), delegate
                {
                    this.curTab = tab;
                }, this.curTab == tab));
            }

            TabDrawer.DrawTabs<TabRecord>(inRect, tabRecords, 150f);

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
                case MP_SettingTab.Apparel:
                    settings.apparelManager.DoWindowContents(inRect);
                    break;
                case MP_SettingTab.CustomAmmo:
                    settings.customAmmoManager.DoWindowContents(inRect);
                    break;
                case MP_SettingTab.Body:
                    settings.bodyDefManager.DoWindowContents(inRect);
                    break;
            }

            WidgetsUtility.UtilityTick();
        }

        //test

        private void DebugLogAllButtonImage(Rect rect)
        {
            float width = 30f;

            float curY = rect.y;
            float curX = rect.x;

            //TexButton tex = new TexButton();

            Type type = typeof(TexButton);
            //Type type = typeof(Widgets);

            //显示TexButton类下的所有按钮
            foreach (var field in type.GetFields())
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
