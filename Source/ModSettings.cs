using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
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
        Race,
        Hediff
    }

    public class ModSetting_CEManualPatcher : ModSettings
    {
        //managers
        internal AmmoManager ammoManager = new AmmoManager();
        internal WeaponManager weaponManager = new WeaponManager();
        internal ApparelManager apparelManager = new ApparelManager();
        internal BodyDefManager bodyDefManager = new BodyDefManager();
        internal RaceManager raceManager = new RaceManager();
        internal HediffDefManager hediffDefManager = new HediffDefManager();
        internal CustomAmmoManager customAmmoManager = new CustomAmmoManager();

        private Dictionary<MP_SettingTab, IManager> tabManagers;
        private List<IManager> allManagers;

        public ModSetting_CEManualPatcher()
        {
            InitManagers();
        }

        private void InitManagers()
        {
            tabManagers = new Dictionary<MP_SettingTab, IManager>
            {
                { MP_SettingTab.Weapon, weaponManager },
                { MP_SettingTab.Ammo, ammoManager },
                { MP_SettingTab.Apparel, apparelManager },
                { MP_SettingTab.CustomAmmo, customAmmoManager },
                { MP_SettingTab.Body, bodyDefManager },
                { MP_SettingTab.Race, raceManager },
                { MP_SettingTab.Hediff, hediffDefManager }
            };

            allManagers = tabManagers.Values.ToList();
        }

        public IManager GetManager(MP_SettingTab tab)
        {
            return tabManagers.TryGetValue(tab, out var manager) ? manager : null;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref ammoManager, "ammoManager");
            Scribe_Deep.Look(ref weaponManager, "weaponManager");
            Scribe_Deep.Look(ref apparelManager, "apparelManager");
            Scribe_Deep.Look(ref customAmmoManager, "customAmmoManager");
            Scribe_Deep.Look(ref bodyDefManager, "bodyDefManager");
            Scribe_Deep.Look(ref raceManager, "raceManager");
            Scribe_Deep.Look(ref hediffDefManager, "hediffDefManager");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                ammoManager ??= new AmmoManager();
                weaponManager ??= new WeaponManager();
                apparelManager ??= new ApparelManager();
                customAmmoManager ??= new CustomAmmoManager();
                bodyDefManager ??= new BodyDefManager();
                raceManager ??= new RaceManager();
                hediffDefManager ??= new HediffDefManager();

                InitManagers();
            }
        }

        public void PostLoad()
        {
            foreach (var manager in allManagers)
            {
                manager?.PostLoadInit();
            }
        }

        public void ExportPatch()
        {
            XmlUtility.CreateBasicFolders();

            foreach (var manager in allManagers)
            {
                manager?.ExportAll();
            }

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CE Patches");
            Messages.Message($"MP_CEPatchExportMsg".Translate(path), MessageTypeDefOf.NeutralEvent);
        }

    }

    public class Mod_CEManualPatcher : Mod
    {
        public static ModSetting_CEManualPatcher settings { get; private set; }

        public static Mod_CEManualPatcher instance { get; private set; }

        private MP_SettingTab curTab = MP_SettingTab.Weapon;

        public Mod_CEManualPatcher(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSetting_CEManualPatcher>();
            instance = this;
        }

        public override string SettingsCategory() => "CE Manual Patcher";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            inRect.y += 20f;
            inRect.height -= 20f;

            var tabRecords = new List<TabRecord>();

            foreach (MP_SettingTab tab in Enum.GetValues(typeof(MP_SettingTab)))
            {
                //skip for now
                if (tab == MP_SettingTab.Bionic)
                {
                    continue;
                }

                tabRecords.Add(new TabRecord(tab.GetLabel(), () => curTab = tab, curTab == tab));
            }

            TabDrawer.DrawTabs<TabRecord>(inRect, tabRecords, 150f);

            inRect.y += 10f;
            inRect.height -= 10f;

            settings.GetManager(curTab)?.DoWindowContents(inRect);

            WidgetsUtility.UtilityTick();
        }

        public void SetCurTab(MP_SettingTab tab) => curTab = tab;
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
