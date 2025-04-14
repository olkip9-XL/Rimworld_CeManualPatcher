using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    internal class AmmoDefSaveable : SaveableBase
    {
        //字段
        private string label;

        //original
        private string originalLabel;
        public AmmoDefSaveable() { }

        public AmmoDefSaveable(ThingDef ammoDef)
        {
            if (ammoDef == null)
            {
                return;
            }

            this.thingDef = ammoDef;

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && thingDef != null)
            {
                this.label = thingDef.label;
            }

            Scribe_Values.Look(ref label, "ammoLabel");
        }

        public override void Reset()
        {
            if (thingDef == null)
            {
                return;
            }
            thingDef.label = originalLabel;
        }

        protected override void Apply()
        {
            if (thingDef == null)
            {
                return;
            }

            thingDef.label = label;
        }

        protected override void InitOriginalData()
        {
            if (thingDef == null)
            {
                return;
            }

            this.originalLabel = thingDef.label;
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            if (thingDef == null)
            {
                return;
            }
            InitOriginalData();
            this.Apply();
        }
    }
}
