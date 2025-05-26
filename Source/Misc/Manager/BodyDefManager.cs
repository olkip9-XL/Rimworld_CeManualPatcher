using CeManualPatcher.Misc.Patch;
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
                   InitDics();
                }
                return iconDicInt;
            }
        }

        private Dictionary<BodyDef, string> descriptionDicInt = null;
        public Dictionary<BodyDef, string> descriptionDic
        {
            get
            {
                if(descriptionDicInt == null)
                {
                   InitDics();
                }
                return descriptionDicInt;
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

            try
            {
                rect_BodyList.DoWindowContents(leftRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("BodyDefManager body list error", e);
            }

            try
            {
                rect_BodyInfo.DoWindowContents(rightRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("BodyDefManager body info error", e, curBody);
            }

        }

        protected override void NewPatch(ref PatchBase<BodyDef> patch, BodyDef def)
        {
            patch = new BodyDefPatch(def);
        }

        public bool HasPatch(BodyDef def)
        {
           return patches.Any(x => x?.targetDef == def);
        }

        private void InitDics()
        {
            iconDicInt = new Dictionary<BodyDef, Texture2D>();
            descriptionDicInt = new Dictionary<BodyDef, string>();

            foreach (var item in DefDatabase<BodyDef>.AllDefs)
            {
                ThingDef thingDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(x => x.race?.body == item);

                Texture2D texture = thingDef?.uiIcon;
                string desc = thingDef?.description;

                if (texture == null)
                    texture = BaseContent.BadTex;

                iconDicInt.Add(item, texture);
                descriptionDicInt.Add(item, desc);
            }
        }
      
    }
}
