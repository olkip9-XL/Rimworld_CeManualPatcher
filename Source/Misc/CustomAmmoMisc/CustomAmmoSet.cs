using CombatExtended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc
{
    internal class CustomAmmoSet : IExposable
    {
        //common
        public string label;
        public string description;
        public string defNameBase;

        //reference ammo
        public List<string> referencedAmmoStr = new List<string>();
        public List<string> referencedProjectileStr = new List<string>();

        //category
        public string categoryTexPath = "UI/Icons/ThingCategories/CaliberRifle";
        private string categoryDefString = "AmmoRifles";
        public ThingCategoryDef categoryDef
        {
            get => DefDatabase<ThingCategoryDef>.GetNamed(categoryDefString, false);
            set => categoryDefString = value?.defName ?? "null";
        }

        //ammo
        public string parentAmmo = "SmallAmmoBase";

        public float baseMass = 0.013f;
        public float baseBulk = 0.02f;

        //projectile
        public MP_GraphicWarpper projectileGraphicWarpper = new MP_GraphicWarpper()
        {
            texPath = "Things/Projectile/Bullet_Small",
            graphicClass = typeof(Graphic_Single),
        };
        public float baseSpeed = 168f;

        public List<string> tradeTags = new List<string>()
        {
            "CE_AutoEnableTrade",
            "CE_AutoEnableCrafting"
        };

        public List<AmmoProjectilePair> ammoTypes = new List<AmmoProjectilePair>();

        public static string ModDefDir
        {
            get
            {
                return Path.Combine(MP_FilePath.localModDir, "Common", "Defs", "Ammo");
            }
        }
        public string AmmoSetDefName => $"AmmoSet_{defNameBase}";
        public string AmmoCategoryDefName => $"AmmoCategory_{defNameBase}";
        public string BaseAmmoName => $"Ammo_{defNameBase}_Base";
        public string BaseProjectileName => $"Bullet_{defNameBase}_Base";

        public Texture2D Icon
        {
            get
            {
                Texture2D texture = ContentFinder<Texture2D>.Get(categoryTexPath, false);

                return texture ?? BaseContent.BadTex;
            }
        }
        public string Label
        {
            get
            {
                return label ?? "No Label";
            }
        }


        public CustomAmmoSet()
        {
            string id = UnityEngine.Random.Range(100, 1000).ToString();

            this.defNameBase = $"NewAmmo_{id}A";
            this.label = "New Ammo Set " + id;
            this.description = "New Ammo Description";
            this.categoryDefString = "AmmoRifles";
            this.parentAmmo = "MediumAmmoBase";

        }
        public void ExportToFile(string dirPath = null)
        {
            IEnumerable<string> errors = ConfigError();
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    Log.Error($"[CeManualPatcher] CustomAmmoSet {AmmoSetDefName} error: {error}");
                }
                return;
            }

            if (dirPath == null)
                dirPath = ModDefDir;

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string fileName = Path.Combine(dirPath, $"{AmmoSetDefName}.xml");

            XmlElement rootElement = null;
            XmlDocument xmlDoc = XmlUtility.CreateBaseDefDoc(ref rootElement);

            //thingDef category
            CreateThingCategory();

            //AmmoSetDef
            CreateAmmoSet();

            //Ammo base
            CreateAmmoBase();

            //projectile base
            CreateProjectileBase();

            //Ammo
            foreach (var item in ammoTypes)
            {
                try
                {
                    item.ammo.Export(xmlDoc, rootElement);
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Error while exporting ammo {item.ammo.DefName} to local: {e}");
                }

                try
                {
                    item.projectile.Export(xmlDoc, rootElement);

                }
                catch (Exception e)
                {
                    MP_Log.Error($"Error while exporting projectile {item.projectile.DefName} to local: {e}");
                }

                try
                {
                    item.recipe.Export(xmlDoc, rootElement);
                }
                catch (Exception e)
                {
                    MP_Log.Error($"Error while exporting recipe {item.ammo.DefName} to local: {e}");
                }
            }

            //save
            try
            {
                xmlDoc.Save(fileName);
            }
            catch (Exception e)
            {
                Log.Error($"[CeManualPatcher] Error while saving file to {fileName}: {e}");
            }

            void CreateThingCategory()
            {
                XmlElement categoryElement = xmlDoc.CreateElement("ThingCategoryDef");
                XmlUtility.AddChildElement(xmlDoc, categoryElement, "defName", AmmoCategoryDefName);
                XmlUtility.AddChildElement(xmlDoc, categoryElement, "label", label);
                XmlUtility.AddChildElement(xmlDoc, categoryElement, "parent", categoryDef.defName);
                XmlUtility.AddChildElement(xmlDoc, categoryElement, "iconPath", categoryTexPath);
                rootElement.AppendChild(categoryElement);
            }

            void CreateAmmoSet()
            {
                XmlElement ammoSetElement = xmlDoc.CreateElement("CombatExtended.AmmoSetDef");
                rootElement.AppendChild(ammoSetElement);

                XmlUtility.AddChildElement(xmlDoc, ammoSetElement, "defName", AmmoSetDefName);
                XmlUtility.AddChildElement(xmlDoc, ammoSetElement, "label", label);

                XmlElement ammoTypesElement = xmlDoc.CreateElement("ammoTypes");
                ammoSetElement.AppendChild(ammoTypesElement);
                foreach (var ammoType in ammoTypes)
                {
                    XmlUtility.AddChildElement(xmlDoc, ammoTypesElement, ammoType.ammo.DefName, ammoType.projectile.DefName);
                }

                //referenced ammo
                if (!referencedAmmoStr.NullOrEmpty())
                {
                    if (referencedAmmoStr.Count != referencedProjectileStr.Count)
                    {
                        MP_Log.Warning($"The referenced ammo count is not match referenced projectile in ammo set: {AmmoSetDefName}, {referencedAmmoStr.Count} to {referencedProjectileStr.Count}");
                    }
                    else
                    {
                        for (int i = 0; i < referencedAmmoStr.Count; i++)
                        {
                            XmlUtility.AddChildElement(xmlDoc, ammoTypesElement, referencedAmmoStr[i], referencedProjectileStr[i]);
                        }
                    }
                }

            }

            void CreateAmmoBase()
            {
                XmlElement ammoBaseElement = xmlDoc.CreateElement("ThingDef");
                ammoBaseElement.SetAttribute("Class", "CombatExtended.AmmoDef");
                ammoBaseElement.SetAttribute("Name", BaseAmmoName);
                ammoBaseElement.SetAttribute("ParentName", parentAmmo);
                ammoBaseElement.SetAttribute("Abstract", "True");
                rootElement.AppendChild(ammoBaseElement);

                XmlUtility.AddChildElement(xmlDoc, ammoBaseElement, "description", description);

                XmlElement statBaseElement = xmlDoc.CreateElement("statBases");
                ammoBaseElement.AppendChild(statBaseElement);
                XmlUtility.AddChildElement(xmlDoc, statBaseElement, "Mass", baseMass.ToString());
                XmlUtility.AddChildElement(xmlDoc, statBaseElement, "Bulk", baseBulk.ToString());

                XmlUtility.AddChildElementList(xmlDoc, ammoBaseElement, "tradeTags", tradeTags);
                XmlUtility.AddChildElementList(xmlDoc, ammoBaseElement, "thingCategories", new List<string>()
                {
                    AmmoCategoryDefName
                });
            }

            void CreateProjectileBase()
            {
                XmlElement projectileBaseElement = xmlDoc.CreateElement("ThingDef");
                projectileBaseElement.SetAttribute("Name", BaseProjectileName);
                projectileBaseElement.SetAttribute("ParentName", "BaseBulletCE");
                projectileBaseElement.SetAttribute("Abstract", "True");
                rootElement.AppendChild(projectileBaseElement);

                XmlElement graphicElement2 = xmlDoc.CreateElement("graphicData");
                projectileBaseElement.AppendChild(graphicElement2);
                XmlUtility.AddChildElement(xmlDoc, graphicElement2, "texPath", projectileGraphicWarpper.texPath);
                XmlUtility.AddChildElement(xmlDoc, graphicElement2, "graphicClass", projectileGraphicWarpper.graphicClass.ToString());

                XmlElement projectileElement = xmlDoc.CreateElement("projectile");
                projectileElement.SetAttribute("Class", "CombatExtended.ProjectilePropertiesCE");
                projectileBaseElement.AppendChild(projectileElement);
                XmlUtility.AddChildElement(xmlDoc, projectileElement, "speed", baseSpeed.ToString());
                XmlUtility.AddChildElement(xmlDoc, projectileElement, "damageDef", "Bullet");
                XmlUtility.AddChildElement(xmlDoc, projectileElement, "dropsCasings", "true");
            }


        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref defNameBase, "defNameBase");
            Scribe_Values.Look(ref categoryTexPath, "categoryTexPath");
            Scribe_Values.Look(ref categoryDefString, "categoryDef");
            Scribe_Values.Look(ref parentAmmo, "parentAmmo");
            Scribe_Values.Look(ref baseMass, "baseMass");
            Scribe_Values.Look(ref baseBulk, "baseBulk");
            Scribe_Values.Look(ref baseSpeed, "baseSpeed");
            Scribe_Deep.Look(ref projectileGraphicWarpper, "projectileGraphicWarpper");
            Scribe_Collections.Look(ref tradeTags, "tradeTags", LookMode.Value);
            Scribe_Collections.Look(ref ammoTypes, "ammoTypes", LookMode.Deep);

            Scribe_Collections.Look(ref referencedAmmoStr, "referencedAmmo", LookMode.Value);
            Scribe_Collections.Look(ref referencedProjectileStr, "referencedProjectile", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (tradeTags == null)
                {
                    tradeTags = new List<string>();
                }

                if (ammoTypes == null)
                {
                    ammoTypes = new List<AmmoProjectilePair>();
                }
                else
                {
                    foreach (var item in ammoTypes)
                    {
                        item.ammo.parentSet = this;
                        item.projectile.parentSet = this;
                    }
                }

                if (referencedAmmoStr == null)
                {
                    referencedAmmoStr = new List<string>();
                }

                if (referencedProjectileStr == null)
                {
                    referencedProjectileStr = new List<string>();
                }
            }

        }
        private IEnumerable<string> ConfigError()
        {
            if (defNameBase.Contains(" "))
            {
                //yield return $"has space in defName";
                yield return "MP_Error_SpaceInDefName".Translate();
            }

            if (description.NullOrEmpty())
            {
                //yield return $"has no description";
                yield return "MP_Error_NoDescription".Translate();
            }

            if (label.NullOrEmpty())
            {
                //yield return $"has no label";
                yield return "MP_Error_NoLabel".Translate();
            }

            if (defNameBase.NullOrEmpty())
            {
                //yield return $"has no defName";
                yield return "MP_Error_NoDefName".Translate();
            }

            if (projectileGraphicWarpper.texPath.NullOrEmpty())
            {
                //yield return $"has no base projectile texture path";
                yield return "MP_Error_NoBaseProjectileTexturePath".Translate();
            }

            if (Math.Abs(baseMass) < float.Epsilon)
            {
                //yield return $"has no base mass";
                yield return "MP_Error_NoBaseMass".Translate();
            }

            if (Math.Abs(baseBulk) < float.Epsilon)
            {
                //yield return $"has no base bulk";
                yield return "MP_Error_NoBaseBulk".Translate();
            }

            if (ammoTypes.NullOrEmpty())
            {
                yield return "MP_Error_NoAmmoTypes".Translate();
            }

            yield break;
        }

        public void PostLoadInit()
        {
            foreach (var item in ammoTypes)
            {
                item.projectile?.PostLoadInit();
            }
        }
    }

    internal class AmmoProjectilePair : IExposable
    {
        public CustomAmmo ammo;
        public CustomProjectile projectile;
        public CustomAmmoRecipe recipe;

        public string suffix = "Default";

        public AmmoProjectilePair() { }

        public AmmoProjectilePair(CustomAmmoSet parentSet)
        {
            suffix = GenerateRandomLetters(3);

            this.ammo = new CustomAmmo(parentSet, suffix);
            this.projectile = new CustomProjectile(parentSet);
            this.recipe = new CustomAmmoRecipe(this);

            ammo.parentPair = this;
            projectile.parentPair = this;
        }
        public void ExposeData()
        {
            Scribe_Deep.Look(ref ammo, "ammo");
            Scribe_Deep.Look(ref projectile, "projectile");
            Scribe_Deep.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref suffix, "suffix");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                string str = null;
                Scribe_Values.Look(ref str, "null");
                if (str != null)
                {
                    suffix = str;
                }


                ammo.parentPair = this;
                projectile.parentPair = this;

                if (recipe == null)
                {
                    recipe = new CustomAmmoRecipe(this);
                }
                recipe.parentPair = this;
            }

        }

        string GenerateRandomLetters(int length)
        {
            char[] letters = new char[length];
            for (int i = 0; i < length; i++)
            {
                letters[i] = (char)UnityEngine.Random.Range('A', 'Z' + 1); // 生成大写字母
            }
            return new string(letters);
        }
    }

    internal class MP_GraphicWarpper : IExposable
    {
        public string texPath = "";
        public Type graphicClass = typeof(Graphic_Single);

        public MP_GraphicWarpper() { }
        public MP_GraphicWarpper(string texPath, Type graphicClass)
        {
            this.texPath = texPath;
            this.graphicClass = graphicClass;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref texPath, "texPath");
            Scribe_Values.Look(ref graphicClass, "graphicClass");
        }
    }
}
