using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

using RimWorld;
using CeManualPatcher.Misc;
using CombatExtended;
using CeManualPatcher.Extension;
using CeManualPatcher.Saveable;
using RuntimeAudioClipLoader;

namespace CeManualPatcher.Dialogs
{
    internal class Dialog_MakeCEPatch : Window
    {

        private ModSetting_CEManualPatcher settings => Mod_CEManualPatcher.settings;

        private ThingDef thingDef;
        private CEPatcher patcher;

        //scroll
        private Vector2 scrollPosition = Vector2.zero;
        private float scrollHeight = 0f;
        public Dialog_MakeCEPatch(ThingDef thingDef)
        {
            this.thingDef = thingDef;
            this.patcher = new CEPatcher(thingDef);
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
                return new Vector2(500f, 500f);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            if (thingDef == null)
            {
                listing.Label("ThingDef is null");
                listing.End();
                return;
            }

            Text.Font = GameFont.Medium;
            listing.Label("Make CE Patch");
            Text.Font = GameFont.Small;

            WidgetsUtility.ScrollView(listing.GetRect(inRect.height - listing.CurHeight - 30f), ref scrollPosition, ref scrollHeight,
            (innerListing) =>
            {
                DrawHead(innerListing);
                DrawStat(innerListing);
                DrawVerb(innerListing);
                DrawTools(innerListing);
                DrawComps(innerListing);
            });

            Rect buttonRect = new Rect(0, 0, 100f, 30f).CenterIn(listing.GetRect(30f));
            if (Widgets.ButtonText(buttonRect, "Accept"))
            {
                this.OnAcceptKeyPressed();
            }

            listing.End();
        }

        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            Rect iconRect = rect.LeftPartPixels(30f);
            Rect labelRect = rect.RightPartPixels(rect.width - 30f);

            Widgets.ThingIcon(iconRect, thingDef);
            Widgets.Label(labelRect, thingDef.LabelCap);

            listing.Gap(listing.verticalSpacing);
        }

        private void DrawStat(Listing_Standard listing)
        {
            if (patcher.stat_patches == null)
            {
                return;
            }

            listing.GapLine(6f);
            listing.Label("Stat");

            //head
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect addRect = rect.RightPartPixels(rect.height);
            Widgets.Label(rect, "<b>Stats</b>");
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                List<StatDef> exsist = new List<StatDef>();
                foreach (var item in patcher.stat_patches)
                {
                    exsist.Add(item.StatDef);
                }

                FloatMenuUtility.MakeMenu<StatDef>(MP_Options.statDefs.Except(exsist),
                    (StatDef x) => x.LabelCap,
                    (StatDef x) => delegate ()
                    {
                        StatSaveable statSaveable = new StatSaveable()
                        {
                            StatDef = x,
                        };
                        patcher.stat_patches.Add(statSaveable);
                    });
            }

            foreach (var item in patcher.stat_patches)
            {
                StatModifier temp = new StatModifier()
                {
                    stat = item.StatDef,
                    value = item.value,
                };
                listing.StatDefX(temp, 100f, newValue =>
                {
                    item.value = newValue;
                }, () =>
                {
                    patcher.stat_patches.Remove(item);
                }, indent: 20f);
            }

