using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.RenderRect.Ammo;
using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_SetDefaultProjectile : Window
    {

        VerbPropertiesCE verbProperties;
        CompProperties_AmmoUser ammoUser;

        private Rect_AmmoList rect_AmmoList = new Rect_AmmoList();
        public Dialog_SetDefaultProjectile(VerbPropertiesCE verbProps, CompProperties_AmmoUser compProps)
        {
            this.verbProperties = verbProps;
            this.ammoUser = compProps;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(300f, 700f);
            }
        }

        public override void PreOpen()
        {
            this.focusWhenOpened = true;
            this.draggable = true;
            this.doCloseX = true;
            this.resizeable = true;
            this.windowRect = new Rect((UI.screenWidth - this.InitialSize.x) / 2f, (UI.screenHeight - this.InitialSize.y) / 2f, this.InitialSize.x, this.InitialSize.y).Rounded();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect topRect = inRect.TopPartPixels(inRect.height - 35f);
            Rect bottomRect = inRect.BottomPartPixels(35f);

            rect_AmmoList.DoWindowContents(topRect);

            Rect buttonRect = new Rect(0, 0, 100f, 30f);
            buttonRect = buttonRect.CenteredOnXIn(bottomRect);
            buttonRect = buttonRect.CenteredOnYIn(bottomRect);
            if (Widgets.ButtonText(buttonRect, "Accept"))
            {
                this.OnAcceptKeyPressed();
                this.Close();
            }
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            if(verbProperties != null)
            {
                verbProperties.defaultProjectile = AmmoManager.curAmmoSet.ammoList[0].projectile;
            }

            if(ammoUser != null)
            {
                ammoUser.ammoSet = AmmoManager.curAmmoSet.ammoSetDef;

                if(ammoUser.ammoSet == null && verbProperties != null)
                    verbProperties.verbClass = typeof(Verb_Shoot);
            }

            this.Close();
        }
      
    }
}
