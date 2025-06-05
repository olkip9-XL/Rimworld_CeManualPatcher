using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher
{
    public class CEPatcher : IExposable
    {
        private string thingDefString;
        public ThingDef thingDef
        {
            get => DefDatabase<ThingDef>.GetNamed(thingDefString, false);
            set => thingDefString = value?.defName ?? "null";
        }

        private CEPatchManager patchManager => CEPatchManager.instance;
        private WeaponManager weaponManager => WeaponManager.instance;

        //patch data
        internal List<StatModifier> stats = new List<StatModifier>();
        internal List<StatModifier> statOffsets = new List<StatModifier>();
        internal VerbPropertiesCE verbProperties;
        internal List<Tool> tools = new List<Tool>();
        internal CompProperties_AmmoUser ammoUser;
        internal CompProperties_FireModes fireMode;
        internal List<string> weaponTags;

        public CEPatcher() { }
        public CEPatcher(ThingDef thingDef)
        {
            this.thingDef = thingDef;

            if (thingDef == null)
                return;

            weaponManager.Reset(thingDef);
            weaponManager.GetPatch(thingDef);

            InitNewData();
        }
        public void ApplyCEPatch()
        {
            if (thingDef == null)
            {
                return;
            }

            //add
            if (verbProperties != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddVerb();
            }

            if (ammoUser != null && ammoUser.ammoSet != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddAmmoUser();
            }

            if (fireMode != null)
            {
                weaponManager.GetWeaponPatch(thingDef).AddFireMode();
            }

            ReplaceData();
        }

        private void ReplaceData()
        {
            //statBases

            if (thingDef.statBases != null)
            {
                thingDef.statBases = this.stats;
                HandleStats();
            }

            //statOffsets
            thingDef.equippedStatOffsets = this.statOffsets;

            //weaponTags
            thingDef.weaponTags = this.weaponTags;

            //verbs
            if (!thingDef.Verbs.NullOrEmpty())
            {
                thingDef.Verbs[0] = this.verbProperties;
            }

            //tools
            if (thingDef.tools != null)
            {
                thingDef.tools = this.tools;
            }

            //comps
            if (thingDef.comps != null)
            {
                if (!thingDef.HasComp<CompAmmoUser>() && ammoUser != null && ammoUser.ammoSet != null)
                {
                    thingDef.comps.Add(ammoUser);
                }
                if (!thingDef.HasComp<CompFireModes>() && fireMode != null)
                {
                    thingDef.comps.Add(fireMode);
                }
            }
        }

        private void HandleStats()
        {
            StatModifier statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.Recoil);
            if (statModifier != null && this.verbProperties != null)
            {
                this.verbProperties.recoilAmount = statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.TicksBetweenBurstShots);
            if (statModifier != null && this.verbProperties != null)
            {
                this.verbProperties.ticksBetweenBurstShots = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.BurstShotCount);
            if (statModifier != null && this.verbProperties != null)
            {
                this.verbProperties.burstShotCount = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.MagazineCapacity);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.magazineSize = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.ReloadSpeed);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.reloadTime = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.AmmoGenPerMagOverride);
            if (statModifier != null && this.ammoUser != null)
            {
                this.ammoUser.AmmoGenPerMagOverride = (int)statModifier.value;
            }

            statModifier = this.stats.FirstOrDefault(x => x.stat == CE_StatDefOf.BipodStats);
            if (statModifier != null && this.verbProperties != null)
            {
                thingDef.weaponTags.Add("Bipod_LMG");
            }

        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref thingDefString, "thingDef");
        }
        public void PostLoadInit()
        {
            if (thingDef == null)
            {
                return;
            }
        }

        private void InitNewData()
        {
            //statBases
            if (thingDef.statBases != null)
            {
                this.stats = new List<StatModifier>();
                foreach (var stat in thingDef.statBases)
                {
                    if (MP_Options.exceptStatDefs.Contains(stat.stat))
                    {
                        continue;
                    }

                    StatModifier statModifier = new StatModifier();
                    PropUtility.CopyPropValue(stat, statModifier);
                    this.stats.Add(statModifier);
                }

                // Add CE stats, default AR
                stats.Add(new StatModifier() { stat = CE_StatDefOf.Bulk, value = 10.03f });

                if (!thingDef.Verbs.NullOrEmpty())
                {
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.TicksBetweenBurstShots, value = 4 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.BurstShotCount, value = 6 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SightsEfficiency, value = 1 });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.SwayFactor, value = 1.33f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ShotSpread, value = 0.07f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.Recoil, value = 1.5f });

                    stats.Add(new StatModifier() { stat = CE_StatDefOf.MagazineCapacity, value = 30f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.ReloadSpeed, value = 4f });
                    stats.Add(new StatModifier() { stat = CE_StatDefOf.AmmoGenPerMagOverride });
                }
            }

            //statOffsets

            if (thingDef.equippedStatOffsets != null)
                this.statOffsets = CopyUtility.CopyStats(thingDef.equippedStatOffsets);

            //weaponTags
            this.weaponTags = new List<string>();
            if (thingDef.weaponTags != null)
            {
                this.weaponTags.AddRange(thingDef.weaponTags);
            }

            //verbs
            if (!thingDef.Verbs.NullOrEmpty())
            {
                this.verbProperties = PropUtility.ConvertToChild<VerbProperties, VerbPropertiesCE>(thingDef.Verbs[0]);

                if (this.verbProperties.verbClass == typeof(Verb_Shoot))
                    this.verbProperties.verbClass = typeof(Verb_ShootCE);

                if (this.verbProperties.verbClass == typeof(Verb_ShootOneUse))
                    this.verbProperties.verbClass = typeof(Verb_ShootCEOneUse);

                if (this.verbProperties.verbClass == typeof(Verb_LaunchProjectile))
                    this.verbProperties.verbClass = typeof(Verb_LaunchProjectileCE);

                this.verbProperties.defaultProjectile = MP_ProjectileDefOf.Bullet_556x45mmNATO_FMJ;
            }

            //tools
            if (thingDef.tools != null)
            {
                this.tools = new List<Tool>();
                foreach (var tool in thingDef.tools)
                {
                    ToolCE toolCopy = PropUtility.ConvertToChild<Tool, ToolCE>(tool);

                    List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();
                    capacities.AddRange(tool.capacities);
                    toolCopy.capacities = capacities;

                    this.tools.Add(toolCopy);
                }
            }

            //comps
            if (thingDef.comps != null && !thingDef.Verbs.NullOrEmpty())
            {
                this.ammoUser = new CompProperties_AmmoUser();
                if (thingDef.HasComp<CompAmmoUser>())
                {
                    PropUtility.CopyPropValue(thingDef.GetCompProperties<CompProperties_AmmoUser>(), this.ammoUser);
                }
                this.ammoUser.ammoSet = MP_AmmoSetDefOf.AmmoSet_556x45mmNATO;

                this.fireMode = new CompProperties_FireModes();
                if (thingDef.HasComp<CompFireModes>())
                {
                    PropUtility.CopyPropValue(thingDef.GetCompProperties<CompProperties_FireModes>(), this.fireMode);
                }
                if (fireMode.aimedBurstShotCount <= 0)
                {
                    fireMode.aimedBurstShotCount = 3;
                }
            }
        }

        public void ExportToFile(string dirPath)
        {
            if (this.thingDef == null)
                return;

            string folderPath = Path.Combine(dirPath, thingDef.modContentPack.Name);
            folderPath = Path.Combine(folderPath, "Weapon");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, thingDef.defName + ".xml");

            // 创建XML文档
            XmlElement rootPatchElement = null;
            XmlDocument xmlDoc = XmlUtility.CreateBasePatchDoc(ref rootPatchElement, thingDef.modContentPack.Name);

            XmlUtility.Replace_Tools(xmlDoc, rootPatchElement, thingDef.defName, thingDef.tools);
            XmlUtility.MakeGunCECompatible(xmlDoc, rootPatchElement, thingDef);
            XmlUtility.Replace_StatOffsets(xmlDoc, rootPatchElement, thingDef.defName, thingDef.equippedStatOffsets);

            xmlDoc.Save(filePath);
        }
    }
}