            listing.Gap(listing.verticalSpacing);
        }
        private void DrawVerb(Listing_Standard listing)
        {
            if (patcher.verb_patch == null)
            {
                return;
            }

            listing.GapLine(6f);
            listing.Label("Verb");

            VerbPropertiesCESaveable verb = patcher.verb_patch;

            listing.ButtonX("recoilPattern", 100f, verb.recoilPattern.ToString(), () =>
            {
                List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                FloatMenuUtility.MakeMenu(list,
                    (RecoilPattern x) => x.ToString(),
                    (RecoilPattern x) => delegate ()
                    {
                        verb.recoilPattern = x;
                    });

            }, indent: 20f);

            listing.TextfieldX("ammoConsumedPerShotCount", 100f, verb.ammoConsumedPerShotCount, newValue =>
            {
                verb.ammoConsumedPerShotCount = newValue;
            }, indent: 20f);

            listing.TextfieldX("recoilAmount", 100f, verb.recoilAmount, newValue =>
            {
                verb.recoilAmount = newValue;
            }, indent: 20f);

            listing.TextfieldX("indirectFirePenalty", 100f, verb.indirectFirePenalty, newValue =>
            {
                verb.indirectFirePenalty = newValue;
            }, indent: 20f);

            listing.TextfieldX("circularError", 100f, verb.circularError, newValue =>
            {
                verb.circularError = newValue;
            }, indent: 20f);

            listing.TextfieldX("ticksToTruePosition", 100f, verb.ticksToTruePosition, newValue =>
            {
                verb.ticksToTruePosition = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("ejectsCasings", verb.ejectsCasings, newValue =>
            {
                verb.ejectsCasings = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("ignorePartialLoSBlocker", verb.ignorePartialLoSBlocker, newValue =>
            {
                verb.ignorePartialLoSBlocker = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("interruptibleBurst", verb.interruptibleBurst, newValue =>
            {
                verb.interruptibleBurst = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("hasStandardCommand", verb.hasStandardCommand, newValue =>
            {
                verb.hasStandardCommand = newValue;
            }, indent: 20f);

            if (verb.defaultProjectile != null)
                listing.ButtonX("defaultProjectile", 200f, verb.defaultProjectile.ToString(), () =>
                {
                    Dialog_SetDefaultProjectile dialog = new Dialog_SetDefaultProjectile(thingDef);
                    Find.WindowStack.Add(dialog);
                }, indent: 20f);
            listing.TextfieldX("warmupTime", 100f, verb.warmupTime, newValue =>
            {
                verb.warmupTime = newValue;
            }, indent: 20f);
            listing.TextfieldX("range", 100f, verb.range, newValue =>
            {
                verb.range = newValue;
            }, indent: 20f);
            listing.TextfieldX("burstShotCount", 100f, verb.burstShotCount, newValue =>
            {
                verb.burstShotCount = newValue;
            }, indent: 20f);
            listing.TextfieldX("ticksBetweenBurstShots", 100f, verb.ticksBetweenBurstShots, newValue =>
            {
                verb.ticksBetweenBurstShots = newValue;
            }, indent: 20f);
            if (verb.soundCast != null)
                listing.ButtonX("soundCast", 200f, verb.soundCast.ToString(), () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.soundCast,
                        (SoundDef x) => x.defName,
                        (SoundDef x) => delegate ()
                        {
                            verb.soundCast = x;
                        });

                }, indent: 20f);
            if (verb.soundCastTail != null)
                listing.ButtonX("soundCastTail", 200f, verb.soundCastTail.ToString(), () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.soundCastTail,
                        (SoundDef x) => x.defName,
                        (SoundDef x) => delegate ()
                        {
                            verb.soundCastTail = x;
                        });
                }, indent: 20f);
            listing.TextfieldX("muzzleFlashScale", 100f, verb.muzzleFlashScale, newValue =>
            {
                verb.muzzleFlashScale = newValue;
            }, indent: 20f);

            listing.Gap(listing.verticalSpacing);
        }
        private void DrawTools(Listing_Standard listing)
        {
            if (patcher.tool_patches == null)
            {
                return;
            }

            listing.GapLine(6f);
            //head
            Rect rect = listing.GetRect(Text.LineHeight);
            Rect addRect = rect.RightPartPixels(rect.height);
            Widgets.Label(rect, "<b>Tools</b>");
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                Find.WindowStack.Add(new Dialog_AddTool(patcher));
            }

            List<ToolCESaveable> tools = patcher.tool_patches;

            tools.RemoveWhere(x => x.needDelete);

            foreach (var item in tools)
            {
                listing.Gap(6f);

                Rect subRect = listing.GetRect(Text.LineHeight);
                Widgets.Label(subRect, $"{item.label} ({item.id})");

                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    item.needDelete = true;
                    continue;
                }

                //内容
                listing.TextfieldX("armorPenetrationBlunt", 100f, item.armorPenetrationBlunt, newValue =>
                {
                    item.armorPenetrationBlunt = newValue;
                }, indent: 20f);
                listing.TextfieldX("armorPenetrationSharp", 100f, item.armorPenetrationSharp, newValue =>
                {
                    item.armorPenetrationSharp = newValue;
                }, indent: 20f);
                listing.TextfieldX("power", 100f, item.power, newValue =>
                {
                    item.power = newValue;
                }, indent: 20f);
                listing.TextfieldX("cooldownTime", 100f, item.cooldownTime, newValue =>
                {
                    item.cooldownTime = newValue;
                }, indent: 20f);
                listing.ButtonX("linkedBodyPartsGroup", 100f, item.linkedBodyPartsGroup?.label ?? "null", () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.bodyPartGroupDefs,
                        (BodyPartGroupDef x) => $"{x.LabelCap} ({x.defName})",
                        (BodyPartGroupDef x) => delegate ()
                        {
                            item.linkedBodyPartsGroup = x;
                            Messages.Message(x.defName, MessageTypeDefOf.SilentInput);
                        });
                }, indent: 20f);
                listing.ChenkBoxX("alwaysTreatAsWeapon", item.alwaysTreatAsWeapon, newValue =>
                {
                    item.alwaysTreatAsWeapon = newValue;
                }, indent: 20f);
                listing.TextfieldX("chanceFactor", 100f, item.chanceFactor, newValue =>
                {
                    item.chanceFactor = newValue;
                }, indent: 20f);
            }
        }

        private void DrawComps(Listing_Standard listing)
        {
            //fire mode
            if (patcher.fireMode_patch!= null)
            {
                CompFireModesSaveable props = patcher.fireMode_patch;

                listing.GapLine(6f);
                listing.Label("Fire mode");

                listing.TextfieldX("aimedBurstShotCount", 100f, props.aimedBurstShotCount, newValue =>
                {
                    props.aimedBurstShotCount = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("aiUseBurstMode", props.aiUseBurstMode, newValue =>
                {
                    props.aiUseBurstMode = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("noSingleShot", props.noSingleShot, newValue =>
                {
                    props.noSingleShot = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("noSnapshot", props.noSnapshot, newValue =>
                {
                    props.noSnapshot = newValue;
                }, indent: 20f);
                listing.ButtonX("aiAimMode", 100f, props.aiAimMode.ToString(), () =>
                {
                    List<AimMode> list = Enum.GetValues(typeof(AimMode)).Cast<AimMode>().ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (AimMode x) => x.ToString(),
                        (AimMode x) => delegate ()
                        {
                            props.aiAimMode = x;
                        });
                }, indent: 20f);
            }

            //ammo user
            if (patcher.ammoUser_patch != null)
            {
                CompAmmoUserSaveable props = patcher.ammoUser_patch;

                listing.GapLine(6f);
                listing.Label("<b>Ammo User</b>");

                listing.TextfieldX("magazineSize", 100f, props.magazineSize, newValue =>
                {
                    props.magazineSize = newValue;
                }, indent: 20f);
                listing.TextfieldX("AmmoGenPerMagOverride", 100f, props.AmmoGenPerMagOverride, newValue =>
                {
                    props.AmmoGenPerMagOverride = newValue;
                }, indent: 20f);
                listing.TextfieldX("reloadTime", 100f, props.reloadTime, newValue =>
                {
                    props.reloadTime = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("reloadOneAtATime", props.reloadOneAtATime, newValue =>
                {
                    props.reloadOneAtATime = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("throwMote", props.throwMote, newValue =>
                {
                    props.throwMote = newValue;
                }, indent: 20f);
                listing.ButtonX("ammoSet", 200f, props.ammoSet.LabelCap, () =>
                {
                    Find.WindowStack.Add(new Dialog_SetDefaultProjectile(patcher));
                }, indent: 20f);
                listing.TextfieldX("loadedAmmoBulkFactor", 100f, props.loadedAmmoBulkFactor, newValue =>
                {
                    props.loadedAmmoBulkFactor = newValue;
                }, indent: 20f);

            }
        }

        public override void OnAcceptKeyPressed()
        {
            base.OnAcceptKeyPressed();

            settings.patchers.Add(this.patcher);
            patcher.ApplyCEPatch();

            this.Close();
        }
    }
}
