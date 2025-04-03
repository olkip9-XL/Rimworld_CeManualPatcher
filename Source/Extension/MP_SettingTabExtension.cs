using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeManualPatcher.Extension
{
    public static class MP_SettingTabExtension
    {
        public static string GetLabel(this MP_SettingTab tab)
        {
            switch (tab)
            {
                case MP_SettingTab.Weapon:
                    return "Weapon";
                case MP_SettingTab.Ammo:
                    return "Ammo";
                case MP_SettingTab.Bionic:
                    return "Bionic";
                default:
                    return "Unknown";
            }
        }
    }
}
