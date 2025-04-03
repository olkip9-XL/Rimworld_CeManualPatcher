using CeManualPatcher.Extension;
using CeManualPatcher.Manager;
using CeManualPatcher.Misc;
using CombatExtended;
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
    internal class Rect_AmmoInfo : RenderRectBase
    {
        MP_AmmoSet curAmmoSet => AmmoManager.curAmmoSet;
        AmmoManager manager => AmmoManager.instance;

        private Vector2 scrollPosition = Vector2.zero;
        private float viewHeight = 0f;

        private readonly float damageButtonWidth = 150f;

        public override void DoWindowContents(Rect rect)
        {
            if (curAmmoSet == null)
            {
                return;
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            WidgetsUtility.ScrollView(listingStandard.GetRect(rect.height - listingStandard.CurHeight), ref scrollPosition, ref viewHeight, scrollListing =>
            {
                DrawHead(scrollListing);

                foreach (var item in curAmmoSet.ammoList)
                {
                    DrawAmmo(scrollListing, item);
                }
            });

            listingStandard.End();
        }

        private void DrawHead(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label(curAmmoSet.Label);
            Text.Font = GameFont.Small;
        }

        private void DrawAmmo(Listing_Standard listing, MP_Ammo ammo)
        {

            if (ammo?.projectile?.projectile?.damageDef == null)
            {
                return;
            }

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            if (props == null)
            {
                return;
            }

            listing.GapLine();
            DrawAmmoHead(listing, ammo);

            //Explosive
            if (ammo.isExplosive)
            {
                listing.TextfieldX("Explosion Radius", 100f, props.explosionRadius, newValue =>
                {
                    manager.GetAmmoPatch(ammo.projectile).projectile.explosionRadius = newValue;
                });

                listing.ButtonX("ExplosionGas", 100f, props.postExplosionGasType.GetLabel(), () =>
                {
                    List<GasType?> list = new List<GasType?>();
                    list.Add(null);
                    list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());

                    FloatMenuUtility.MakeMenu<GasType?>(list,
                        (gas) => gas.GetLabel(),
                        (gas) => delegate
                        {
                            manager.GetAmmoPatch(ammo.projectile).projectile.postExplosionGasType = gas;
                        }
                    );
                });

                //damage
                DrawAmmoDamageEx(listing, ammo);
            }
            //Non-Explosive
            else
            {
                listing.TextfieldX("armorPenetrationSharp", 100f, props.armorPenetrationSharp, newValue =>
                {
                    manager.GetAmmoPatch(ammo.projectile).projectile.armorPenetrationSharp = newValue;
                });

                listing.TextfieldX("armorPenetrationBlunt", 100f, props.armorPenetrationBlunt, newValue =>
                {
                    manager.GetAmmoPatch(ammo.projectile).projectile.armorPenetrationBlunt = newValue;
                });

                //damage
                DrawAmmoDamageNEx(listing, ammo);
            }

            //common
            listing.TextfieldX("suprressionFactor", 100f, props.suppressionFactor, newValue =>
            {
                manager.GetAmmoPatch(ammo.projectile).projectile.suppressionFactor = newValue;
            });

            //comps
            if (ammo.projectile.HasComp<CompFragments>())
            {
                CompProperties_Fragments compProps = ammo.projectile.GetCompProperties<CompProperties_Fragments>();

                //head
                Rect headRect = listing.GetRect(Text.LineHeight);
                Rect addRect = headRect.RightPartPixels(headRect.height);

                Widgets.Label(headRect, "Fragments");
                if (Widgets.ButtonImage(addRect, TexButton.Add))
                {
                    List<ThingDef> list = MP_Options.fragments.ToList();
                    FloatMenuUtility.MakeMenu(list,
                        (x) => x.LabelCap,
                        (x) => delegate
                        {
                            manager.GetAmmoPatch(ammo.projectile).projectile.fragments.Add(new ThingDefCountClass(x, 1));
                        }
                    );
                }

                //fragments
                for (int i = 0; i < compProps.fragments.Count; i++)
                {
                    DrawFragmentsRow(listing, ammo, compProps, i);
                }
            }

            if (ammo.projectile.HasComp<CompExplosiveCE>())
            {
                CompProperties_ExplosiveCE compProps = ammo.projectile.GetCompProperties<CompProperties_ExplosiveCE>();

                listing.Label("ExplosiveCE");

                listing.DamageRow(compProps.explosiveDamageType.LabelCap, damageButtonWidth,
                    () =>
                    {
                        FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                            (x) => x.LabelCap,
                            (x) => delegate
                            {
                                manager.GetAmmoPatch(ammo.projectile).projectile.secondaryExplosion.damageDef = x;
                            }
                        );
                    }, (int)compProps.damageAmountBase, 100f,
                    (newValue) =>
                    {
                        manager.GetAmmoPatch(ammo.projectile).projectile.secondaryExplosion.damageAmountBase = newValue;
                    }, indent: 20f);

                listing.TextfieldX("explosiveRadius", 100f, compProps.explosiveRadius,
                    newValue =>
                    {
                        manager.GetAmmoPatch(ammo.projectile).projectile.secondaryExplosion.explosionRadius = newValue;
                    });
                listing.ButtonX("ExplosionGas", 100f, compProps.postExplosionGasType.GetLabel(), () =>
                {
                    List<GasType?> list = new List<GasType?>();
                    list.Add(null);
                    list.AddRange(Enum.GetValues(typeof(GasType)).Cast<GasType?>());
                    FloatMenuUtility.MakeMenu<GasType?>(list,
                        (gas) => gas.GetLabel(),
                        (gas) => delegate
                        {
                            manager.GetAmmoPatch(ammo.projectile).projectile.secondaryExplosion.postExplosionGasType = gas;
                        }
                    );
                });
            }
        }

        private void DrawAmmoHead(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect signRect = rect.LeftPartPixels(4f);
            Rect iconRect = new Rect(signRect.xMax, rect.y, rect.height, rect.height);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width - 4f, rect.height);
            Rect resetRect = labelRect.RightPartPixels(rect.height);

            if (manager.HasAmmoPatch(ammo))
            {
                Widgets.DrawBoxSolidWithOutline(signRect, new Color(85f / 256f, 177f / 256f, 85f / 256f), Color.white, 0);
            }

            Widgets.DrawTextureFitted(iconRect, ammo.Icon, 0.7f);

            Widgets.Label(labelRect, ammo.Label);

            if (Widgets.ButtonImage(resetRect, TexButton.Banish))
            {
                manager.Reset(ammo.projectile);
            }

            listing.Gap(listing.verticalSpacing);
        }

        private void DrawAmmoDamageEx(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect rect = listing.GetRect(Text.LineHeight);

            Rect fieldRect = rect.RightPartPixels(100f);
            Rect buttonRect = new Rect(fieldRect.x - 6f - damageButtonWidth, rect.y, damageButtonWidth, rect.height);

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            Widgets.Label(rect, "Damage");

            int damageAmountBase = (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props);
            WidgetsUtility.TextFieldOnChange(fieldRect, damageAmountBase, newValue =>
            {
                manager.GetAmmoPatch(ammo.projectile).projectile.damageAmountBase = newValue;
            });

            if (Widgets.ButtonText(buttonRect, props.damageDef.label))
            {
                List<DamageDef> list = DefDatabase<DamageDef>.AllDefs.Except(ammo.projectile.ExistDamages()).ToList();

                FloatMenuUtility.MakeMenu(list,
                    (x) => x.LabelCap,
                    (x) => delegate
                    {
                        manager.GetAmmoPatch(ammo.projectile).projectile.damageDef = x;
                    }
                 );
            }

            listing.Gap(listing.verticalSpacing);
        }

        private void DrawAmmoDamageNEx(Listing_Standard listing, MP_Ammo ammo)
        {
            Rect headRect = listing.GetRect(Text.LineHeight);
            Rect addRect = headRect.RightPartPixels(headRect.height);

            ProjectilePropertiesCE props = ammo.projectile.projectile as ProjectilePropertiesCE;

            Widgets.Label(headRect, "Damage");
            if (Widgets.ButtonImage(addRect, TexButton.Add))
            {
                FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                    (x) => x.LabelCap,
                    (x) => delegate
                    {
                        manager.GetAmmoPatch(ammo.projectile).projectile.secondaryDamages.Add(new SecondaryDamage()
                        {
                            def = x,
                            amount = 1
                        });
                    }
                );

            }

            //main damage
            for (int i = -1; i < props.secondaryDamage.Count; i++)
            {
                string label = i == -1 ? props.damageDef.LabelCap : props.secondaryDamage[i].def.LabelCap;
                int amount = i == -1 ? (int)typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(props) : props.secondaryDamage[i].amount;

                Action onDelete = delegate
                {
                    manager.GetAmmoPatch(ammo.projectile).projectile.secondaryDamages.RemoveAt(i);
                };

                listing.DamageRow(label, damageButtonWidth,
                    () =>
                    {
                        int index = i;
                        FloatMenuUtility.MakeMenu(AvaliableDamageDefs(ammo),
                            (x) => x.LabelCap,
                            (x) => delegate
                            {
                                if (index == -1)
                                    manager.GetAmmoPatch(ammo.projectile).projectile.damageDef = x;
                                else
                                    manager.GetAmmoPatch(ammo.projectile).projectile.secondaryDamages[i].def = x;
                            }
                        );
                    }, amount, 100f,
                    (newValue) =>
                    {
                        if (i == -1)
                            manager.GetAmmoPatch(ammo.projectile).projectile.damageAmountBase = newValue;
                        else
                            manager.GetAmmoPatch(ammo.projectile).projectile.secondaryDamages[i].amount = newValue;

                    }, i == -1 ? null : onDelete, 20f);
            }
        }
        private void DrawFragmentsRow(Listing_Standard listing, MP_Ammo ammo, CompProperties_Fragments compProps, int index)
        {
            Rect rect = listing.GetRect(Text.LineHeight);
            rect.x += 20f;
            rect.width -= 20f;

            Rect deleteRect = rect.RightPartPixels(rect.height);
            Rect fieldRect = new Rect(deleteRect.x - 6f - 100f, rect.y, 100f, rect.height);
            Rect xRect = new Rect(fieldRect.x - rect.height, rect.y, rect.height, rect.height);

            ThingDefCountClass fragment = compProps.fragments[index];

            Widgets.Label(rect, fragment.thingDef.LabelCap);

            Widgets.Label(xRect, "x");

            WidgetsUtility.TextFieldOnChange(fieldRect, fragment.count, newValue =>
            {
                manager.GetAmmoPatch(ammo.projectile).projectile.fragments[index].count = newValue;
            });

            if (Widgets.ButtonImage(deleteRect, TexButton.Delete))
            {
                manager.GetAmmoPatch(ammo.projectile).projectile.fragments.RemoveAt(index);
            }

            listing.Gap(listing.verticalSpacing);
        }

        private List<DamageDef> AvaliableDamageDefs(MP_Ammo ammo)
        {
            if (ammo.isExplosive)
            {
                return MP_Options.allDamageDefsEX.Except(ammo.projectile.ExistDamages()).ToList();
            }
            else
            {
                return MP_Options.allDamageDefs.Except(ammo.projectile.ExistDamages()).ToList();
            }
        }
    }
}
