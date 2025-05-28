using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Misc
{
    public class MP_Ammo
    {
        public ThingDef projectile;
        public ThingDef ammo;
        public string Label
        {
            get
            {
                if (this.ammo != null)
                {
                    return this.ammo.label;
                }
                else if (this.projectile != null)
                {
                    return this.projectile.label;
                }
                else
                {
                    return "null";
                }
            }
        }
        public Texture2D Icon
        {
            get
            {
                if (this.ammo != null)
                {
                    return this.ammo.uiIcon;
                }
                else if (this.projectile != null)
                {
                    return this.projectile.uiIcon;
                }
                else
                {
                    return Texture2D.whiteTexture;
                }
            }
        }

        public string DefName
        {
            get
            {

                if (this.projectile != null)
                {
                    return this.projectile.defName;
                }
                else
                {
                    return "null";
                }
            }
        }

        public string Description
        {
            get
            {

                if (this.ammo != null)
                {
                    return this.ammo.description;
                }
                else
                {
                    return "";
                }
            }
        }

        public bool isExplosive { get; private set; }

        public MP_Ammo() { }

        public MP_Ammo(ThingDef projectile, ThingDef ammo)
        {
            if (projectile == null)
                return;

            this.projectile = projectile;
            this.ammo = ammo;

            this.isExplosive = projectile.projectile != null && projectile.projectile.explosionRadius > 0;
        }

        public MP_Ammo(AmmoLink ammoLink)
        {
            this.projectile = ammoLink.projectile;
            this.ammo = ammoLink.ammo;

            this.isExplosive = projectile.projectile != null && projectile.projectile.explosionRadius > 0;
        }


    }
}
