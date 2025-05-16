using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable.Ammo
{
    internal class AmmoDefSaveable : SaveableBase<ThingDef>
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

            this.def = ammoDef;

            InitOriginalData();
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && def != null)
            {
                this.label = def.label;
            }

            Scribe_Values.Look(ref label, "ammoLabel");
        }

        public override void Reset()
        {
            if (def == null)
            {
                return;
            }
            def.label = originalLabel;
        }

        protected override void Apply()
        {
            if (def == null)
            {
                return;
            }

            def.label = label;
        }

        protected override void InitOriginalData()
        {
            if (def == null)
            {
                return;
            }

            this.originalLabel = def.label;
        }

        public override void PostLoadInit(ThingDef thingDef)
        {
            this.def = thingDef;
            if (thingDef == null)
            {
                return;
            }
            InitOriginalData();
            this.Apply();
        }
    }
}
