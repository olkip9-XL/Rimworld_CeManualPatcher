using CeManualPatcher.Patch;
using CeManualPatcher.RenderRect.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc.Manager
{
    internal class BodyDefManager : MP_DefManagerBase<BodyDef>
    {
        public static BodyDefManager instance;

        public BodyDef curBody;

        private Rect_BodyInfo rect_BodyInfo = new Rect_BodyInfo();
        private Rect_BodyList rect_BodyList = new Rect_BodyList();

        private Dictionary<BodyDef, Texture2D> iconDicInt = null;
        public Dictionary<BodyDef, Texture2D> iconDic
        {
            get
            {
                if (iconDicInt == null)
                {
                    iconDicInt = new Dictionary<BodyDef, Texture2D>();

                    foreach (var item in DefDatabase<BodyDef>.AllDefs)
                    {
                        Texture2D texture = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(x => x.race?.body == item)?.uiIcon;

                        if(texture == null)
                            texture = BaseContent.BadTex;

                        iconDicInt.Add(item, texture);
                    }
                }
                return iconDicInt;
            }
        }

        public BodyDefManager()
        {
            instance = this;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect leftRect = rect.LeftPart(0.3f);
            Rect rightRect = rect.RightPart(0.7f);

            rect_BodyList.DoWindowContents(leftRect);
            rect_BodyInfo.DoWindowContents(rightRect);
        }

        protected override void NewPatch(ref PatchBase<BodyDef> patch, BodyDef def)
        {
        }
    }
}
