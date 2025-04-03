using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    public class MP_AmmoCategory
    {
        public ThingCategoryDef thingCategoryDef;

        public string name;
        public string Label
        {
            get
            {
                if (thingCategoryDef != null)
                {
                    return thingCategoryDef.label;
                }
                else
                {
                    switch (name)
                    {
                        case "Grenades":
                            return "Grenades";
                        default:
                            return "Uncategorized";
                    }
                }
            }
        }

        public MP_AmmoCategory(ThingCategoryDef thingCategoryDef)
        {
            this.thingCategoryDef = thingCategoryDef;
            this.name = thingCategoryDef.defName;
        }

        public MP_AmmoCategory(string name)
        {
            this.name = name;
        }

    }
}
