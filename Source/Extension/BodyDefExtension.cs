using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Extension
{
    public static class BodyDefExtension
    {
        public static List<BodyPartGroupDef> AllBodyPartGroup(this BodyDef bodyDef)
        {
            List<BodyPartGroupDef> list = new List<BodyPartGroupDef>();
            Search(list, bodyDef.corePart);
            return list;
        }

        private static void Search(List<BodyPartGroupDef> list, BodyPartRecord bodyPartRecord)
        {
            if (bodyPartRecord == null)
            {
                return;
            }

            if (bodyPartRecord.groups != null)
            {
                list.AddRange(bodyPartRecord.groups);
            }

            if (bodyPartRecord.parts != null)
            {
                foreach (var item in bodyPartRecord.parts)
                {
                    Search(list, item);
                }
            }
        }
    }
}
