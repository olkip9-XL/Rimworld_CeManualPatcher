using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc.CustomAmmoMisc
{
    internal class CustomAmmo : IExposable
    {
        public CustomAmmoSet parentSet;

        public AmmoProjectilePair parentPair;

        private string ammoCategoryDefString = "FullMetalJacket";
        public AmmoCategoryDef ammoCategoryDef
        {
            get=> DefDatabase<AmmoCategoryDef>.GetNamedSilentFail(ammoCategoryDefString);
            set=> ammoCategoryDefString = value?.defName ?? null;
        }

        public string label;

        public MP_GraphicWarpper graphicWarpper = new MP_GraphicWarpper()
        {
            texPath = "Things/Ammo/Rifle/FMJ",
            graphicClass = typeof(Graphic_StackCount),
        };

        public float bulk;
        public float mass;

        public string DefName
        {
            get
            {
                return $"Ammo_{parentSet.defNameBase}_{suffix}";
            }
        }
        private CustomProjectile projectile => parentPair.projectile;
        private string suffix => parentPair.suffix;

        public CustomAmmo() { }
        public CustomAmmo(CustomAmmoSet parent, string _suffix = "Default")
        {
            this.parentSet = parent;

            this.bulk = parent.baseBulk;
            this.mass = parent.baseMass;

            label = $"{parentSet.defNameBase} {_suffix}";

            graphicWarpper.graphicClass = typeof(Graphic_StackCount);
            graphicWarpper.texPath = "Things/Ammo/Rifle/FMJ";

            ammoCategoryDef = DefDatabase<AmmoCategoryDef>.GetNamedSilentFail("FullMetalJacket");
        }

        public void Export(XmlDocument doc, XmlElement root)
        {
            IEnumerable<string> errors = ConfigError();
            if(errors.Any())
            {
                foreach (string error in errors)
                {
                    Log.Error($"[CeManualPatcher] CustomAmmo {DefName} error: {error}");
                }
                return;
            }

            XmlElement ammoElement = doc.CreateElement("ThingDef");
            ammoElement.SetAttribute("Class", "CombatExtended.AmmoDef");
            ammoElement.SetAttribute("ParentName", parentSet.BaseAmmoName);
            root.AppendChild(ammoElement);

            XmlUtility.AddChildElement(doc, ammoElement, "defName", DefName);
            XmlUtility.AddChildElement(doc, ammoElement, "label", label);

            XmlElement graphicElement = doc.CreateElement("graphicData");
            ammoElement.AppendChild(graphicElement);
            XmlUtility.AddChildElement(doc, graphicElement, "texPath", graphicWarpper.texPath);
            XmlUtility.AddChildElement(doc, graphicElement, "graphicClass", graphicWarpper.graphicClass.ToString());

            //stat
            List<string> list = new List<string>();
            if (Math.Abs(bulk - parentSet.baseBulk) > float.Epsilon)
            {
                list.Add(bulk.ToString());
            }
            if (Math.Abs(mass - parentSet.baseMass) > float.Epsilon)
            {
                list.Add(mass.ToString());
            }

            if (!list.NullOrEmpty())
            {
                XmlElement statBaseElement = doc.CreateElement("statBases");
                ammoElement.AppendChild(statBaseElement);
                XmlUtility.AddChildElement(doc, statBaseElement, "Mass", mass.ToString());
                XmlUtility.AddChildElement(doc, statBaseElement, "Bulk", bulk.ToString());
            }

            XmlUtility.AddChildElement(doc, ammoElement, "ammoClass", ammoCategoryDef.defName);
            XmlUtility.AddChildElement(doc, ammoElement, "cookOffProjectile", projectile.DefName);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref bulk, "bulk");
            Scribe_Values.Look(ref mass, "mass");
            Scribe_Deep.Look(ref graphicWarpper, "graphicWarpper");
            Scribe_Values.Look(ref ammoCategoryDefString, "ammoCategoryDef", "FullMetalJacket");
        }

        private IEnumerable<string> ConfigError()
        {
            if (graphicWarpper.texPath.NullOrEmpty())
            {
                //yield return "has no texPath";
                yield return "MP_Error_NoTexturePath".Translate();
            }

            if(ammoCategoryDef == null)
            {
                //yield return "has no ammo class";
                yield return "MP_Error_NoAmmoClass".Translate();
            }

            if(Math.Abs(mass)< float.Epsilon)
            {
                //yield return "has no mass";
                yield return "MP_Error_NoMass".Translate();
            }

            if (Math.Abs(bulk) < float.Epsilon)
            {
                //yield return "has no bulk";
                yield return "MP_Error_NoBulk".Translate();
            }
            yield break;
        }
    }
}
