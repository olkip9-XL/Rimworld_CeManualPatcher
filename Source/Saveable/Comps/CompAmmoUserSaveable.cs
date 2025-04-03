using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Saveable
{
    internal class CompAmmoUserSaveable : SaveableBase
    {
        public int magazineSize = 0;

        public int AmmoGenPerMagOverride = 0;

        public float reloadTime = 1f;

        public bool reloadOneAtATime = false;

        public bool throwMote = true;

        private string ammoSetString;
        public AmmoSetDef ammoSet
        {
            get => DefDatabase<AmmoSetDef>.GetNamed(ammoSetString, false);
            set => ammoSetString = value.defName;
        }

        public float loadedAmmoBulkFactor = 0f;

        private CompAmmoUserSaveable Original => base.originalData as CompAmmoUserSaveable;

        public CompAmmoUserSaveable() : base()
        {
        }

        public CompAmmoUserSaveable(ThingDef thingDef, bool isOriginal = false) : base(thingDef, isOriginal)
        {
            if (thingDef == null || !thingDef.HasComp<CompAmmoUser>())
            {
                return;
            }

            CompProperties_AmmoUser compProperties_AmmoUser = thingDef.GetCompProperties<CompProperties_AmmoUser>();

            if (compProperties_AmmoUser == null)
            {
                return;
            }

            //初始化
            this.magazineSize = compProperties_AmmoUser.magazineSize;
            this.AmmoGenPerMagOverride = compProperties_AmmoUser.AmmoGenPerMagOverride;
            this.reloadTime = compProperties_AmmoUser.reloadTime;
            this.reloadOneAtATime = compProperties_AmmoUser.reloadOneAtATime;
            this.throwMote = compProperties_AmmoUser.throwMote;
            this.ammoSet = compProperties_AmmoUser.ammoSet;
            this.loadedAmmoBulkFactor = compProperties_AmmoUser.loadedAmmoBulkFactor;

            if (!isOriginal)
                this.originalData = new CompAmmoUserSaveable(thingDef, true);
        }

        public override void Apply(ThingDef thingDef)
        {
            if (thingDef == null || !thingDef.HasComp<CompAmmoUser>())
            {
                return;
            }
            CompProperties_AmmoUser compProperties_AmmoUser = thingDef.GetCompProperties<CompProperties_AmmoUser>();
            //应用
            compProperties_AmmoUser.magazineSize = this.magazineSize;
            compProperties_AmmoUser.AmmoGenPerMagOverride = this.AmmoGenPerMagOverride;
            compProperties_AmmoUser.reloadTime = this.reloadTime;
            compProperties_AmmoUser.reloadOneAtATime = this.reloadOneAtATime;
            compProperties_AmmoUser.throwMote = this.throwMote;
            compProperties_AmmoUser.ammoSet = this.ammoSet;
            compProperties_AmmoUser.loadedAmmoBulkFactor = this.loadedAmmoBulkFactor;
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref magazineSize, "magazineSize", 0);
            Scribe_Values.Look(ref AmmoGenPerMagOverride, "AmmoGenPerMagOverride", 0);
            Scribe_Values.Look(ref reloadTime, "reloadTime", 1f);
            Scribe_Values.Look(ref reloadOneAtATime, "reloadOneAtATime", false);
            Scribe_Values.Look(ref throwMote, "throwMote", true);
            Scribe_Values.Look(ref ammoSetString, "ammoSet", null);
            Scribe_Values.Look(ref loadedAmmoBulkFactor, "loadedAmmoBulkFactor", 0f);
        }

        public override void Reset(ThingDef thingDef)
        {
            if (Original == null || thingDef == null || !thingDef.HasComp<CompAmmoUser>())
            {
                return;
            }
            this.Original.Apply(thingDef);
        }

        public override void PostLoadInit(ThingDef thingDef)
        {

            this.originalData = new CompAmmoUserSaveable(thingDef, true);
        }
    }
}
