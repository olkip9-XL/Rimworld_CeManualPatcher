using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.RenderRect;
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
    internal class Dialog_AddAmmoReference : Window
    {
        List<string> referencedAmmo;
        List<string> referencedProjectiles;

        private Rect_AmmoList rect_AmmoList = new Rect_AmmoList();

        public Dialog_AddAmmoReference(List<string> _refrencedAmmo, List<string> _referencedProjectile)
        {
            this.referencedAmmo = _refrencedAmmo;
            this.referencedProjectiles = _referencedProjectile;
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

            try
            {
                rect_AmmoList.DoWindowContents(topRect);
            }
            catch (Exception e)
            {
                MP_Log.Error("Dialog_SetDefaultProjectile ammo list error", e);
            }

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

            MP_AmmoSet ammoSet = AmmoManager.curAmmoSet;

            if (this.referencedAmmo != null && ammoSet != null)
            {
                foreach (var item in ammoSet.ammoList)
                {
                    if (item.ammo == null || item.projectile == null)
                    {
                        MP_Log.Error($"Must use ce ammo");
                    }

                    referencedAmmo.AddDistinct(item.ammo.defName);
                    referencedProjectiles.AddDistinct(item.projectile.defName);
                }
            }

            this.Close();
        }

    }
}
