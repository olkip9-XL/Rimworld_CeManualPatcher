using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Saveable;
using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_AddTool : Window
    {
        private string label = "New tool";
        List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();
        private float power = 0;
        private float cooldown = 0;
        private float chanceFactor = 1;
        private float armorPenetrationBlunt = 0;
        private float armorPenetrationSharp = 0;
        private BodyPartGroupDef linkedBodyPartsGroup = null;
        private bool alwaysTreatAsWeapon = false;

        private CEPatcher cePatcher;

        private WeaponManager manager => WeaponManager.instance;
        private ThingDef curWeaponDef => WeaponManager.curWeaponDef;
        public Dialog_AddTool()
        {
        }

        public Dialog_AddTool(CEPatcher cePatcher)
        {
            this.cePatcher = cePatcher;
        }
        public override void PreOpen()
        {
            this.focusWhenOpened = true;
            this.draggable = true;
            this.doCloseX = true;
            this.resizeable = true;

            this.windowRect = new Rect((UI.screenWidth - this.InitialSize.x) / 2f, (UI.screenHeight - this.InitialSize.y) / 2f, this.InitialSize.x, this.InitialSize.y).Rounded();
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400f, 350f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Gap(30f);

            listingStandard.TextX("Label", 100f, ref label);
            listingStandard.TextfieldX("armorPenetrationBlunt", 100f, armorPenetrationBlunt, newValue =>
            {
                armorPenetrationBlunt = newValue;
            });
            listingStandard.TextfieldX("armorPenetrationSharp", 100f, armorPenetrationSharp, newValue =>
            {
                armorPenetrationSharp = newValue;
            });

            DrawCapacity(listingStandard);

            listingStandard.TextfieldX("power", 100f, power, newValue =>
            {
                power = newValue;
            });
            listingStandard.TextfieldX("cooldown", 100f, cooldown, newValue =>
            {
                cooldown = newValue;
            });
            listingStandard.TextfieldX("changeFactor", 100f, chanceFactor, newValue =>
            {
                chanceFactor = newValue;
            });
            listingStandard.ButtonX("LinkedBodyPartGroup", 100f, this.linkedBodyPartsGroup?.label ?? "null", () =>
            {
                FloatMenuUtility.MakeMenu<BodyPartGroupDef>(MP_Options.bodyPartGroupDefs,
                    (BodyPartGroupDef bpg) => bpg.label,
                    (BodyPartGroupDef bpg) => delegate
                    {
                        linkedBodyPartsGroup = bpg;
                    });
            });
            listingStandard.ChenkBoxX("AlwaysTreatAsWeapon", alwaysTreatAsWeapon, newValue =>
            {
                alwaysTreatAsWeapon = newValue;
            });
            listingStandard.Gap();
            DrawControlPannel(listingStandard);
        }

        private void DrawCapacity(Listing_Standard listing)
        {
            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(Text.LineHeight);

            Widgets.Label(headRect, "Capacities");
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu<ToolCapacityDef>(MP_Options.toolCapacityDefs,
                    (ToolCapacityDef capacity) => capacity.label,
                    (ToolCapacityDef capacity) => delegate
                    {
                        capacities.Add(capacity);
                    });
            }

            foreach (var capacity in capacities)
            {
                Rect rect = listing.GetRect(Text.LineHeight);
                Rect removeRect = rect.RightPartPixels(Text.LineHeight);
                Widgets.Label(rect, capacity.label);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    capacities.Remove(capacity);
                }
            }

        }

        private void DrawControlPannel(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //rect 中间两个100f宽的Rect
            Rect leftRect = new Rect(rect.x + rect.width / 2 - 100f - 6f, rect.y, 100f, rect.height);
            Rect rightRect = new Rect(rect.x + rect.width / 2 + 6f, rect.y, 100f, rect.height);

            if (Widgets.ButtonText(leftRect, "Accept"))
            {
                OnAcceptKeyPressed();
                this.Close();
            }

            if (Widgets.ButtonText(rightRect, "Cancel"))
            {
                this.Close();
            }
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            ToolCE tool = new ToolCE()
            {
                id = cePatcher == null ? manager.GetWeaponPatch(curWeaponDef).tools.Count.ToString() : cePatcher.tool_patches.Count.ToString(),
                label = label,
                capacities = new List<ToolCapacityDef>(capacities),
                power = power,
                cooldownTime = cooldown,
                chanceFactor = chanceFactor,
                armorPenetrationBlunt = armorPenetrationBlunt,
                armorPenetrationSharp = armorPenetrationSharp,
                linkedBodyPartsGroup = linkedBodyPartsGroup,
                alwaysTreatAsWeapon = alwaysTreatAsWeapon
            };

            if (cePatcher != null)
            {
                cePatcher.tool_patches.Add(new ToolCESaveable(tool));
            }
            else
            {

                manager.GetWeaponPatch(curWeaponDef).tools.Add(new ToolCESaveable(tool));
            }
        }
    }
}
