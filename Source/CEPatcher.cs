using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    public class CEPatcher : IExposable
    {
        public ThingDef thingDef;

        //patch data
        internal VerbPropertiesCESaveable verb_patch;

        internal List<StatSaveable> stat_patches = new List<StatSaveable>();

        internal List<ToolCESaveable> tool_patches = new List<ToolCESaveable>();

        internal CompAmmoUserSaveable ammoUser_patch;

        internal CompFireModesSaveable fireMode_patch;

        //original data
        VerbProperties verb;

        List<StatModifier> stats = new List<StatModifier>();

        List<Tool> tools = new List<Tool>();

        public CEPatcher() { }
        public CEPatcher(ThingDef thingDef)
        {
            this.thingDef = thingDef;

            if (thingDef == null)
                return;

            //init original data
            InitOriginalData();
        }


        private void InitOriginalData() {
            //statBases
            if (thingDef.statBases != null)
            {
                this.stats.Clear();
                this.stats.AddRange(thingDef.statBases);

                //vanilla stat
                foreach (var item in this.stats)
                {
                    stat_patches.Add(new StatSaveable(item));
                }

                //CE stat
                List<StatDef> patchStats = new List<StatDef>()
                {
                    StatDefOf.WorkToMake,
                    StatDefOf.Mass,
                    StatDefOf.RangedWeapon_Cooldown,

                    CE_StatDefOf.SightsEfficiency,
                    CE_StatDefOf.ShotSpread,
                    CE_StatDefOf.SwayFactor,
                    CE_StatDefOf.Bulk,
                };

                foreach (StatDef item in patchStats)
                {
                    StatSaveable statSaveable = new StatSaveable()
                    {
                        StatDef = item,
                    };
                    this.stat_patches.Add(statSaveable);
                }

            }

            //verbs
            if (!thingDef.Verbs.NullOrEmpty() && !(thingDef.Verbs[0] is VerbPropertiesCE))
            {
                this.verb = thingDef.Verbs[0];

                this.verb_patch = new VerbPropertiesCESaveable()
                {
                    //set vanilla data
                    hasStandardCommand = this.verb.hasStandardCommand,
                    defaultProjectile = MP_ProjectileDefOf.Bullet_556x45mmNATO_FMJ,
                    warmupTime = this.verb.warmupTime,
                    range = this.verb.range,
                    burstShotCount = this.verb.burstShotCount,
                    ticksBetweenBurstShots = this.verb.ticksBetweenBurstShots,
                    soundCast = this.verb.soundCast,
                    soundCastTail = this.verb.soundCastTail,
                    muzzleFlashScale = this.verb.muzzleFlashScale
                };
            }

            //tools
            if (thingDef.tools != null && thingDef.tools.Any(x => !(x is ToolCE)))
            {
                this.tools.Clear();
                this.tools.AddRange(thingDef.tools);

                this.tool_patches.Clear();

                foreach (var item in this.tools)
                {
                    if (item is ToolCE toolCE)
                    {
                        tool_patches.Add(new ToolCESaveable(toolCE));
                    }
                    else
                    {
                        tool_patches.Add(new ToolCESaveable()
                        {
                            id = item.id,
                            label = item.label,
                            power = item.power,
                            cooldownTime = item.cooldownTime,
                            linkedBodyPartsGroup = item.linkedBodyPartsGroup,
                            alwaysTreatAsWeapon = item.alwaysTreatAsWeapon,
                            chanceFactor = item.chanceFactor,
                        });
                    }
                }

            }


            //comps
            if (thingDef.comps != null)
            {
                if (!thingDef.HasComp<CompAmmoUser>())
                {
                    ammoUser_patch = new CompAmmoUserSaveable()
                    {
                        ammoSet = MP_AmmoSetDefOf.AmmoSet_556x45mmNATO,
                    };
                }

                if (!thingDef.HasComp<CompFireModes>())
                {
                    fireMode_patch = new CompFireModesSaveable();
                }
            }
        }
        public void ApplyCEPatch()
        {
            if (thingDef == null)
            {
                return;
            }
            //statBases
            
            if(thingDef.statBases != null)
            {
                thingDef.statBases = new List<StatModifier>();
                stat_patches.ForEach(x => x?.Apply(thingDef));
            }

            //verbs
            if (!thingDef.Verbs.NullOrEmpty() && !(thingDef.Verbs[0] is VerbPropertiesCE))
            {
                thingDef.Verbs[0] = new VerbPropertiesCE();

                verb_patch?.Apply(thingDef);
            }
            //tools
            if (!thingDef.tools.NullOrEmpty())
            {
                thingDef.tools = new List<Tool>();

                tool_patches.ForEach(x => x?.Apply(thingDef));
            }
            //comps
            if (thingDef.comps != null)
            {
                if(!thingDef.HasComp<CompAmmoUser>())
                {
                    thingDef.comps.Add(new CompProperties_AmmoUser());
                    ammoUser_patch?.Apply(thingDef);
                }

                if (!thingDef.HasComp<CompFireModes>())
                {
                    thingDef.comps.Add(new CompProperties_FireModes());
                    fireMode_patch?.Apply(thingDef);
                }
            }
        }

        public void ResetCEPatch()
        {
            if (thingDef == null)
            {
                return;
            }
            //statBases
            if (thingDef.statBases != null)
            {
                thingDef.statBases = this.stats;
            }
            //verbs
            if (!thingDef.Verbs.NullOrEmpty() && thingDef.Verbs[0] is VerbPropertiesCE)
            {
                thingDef.Verbs[0] = this.verb;
            }
            //tools
            if (!thingDef.tools.NullOrEmpty())
            {
                thingDef.tools = this.tools;

            }
            //comps
            if (thingDef.comps != null)
            {
                if (ammoUser_patch != null)
                {
                    thingDef.comps.RemoveAll(x => x is CompProperties_AmmoUser);
                }
                if (fireMode_patch != null)
                {
                    thingDef.comps.RemoveAll(x => x is CompProperties_FireModes);
                }
            }
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }
        public void PostLoadInit()
        {
            //字段
            verb_patch?.PostLoadInit(thingDef);
            ammoUser_patch?.PostLoadInit(thingDef);
            fireMode_patch?.PostLoadInit(thingDef);
            stat_patches.ForEach(x => x?.PostLoadInit(thingDef));
            tool_patches.ForEach(x => x?.PostLoadInit(thingDef));

            //original data
            InitOriginalData();
        }
    }
}
