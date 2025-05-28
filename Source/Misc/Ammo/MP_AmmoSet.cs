using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc
{
    public class MP_AmmoSet
    {
        public AmmoSetDef ammoSetDef { get; private set; }

        public List<MP_Ammo> ammoList = new List<MP_Ammo>();

        public MP_AmmoCategory category;

        public string sourceModName { get; private set; }
        public string Label
        {
            get
            {
                if (ammoSetDef != null)
                {
                    return ammoSetDef.label;
                }
                else if (ammoList.Any())
                {
                    return ammoList.First().Label;
                }
                else
                {
                    return "null";
                }
            }
        }
        public Texture2D Icon
        {
            get
            {
                if (ammoList.Any())
                {
                    return ammoList.First().Icon;
                }
                else
                {
                    return Texture2D.whiteTexture;
                }
            }
        }
        public string DefName
        {
            get
            {
                if (ammoSetDef != null)
                {
                    return ammoSetDef.defName;
                }
                else if (ammoList.Any())
                {
                    return ammoList.First().DefName;
                }
                else
                {
                    return "null";
                }
            }
        }
        public string Description
        {
            get
            {

                if (ammoList.Any())
                {
                    return ammoList.First().Description;
                }
                else
                {
                    return "null";
                }
            }
        }

        public MP_AmmoSet() { }
        public MP_AmmoSet(AmmoSetDef ammoSetDef)
        {
            this.ammoSetDef = ammoSetDef;

            for(int i = 0; i < ammoSetDef.ammoTypes.Count; i++)
            {
                AmmoLink ammo = ammoSetDef.ammoTypes[i];

                if (ammo.ammo == null)
                {
                    MP_Log.Warning($"MP_AmmoSet : ammoSetDef {ammoSetDef?.defName ?? "null"} From {ammoSetDef?.modContentPack.Name ?? "null"} has a NULL Ammo at [{i}] item , skipping");
                    continue;
                }

                if (ammo.projectile == null || ammo.projectile.projectile == null)
                {
                    MP_Log.Warning($"MP_AmmoSet : ammoSetDef {ammoSetDef?.defName ?? "null"} From {ammoSetDef?.modContentPack.Name ?? "null"} has a Null Projectile at [{i}] item , skipping");
                    continue;
                }

                ammoList.Add(new MP_Ammo(ammo));
            }
           
            this.sourceModName = ammoSetDef.modContentPack?.Name;

            List<ThingCategoryDef> categoryUnderAmmo = DefDatabase<ThingCategoryDef>.AllDefs.Where(x => x.parent == MP_ThingCategoryDefOf.Ammo).ToList();
            ThingCategoryDef targetDef = categoryUnderAmmo.Find(x => ammoSetDef.ammoTypes[0].ammo.IsWithinCategory(x));

            this.category = MP_Options.ammoCategories.Find(x => x.thingCategoryDef == targetDef);
        }

        //未分类专用
        public MP_AmmoSet(ThingDef ammo, ThingDef projectile, MP_AmmoCategory category)
        {
            if (projectile == null)
            {
                MP_Log.Warning("MP_AmmoSet : projectile can't be null");
                return;
            }

            if (projectile.projectile == null)
            {
                MP_Log.Warning("MP_AmmoSet : projectile must have a projectile propertie");
                return;
            }

            this.ammoSetDef = null;
            ammoList.Add(new MP_Ammo(projectile, ammo));
            this.sourceModName = projectile.modContentPack?.Name;
            this.category = category;
        }
    }
}
