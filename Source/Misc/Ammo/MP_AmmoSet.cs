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

        public string sourceModName { get;private set; }
        public string Label
        {
            get
            {
                if(ammoSetDef != null)
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
            foreach (var ammo in ammoSetDef.ammoTypes)
            {
                ammoList.Add(new MP_Ammo(ammo));    
            }
            this.sourceModName = ammoSetDef.modContentPack?.Name;

            List<ThingCategoryDef > categoryUnderAmmo = DefDatabase<ThingCategoryDef>.AllDefs.Where(x => x.parent == MP_ThingCategoryDefOf.Ammo).ToList();
            ThingCategoryDef targetDef = categoryUnderAmmo.Find(x =>ammoSetDef.ammoTypes[0].ammo.IsWithinCategory(x));
            //if(targetDef == null)
            //{
            //    Log.Warning("未找到弹药分类：" + ammoSetDef.ammoTypes[0].ammo.defName);
            //    this.category = MP_Options.ammoCategories.Find(x => x.name == "Uncategorized");
            //}
            //else
            //{
            //    this.category = MP_Options.ammoCategories.Find(x => x.thingCategoryDef == targetDef);
            //}
            this.category = MP_Options.ammoCategories.Find(x => x.thingCategoryDef == targetDef);
        }

        //未分类专用
        public MP_AmmoSet(ThingDef ammo, ThingDef projectile, MP_AmmoCategory category)
        {
            this.ammoSetDef = null;
            ammoList.Add(new MP_Ammo(projectile, ammo));
            this.sourceModName = projectile.modContentPack?.Name;
            this.category = category;
        }
    }
}
