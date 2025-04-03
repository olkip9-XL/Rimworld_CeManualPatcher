using CeManualPatcher.Dialogs;
using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CeManualPatcher.Saveable;
using CombatExtended;
using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CeManualPatcher
{
    internal class Rect_WeaponInfo : RenderRectBase
    {
        public ThingDef curWeaponDef => WeaponManager.curWeaponDef;

        public static List<string> avaliableStatDefNames = new List<string>
        {
            //item
            "Bulk",
            "WornBulk",
            "StuffEffectMultiplierToughness",
            "ToughnessRating",

            //ranged weapon
            "ShotSpread",
            "SwayFactor",
            "SightsEfficiency",
            "AimingAccuracy",
            "ReloadSpeed",
            "MuzzleFlash",
            "MagazineCapacity",
            "AmmoGenPerMagOverride",
            "NightVisionEfficiency_Weapon",
            "TicksBetweenBurstShots",
            "BurstShotCount",
            "Recoil",
            "ReloadTime",
            "OneHandedness",
            "BipodStats"
        };

        //scroll
        private float innerHeight = 0f;
        private Vector2 scrollPosition = Vector2.zero;

        WeaponManager manager => WeaponManager.instance;
        public override void DoWindowContents(Rect rect)
        {
            if (curWeaponDef == null)
            {
                return;
            }

            WidgetsUtility.ScrollView(rect, ref scrollPosition, ref innerHeight, (listing) =>
            {
                DrawHead(listing);
                DrawStat(listing);
                DrawVebs(listing);
                DrawTools(listing);
                DrawComps(listing);
            });
        }
        private void DrawHead(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);

            //icon
            Rect rect1 = rect.LeftPartPixels(30f);
            Widgets.ThingIcon(rect1, curWeaponDef);

            //label
            Rect rect2 = rect.RightPartPixels(rect.width - 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, curWeaponDef.label);
            Text.Font = GameFont.Small;

            //reset button
            Rect rect3 = rect2.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect3, TexButton.Banish))
            {
                manager.Reset(curWeaponDef);
            }

            //add CE patch button
            Rect rect4= rect3.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(rect4, TexButton.Add))
            {
                Find.WindowStack.Add(new Dialog_MakeCEPatch(curWeaponDef));
            }

            //reset CE patch
            Rect rect5 = rect4.LeftAdjoin(rect.height);
            if (Widgets.ButtonImage(rect5, TexButton.Banish))
            {
                Mod_CEManualPatcher.settings.patchers.First(x => x.thingDef == curWeaponDef)?.ResetCEPatch();
            }
            listing.Gap(listing.verticalSpacing);
        }

        private void DrawStat(Listing_Standard listing)
        {
            listing.GapLine(6f);

            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(headRect.height);
            Widgets.Label(headRect, "<b>Stat</b>");

            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                List<StatDef> list = new List<StatDef>();
                list.AddRange(MP_Options.statDefs);
                list = list.Where(x => !curWeaponDef.statBases.Any(y => y.stat == x)).ToList();

                FloatMenuUtility.MakeMenu(list,
                    (StatDef x) => x.label,
                    (StatDef x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef).statBase.AddStat(x);
                    });
            }

            foreach (var item in curWeaponDef.statBases)
            {
                listing.StatDefX(item, 100f, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).statBase.AddStat(item.stat, newValue);
                }, () =>
                {
                    manager.GetWeaponPatch(curWeaponDef).statBase.DeleteStat(item.stat);
                }, indent: 20f);
            }
        }

        private void DrawVebs(Listing_Standard listing)
        {
            if (curWeaponDef.Verbs == null ||
                curWeaponDef.Verbs.Count == 0
                )
            {
                return;
            }

            listing.GapLine(6f);
            listing.Label("<b>VerbProperties</b>");


            if (!(curWeaponDef.Verbs[0] is VerbPropertiesCE))
            {
                listing.Label("not CE compatiable, verbClass is " + curWeaponDef.Verbs[0].verbClass.ToString());
                return;
            }

            VerbPropertiesCE props = curWeaponDef.Verbs[0] as VerbPropertiesCE;

            //test

            FieldInfoWarpper warpper = new FieldInfoWarpper(props, "ammoConsumedPerShotCount")
            {
                LabelGetter= () => "MP_ammoConsumedPerShotCount".Translate(),
            };

            FieldInfoWarpper warpper2 = new FieldInfoWarpper(props, "recoilPattern")
            {
                LabelGetter = () => "MP_recoilPattern".Translate(),
                ValueSetter = (newValue) =>
                {
                    List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                    FloatMenuUtility.MakeMenu(list,
                        (RecoilPattern x) => x.ToString(),
                        (RecoilPattern x) => delegate ()
                        {
                            typeof(VerbPropertiesCE).GetField("recoilPattern", BindingFlags.Public | BindingFlags.Instance).SetValue(props, x);
                        });
                },
                ValueLabelGetter = (obj) =>
                {
                    RecoilPattern recoilPattern = (RecoilPattern)obj;
                    return recoilPattern.ToString() + "test";
                }
            };

            FieldInfoWarpper warpper3 = new FieldInfoWarpper(props, "ejectsCasings")
            {
                LabelGetter = () => "MP_EjectsCasings".Translate(),
            };

            listing.TestField(warpper, indent:20f);
            listing.TestField(warpper2, indent:20f);
            listing.TestField(warpper3, indent:20f);

            //end

            listing.ButtonX("recoilPattern", 100f, props.recoilPattern.ToString(), () =>
            {
                List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                FloatMenuUtility.MakeMenu(list,
                    (RecoilPattern x) => x.ToString(),
                    (RecoilPattern x) => delegate ()
                    {
                        manager.GetWeaponPatch(curWeaponDef).verbProperties.recoilPattern = x;
                    });

            }, indent: 20f);

            listing.TextfieldX("ammoConsumedPerShotCount", 100f, props.ammoConsumedPerShotCount, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.ammoConsumedPerShotCount = newValue;
            }, indent: 20f);

            listing.TextfieldX("recoilAmount", 100f, props.recoilAmount, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.recoilAmount = newValue;
            }, indent: 20f);

            listing.TextfieldX("indirectFirePenalty", 100f, props.indirectFirePenalty, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.indirectFirePenalty = newValue;
            }, indent: 20f);

            listing.TextfieldX("circularError", 100f, props.circularError, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.circularError = newValue;
            }, indent: 20f);

            listing.TextfieldX("ticksToTruePosition", 100f, props.ticksToTruePosition, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.ticksToTruePosition = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("ejectsCasings", props.ejectsCasings, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.ejectsCasings = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("ignorePartialLoSBlocker", props.ignorePartialLoSBlocker, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.ignorePartialLoSBlocker = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("interruptibleBurst", props.interruptibleBurst, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.interruptibleBurst = newValue;
            }, indent: 20f);

            listing.ChenkBoxX("hasStandardCommand", props.hasStandardCommand, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.hasStandardCommand = newValue;
            }, indent: 20f);

            if (props.defaultProjectile != null)
                listing.ButtonX("defaultProjectile", 200f, props.defaultProjectile.ToString(), () =>
                {
                    Dialog_SetDefaultProjectile dialog = new Dialog_SetDefaultProjectile(curWeaponDef);
                    Find.WindowStack.Add(dialog);
                }, indent: 20f);
            listing.TextfieldX("warmupTime", 100f, props.warmupTime, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.warmupTime = newValue;
            }, indent: 20f);
            listing.TextfieldX("range", 100f, props.range, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.range = newValue;
            }, indent: 20f);
            listing.TextfieldX("burstShotCount", 100f, props.burstShotCount, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.burstShotCount = newValue;
            }, indent: 20f);
            listing.TextfieldX("ticksBetweenBurstShots", 100f, props.ticksBetweenBurstShots, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.ticksBetweenBurstShots = newValue;
            }, indent: 20f);
            if (props.soundCast != null)
                listing.ButtonX("soundCast", 200f, props.soundCast.ToString(), () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.soundCast,
                        (SoundDef x) => x.defName,
                        (SoundDef x) => delegate ()
                        {
                            manager.GetWeaponPatch(curWeaponDef).verbProperties.soundCast = x;
                        });

                }, indent: 20f);
            if (props.soundCastTail != null)
                listing.ButtonX("soundCastTail", 200f, props.soundCastTail.ToString(), () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.soundCastTail,
                        (SoundDef x) => x.defName,
                        (SoundDef x) => delegate ()
                        {
                            manager.GetWeaponPatch(curWeaponDef).verbProperties.soundCastTail = x;
                        });
                }, indent: 20f);
            listing.TextfieldX("muzzleFlashScale", 100f, props.muzzleFlashScale, newValue =>
            {
                manager.GetWeaponPatch(curWeaponDef).verbProperties.muzzleFlashScale = newValue;
            }, indent: 20f);
        }
        private void DrawTools(Listing_Standard listing)
        {
            if (curWeaponDef.tools == null)
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
                Find.WindowStack.Add(new Dialog_AddTool());
            }

            foreach (var item in curWeaponDef.tools)
            {
                listing.Gap(6f);

                Rect subRect = listing.GetRect(Text.LineHeight);
                Widgets.Label(subRect, $"{item.label} ({item.id})");

                if (!(item is ToolCE))
                {
                    listing.Label("not CE compatiable, tool class is " + item.ToString());
                    continue;
                }

                ToolCE toolCE = item as ToolCE;

                ToolCESaveable GetTool()
                {
                    return manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id);
                }

                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    //manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).needDelete = true;
                    GetTool().needDelete = true;
                    continue;
                }

                //内容
                listing.TextfieldX("armorPenetrationBlunt", 100f, toolCE.armorPenetrationBlunt, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.GetById(toolCE.id).armorPenetrationBlunt = newValue;
                }, indent: 20f);
                listing.TextfieldX("armorPenetrationSharp", 100f, toolCE.armorPenetrationSharp, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).armorPenetrationSharp = newValue;
                }, indent: 20f);
                listing.TextfieldX("power", 100f, toolCE.power, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).power = newValue;
                }, indent: 20f);
                listing.TextfieldX("cooldownTime", 100f, item.cooldownTime, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).cooldownTime = newValue;
                }, indent: 20f);
                
                DrawCapacities(listing, toolCE);
                DrawSurpriseAttack(listing, toolCE);
                listing.ButtonX("linkedBodyPartsGroup", 100f, item.linkedBodyPartsGroup?.label ?? "null", () =>
                {
                    FloatMenuUtility.MakeMenu(MP_Options.bodyPartGroupDefs,
                        (BodyPartGroupDef x) => $"{x.LabelCap} ({x.defName})",
                        (BodyPartGroupDef x) => delegate ()
                        {
                            manager.GetWeaponPatch(curWeaponDef).tools.Find(y => y.id == item.id).linkedBodyPartsGroup = x;
                            Messages.Message(x.defName, MessageTypeDefOf.SilentInput);
                        });
                }, indent: 20f);
                listing.ChenkBoxX("alwaysTreatAsWeapon", item.alwaysTreatAsWeapon, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).alwaysTreatAsWeapon = newValue;
                }, indent: 20f);
                listing.TextfieldX("chanceFactor", 100f, item.chanceFactor, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == item.id).chanceFactor = newValue;
                }, indent: 20f);
            }
        }

        private void DrawCapacities(Listing_Standard listing, ToolCE tool)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;
            Widgets.Label(rect, "capacities");

            Rect rect1= rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(MP_Options.toolCapacityDefs,
                    (ToolCapacityDef x) => x.label,
                    (ToolCapacityDef x) => delegate ()
                    {
                        GetToolPatch(tool.id).capacities.Add(x);
                    });
            }

            foreach(var item in tool.capacities)
            {
                listing.Gap(6f);
                Rect subRect = listing.GetRect(Text.LineHeight);
                subRect.x += 40f;
                subRect.width -= 40f;

                Widgets.Label(subRect, item.label);
                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    GetToolPatch(tool.id).capacities.Remove(item);
                    break;
                }
            }
        }
        private void DrawSurpriseAttack(Listing_Standard listing, ToolCE tool)
        {
            if(tool.surpriseAttack == null)
            {
                return;
            }

            Rect rect = listing.GetRect(listing.verticalSpacing);
            Widgets.Label(rect, "Surprise Attack Damage");

            Rect rect1 = rect.RightPartPixels(rect.height);
            if (Widgets.ButtonImage(rect1, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(MP_Options.allDamageDefs,
                    (DamageDef x) => x.label,
                    (DamageDef x) => delegate ()
                    {
                        ExtraDamage temp = new ExtraDamage()
                        {
                            def=x,
                            amount = 0,
                        };
                        GetToolPatch(tool.id).surpriseAttack.Add(temp);
                    });
            }

            foreach (var item in tool.surpriseAttack.extraMeleeDamages)
            {
                listing.Gap(6f);
                Rect subRect = listing.GetRect(Text.LineHeight);
                Widgets.Label(subRect, item.def.LabelCap);
                //删除按钮
                Rect removeRect = subRect.RightPartPixels(subRect.height);
                if (Widgets.ButtonImage(removeRect, TexButton.Delete))
                {
                    GetToolPatch(tool.id).surpriseAttack.Remove(item);
                    break;
                }
            }
        }

        private ToolCESaveable GetToolPatch(string id)
        {
            return manager.GetWeaponPatch(curWeaponDef).tools.Find(x => x.id == id);
        }
        private void DrawComps(Listing_Standard listing)
        {
            //CompFireModes
            if (curWeaponDef.HasComp<CompFireModes>())
            {
                CompProperties_FireModes props = curWeaponDef.GetCompProperties<CompProperties_FireModes>();

                listing.GapLine(6f);
                listing.Label("<b>Fire Mode</b>");

                listing.TextfieldX("aimedBurstShotCount", 100f, props.aimedBurstShotCount, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).fireMode.aimedBurstShotCount = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("aiUseBurstMode", props.aiUseBurstMode, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).fireMode.aiUseBurstMode = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("noSingleShot", props.noSingleShot, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).fireMode.noSingleShot = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("noSnapshot", props.noSnapshot, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).fireMode.noSnapshot = newValue;
                }, indent: 20f);
                listing.ButtonX("aiAimMode", 100f, props.aiAimMode.ToString(), () =>
                {
                    List<AimMode> list = Enum.GetValues(typeof(AimMode)).Cast<AimMode>().ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (AimMode x) => x.ToString(),
                        (AimMode x) => delegate ()
                        {
                            manager.GetWeaponPatch(curWeaponDef).fireMode.aiAimMode = x;
                        });
                }, indent: 20f);
            }

            //CompAmmoUser
            if (curWeaponDef.HasComp<CompAmmoUser>())
            {
                CompProperties_AmmoUser props = curWeaponDef.GetCompProperties<CompProperties_AmmoUser>();
                listing.GapLine(6f);
                listing.Label("<b>Ammo User</b>");

                listing.TextfieldX("magazineSize", 100f, props.magazineSize, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.magazineSize = newValue;
                }, indent: 20f);
                listing.TextfieldX("AmmoGenPerMagOverride", 100f, props.AmmoGenPerMagOverride, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.AmmoGenPerMagOverride = newValue;
                }, indent: 20f);
                listing.TextfieldX("reloadTime", 100f, props.reloadTime, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.reloadTime = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("reloadOneAtATime", props.reloadOneAtATime, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.reloadOneAtATime = newValue;
                }, indent: 20f);
                listing.ChenkBoxX("throwMote", props.throwMote, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.throwMote = newValue;
                }, indent: 20f);
                listing.ButtonX("ammoSet", 200f, props.ammoSet.LabelCap, () =>
                {
                    Dialog_SetDefaultProjectile dialog = new Dialog_SetDefaultProjectile(curWeaponDef);
                    Find.WindowStack.Add(dialog);
                }, indent: 20f);
                listing.TextfieldX("loadedAmmoBulkFactor", 100f, props.loadedAmmoBulkFactor, newValue =>
                {
                    manager.GetWeaponPatch(curWeaponDef).ammoUser.loadedAmmoBulkFactor = newValue;
                }, indent: 20f);
            }
        }
    }
}
