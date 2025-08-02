using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Extension
{
    public static class MP_SettingTabExtension
    {
        public static string GetLabel(this MP_SettingTab tab)
        {
            switch (tab)
            {
                case MP_SettingTab.Weapon:
                    return "MP_Weapon".Translate();
                case MP_SettingTab.Ammo:
                    return "MP_Ammo".Translate();
                case MP_SettingTab.Bionic:
                    return "MP_Bionic".Translate();
                case MP_SettingTab.Apparel:
                    return "MP_Apparel".Translate();
                case MP_SettingTab.CustomAmmo:
                    return "MP_CustomAmmo".Translate();
                case MP_SettingTab.Body:
                    return "MP_Body".Translate();
                case MP_SettingTab.Race:
                    return "MP_Race".Translate();
                default:
                    return "Unknown";
            }
        }
    }
}
