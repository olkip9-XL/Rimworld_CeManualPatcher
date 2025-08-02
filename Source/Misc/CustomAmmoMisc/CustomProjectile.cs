using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Misc
{
    internal class CustomProjectile : IExposable
    {
        public CustomAmmoSet parentSet;
        public AmmoProjectilePair parentPair;

        //properties
        public MP_GraphicWarpper graphicWarpper = new MP_GraphicWarpper();

        public int damageAmount = -1;

        public float suppressionFactor = 1f;
        public float stoppingPower = 0.5f;
        public float speed = 168f;

        public string label;

        //Non explosive
        public float armorPenetrationBlunt;
        public float armorPenetrationSharp;

        public List<SecondaryDamage> secondaryDamages = new List<SecondaryDamage>();

        //Explosive
        public float explosionRadius;
        public GasType? gas;

        //comp
        public List<ThingDefCountClass> fragments = new List<ThingDefCountClass>();
        public CompProperties_ExplosiveCE secondaryExplosion = new CompProperties_ExplosiveCE();

        private string damageDefString = "Bullet";

        //Save
        private List<MP_ThingDefCountClass_Save> fragments_save = new List<MP_ThingDefCountClass_Save>();
        private List<MP_SecondaryDamage_Save> secondaryDamages_save = new List<MP_SecondaryDamage_Save>();
        private MP_CompProperties_ExplosiveCE_Save secondaryExplosion_save = new MP_CompProperties_ExplosiveCE_Save();


        public DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value?.defName ?? "null";
        }

        private string suffix => parentPair.suffix;
        public string DefName
        {
            get
            {
                return $"Bullet_{parentSet.defNameBase}_{suffix}";
            }
        }

        public CustomProjectile() { }
        public CustomProjectile(CustomAmmoSet parent)
        {
            this.parentSet = parent;

            this.graphicWarpper = new MP_GraphicWarpper()
            {
                texPath = parent.projectileGraphicWarpper.texPath,
                graphicClass = parent.projectileGraphicWarpper.graphicClass
            };

            this.speed = parent.baseSpeed;
        }

        public void Export(XmlDocument doc, XmlElement root)
        {
            IEnumerable<string> errors = ConfigError();
            if (errors.Any())
            {
                foreach (var item in errors)
                {
                    Log.Error($"[CeManualPatcher] CustomProjectile {DefName} error: {item}");
                }

                return;
            }

            XmlElement xmlElement = doc.CreateElement("ThingDef");
            xmlElement.SetAttribute("ParentName", parentSet.BaseProjectileName);
            root.AppendChild(xmlElement);

            XmlUtility.AddChildElement(doc, xmlElement, "defName", DefName);
            if (!label.NullOrEmpty())
            {
                XmlUtility.AddChildElement(doc, xmlElement, "label", label);
            }

            //graphic
            if (graphicWarpper.graphicClass != parentSet.projectileGraphicWarpper.graphicClass ||
                   graphicWarpper.texPath != parentSet.projectileGraphicWarpper.texPath)
            {

                XmlElement graphicElement = doc.CreateElement("graphicData");
                xmlElement.AppendChild(graphicElement);
                XmlUtility.AddChildElement(doc, graphicElement, "texPath", graphicWarpper.texPath);
                XmlUtility.AddChildElement(doc, graphicElement, "graphicClass", graphicWarpper.graphicClass.ToString());
            }

            //projectile
            CreateProjectile();

            void CreateProjectile()
            {
                XmlElement projectileElement = doc.CreateElement("projectile");
                projectileElement.SetAttribute("Class", "CombatExtended.ProjectilePropertiesCE");
                xmlElement.AppendChild(projectileElement);


                if (damageAmount != -1)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "damageAmountBase", damageAmount.ToString());
                }

                if (this.damageDef != DamageDefOf.Bullet)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "damageDef", damageDefString);
                }

                if (Math.Abs(explosionRadius) > float.Epsilon)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "explosionRadius", explosionRadius.ToString());

                    if (gas != null)
                    {
                        XmlUtility.AddChildElement(doc, projectileElement, "postExplosionGasType", gas.ToString());
                    }

                }
                else
                {
                    if (Math.Abs(armorPenetrationBlunt) > float.Epsilon)
                    {
                        XmlUtility.AddChildElement(doc, projectileElement, "armorPenetrationBlunt", armorPenetrationBlunt.ToString());
                    }
                    if (Math.Abs(armorPenetrationSharp) > float.Epsilon)
                    {
                        XmlUtility.AddChildElement(doc, projectileElement, "armorPenetrationSharp", armorPenetrationSharp.ToString());
                    }

                    if (!secondaryDamages.NullOrEmpty())
                    {
                        XmlElement secondaryDamageElement = doc.CreateElement("secondaryDamage");
                        projectileElement.AppendChild(secondaryDamageElement);


                        foreach (var item in secondaryDamages)
                        {
                            XmlElement liElement = doc.CreateElement("li");
                            secondaryDamageElement.AppendChild(liElement);

                            XmlUtility.AddChildElement(doc, liElement, "def", item.def.defName);
                            XmlUtility.AddChildElement(doc, liElement, "amount", item.amount.ToString());
                        }
                    }
                }

                if (Math.Abs(speed - parentSet.baseSpeed) > float.Epsilon)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "speed", speed.ToString());
                }

                if (Math.Abs(suppressionFactor - 1f) > float.Epsilon)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "suppressionFactor", suppressionFactor.ToString());
                }

                if (Math.Abs(stoppingPower - 0.5f) > float.Epsilon)
                {
                    XmlUtility.AddChildElement(doc, projectileElement, "stoppingPower", stoppingPower.ToString());
                }

                //comps
                List<XmlElement> compsElements = new List<XmlElement>();
                compsElements.Add(GetSecondaryExplosive());
                compsElements.Add(GetFragments());
                compsElements.RemoveWhere(x => x == null);
                if (compsElements.Any())
                {
                    XmlElement comp = doc.CreateElement("comps");
                    xmlElement.AppendChild(comp);
                    foreach (var item in compsElements)
                    {
                        comp.AppendChild(item);
                    }
                }

                XmlElement GetSecondaryExplosive()
                {
                    if (secondaryExplosion == null ||
                        secondaryExplosion.explosiveDamageType == null)
                        return null;

                    XmlElement liElement = doc.CreateElement("li");
                    liElement.SetAttribute("Class", "CombatExtended.CompProperties_ExplosiveCE");

                    XmlUtility.AddChildElement(doc, liElement, "damageAmountBase", secondaryExplosion.damageAmountBase.ToString());
                    XmlUtility.AddChildElement(doc, liElement, "explosiveDamageType", secondaryExplosion.explosiveDamageType.defName);
                    XmlUtility.AddChildElement(doc, liElement, "explosiveRadius", secondaryExplosion.explosiveRadius.ToString());
                    if (secondaryExplosion.postExplosionGasType != null)
                    {
                        XmlUtility.AddChildElement(doc, liElement, "postExplosionGasType", secondaryExplosion.postExplosionGasType.ToString());
                    }

                    return liElement;
                }

                XmlElement GetFragments()
                {
                    if (fragments.NullOrEmpty())
                        return null;

                    XmlElement liElement = doc.CreateElement("li");
                    liElement.SetAttribute("Class", "CombatExtended.CompProperties_Fragments");

                    XmlElement fragmentsElement = doc.CreateElement("fragments");
                    liElement.AppendChild(fragmentsElement);

                    foreach (var item in fragments)
                    {
                        XmlUtility.AddChildElement(doc, fragmentsElement, item.thingDef.defName, item.count.ToString());
                    }

                    return liElement;
                }

            }

        }

        public void ExposeData()
        {
            try
            {
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    if (!secondaryDamages.NullOrEmpty())
                    {
                        secondaryDamages_save = new List<MP_SecondaryDamage_Save>();
                        foreach (var item in secondaryDamages)
                        {
                            secondaryDamages_save.Add(new MP_SecondaryDamage_Save(item));
                        }
                    }

                    if (!fragments.NullOrEmpty())
                    {
                        fragments_save = new List<MP_ThingDefCountClass_Save>();
                        foreach (var item in fragments)
                        {
                            fragments_save.Add(new MP_ThingDefCountClass_Save(item));
                        }
                    }

                    if (secondaryExplosion != null)
                    {
                        secondaryExplosion_save = new MP_CompProperties_ExplosiveCE_Save(secondaryExplosion);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"[CeManualPatcher] Error in CustomProjectile ExposeData: {e}");
            }

            Scribe_Values.Look(ref label, "label");
            Scribe_Deep.Look(ref graphicWarpper, "graphicWarpper");
            Scribe_Values.Look(ref damageAmount, "damageAmount");
            Scribe_Values.Look(ref damageDefString, "damageDef");
            Scribe_Values.Look(ref suppressionFactor, "supressFactor");
            Scribe_Values.Look(ref stoppingPower, "stoppingPower");
            Scribe_Values.Look(ref speed, "speed");
            //Non explosive
            Scribe_Values.Look(ref armorPenetrationBlunt, "armorPenetrationBlunt");
            Scribe_Values.Look(ref armorPenetrationSharp, "armorPenetrationSharp");
            //Explosive
            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
            Scribe_Values.Look(ref gas, "gas");
            //comp
            Scribe_Collections.Look(ref fragments_save, "fragments", LookMode.Deep);
            Scribe_Collections.Look(ref secondaryDamages_save, "secondaryDamages", LookMode.Deep);
            Scribe_Deep.Look(ref secondaryExplosion_save, "secondaryExplosion");


        }

        public IEnumerable<string> ConfigError()
        {
            if (this.damageAmount == -1 && this.damageDef != null && this.damageDef.defaultDamage == -1)
            {
                //yield return "no damage amount specified for projectile";
                yield return "MP_Error_NoDamageAmount".Translate();
            }

            yield break;
        }

        public void PostLoadInit()
        {
            try
            {
                if (fragments_save != null)
                {
                    fragments.Clear();
                    foreach (var item in fragments_save)
                    {
                        fragments.Add(item.ToInstance());
                    }
                }

                if (secondaryDamages_save != null)
                {
                    secondaryDamages.Clear();
                    foreach (var item in secondaryDamages_save)
                    {
                        secondaryDamages.Add(item.ToInstance());
                    }
                }

                if (secondaryExplosion_save != null)
                {
                    secondaryExplosion = secondaryExplosion_save.ToInstance();
                }
            }
            catch (Exception e)
            {
                Log.Error($"[CeManualPatcher] Error in CustomProjectile ExposeData: {e}");
            }
        }

    }



    internal class MP_SecondaryDamage_Save : IExposable
    {
        private string damageDefString;
        public DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value?.defName ?? "null";
        }

        public int amount;

        public MP_SecondaryDamage_Save()
        {
        }
        public MP_SecondaryDamage_Save(SecondaryDamage damage)
        {
            this.amount = damage.amount;
            this.damageDef = damage.def;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref amount, "amount");
            Scribe_Values.Look(ref damageDefString, "damageDef");
        }

        public SecondaryDamage ToInstance()
        {
            return new SecondaryDamage()
            {
                amount = this.amount,
                def = this.damageDef
            };
        }
    }

    internal class MP_ThingDefCountClass_Save : IExposable
    {
        private string thingDefString;
        public ThingDef thingDef
        {
            get => DefDatabase<ThingDef>.GetNamed(thingDefString, false);
            set => thingDefString = value?.defName ?? "null";
        }
        public int count;

        public MP_ThingDefCountClass_Save()
        {
        }
        public MP_ThingDefCountClass_Save(ThingDefCountClass thingDefCount)
        {
            this.count = thingDefCount.count;
            this.thingDef = thingDefCount.thingDef;
        }
        public MP_ThingDefCountClass_Save(ThingDef thingDef, int count)
        {
            this.count = count;
            this.thingDef = thingDef;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref thingDefString, "thingDef");
        }
        public ThingDefCountClass ToInstance()
        {
            return new ThingDefCountClass(thingDef, count);
        }
    }

    internal class MP_CompProperties_ExplosiveCE_Save : IExposable
    {
        private string damageDefString;
        public DamageDef damageDef
        {
            get => DefDatabase<DamageDef>.GetNamed(damageDefString, false);
            set => damageDefString = value?.defName ?? "null";
        }

        public float amount;
        public float radius;
        public GasType? gasType;

        public MP_CompProperties_ExplosiveCE_Save()
        {
        }
        public MP_CompProperties_ExplosiveCE_Save(CompProperties_ExplosiveCE comp)
        {
            this.amount = comp.damageAmountBase;
            this.radius = comp.explosiveRadius;
            this.damageDef = comp.explosiveDamageType;
            this.gasType = comp.postExplosionGasType;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref amount, "amount");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref damageDefString, "damageDef");
            Scribe_Values.Look(ref gasType, "gasType");
        }

        public CompProperties_ExplosiveCE ToInstance()
        {
            return new CompProperties_ExplosiveCE()
            {
                explosiveDamageType = this.damageDef,
                damageAmountBase = this.amount,
                explosiveRadius = this.radius,
                postExplosionGasType = this.gasType
            };
        }

    }
}
