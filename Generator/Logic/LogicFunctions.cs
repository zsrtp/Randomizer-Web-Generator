using System;
using System.Collections.Generic;
using System.Reflection;
using TPRandomizer.SSettings.Enums;

namespace TPRandomizer
{
    /// <summary>
    /// summary text.
    /// </summary>
    public class LogicFunctions
    {
        //Evaluate the tokenized settings to their respective values that are set by the settings string.

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool EvaluateSetting(string setting, string value)
        {
            PropertyInfo[] settingProperties = Randomizer.SSettings.GetType().GetProperties();
            setting = setting.Replace("Setting.", "");
            bool isEqual = false;
            foreach (PropertyInfo property in settingProperties)
            {
                var settingValue = property.GetValue(Randomizer.SSettings, null);
                if ((property.Name == setting) && (value == settingValue.ToString()))
                {
                    isEqual = true;
                }
            }
            return isEqual;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanUse(Item item)
        {
            return Randomizer.Items.heldItems.Contains(item) && CanReplenishItem(item);
        }

        public static bool CanReplenishItem(Item item)
        {
            switch (item)
            {
                case Item.Lantern:
                    return CanRefillOil();
                case Item.Progressive_Bow:
                    return CanGetArrows();
                default:
                    return true;
            }
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanUse(string item)
        {
            return CanUse(Enum.Parse<Item>(item));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanChangeTime()
        {
            if (CanUse(Item.Shadow_Crystal))
            {
                // Can change time on any stage with shadow crystal
                return true;
            }
            else
            {
                foreach (string timeStage in RoomFunctions.timeFlowStages)
                {
                    if (Randomizer.Rooms.RoomDict[timeStage].ReachedByPlaythrough)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CanWarp()
        {
            if (CanUse(Item.Shadow_Crystal))
            {
                foreach (string warpStage in RoomFunctions.WarpableStages)
                {
                    if (Randomizer.Rooms.RoomDict[warpStage].ReachedByPlaythrough)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanGetHotSpringWater()
        {
            return (
                    Randomizer.Rooms.RoomDict["Lower Kakariko Village"].ReachedByPlaythrough
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Death Mountain Elevator Lower"
                        ].ReachedByPlaythrough && CanDefeatGoron()
                    )
                ) && HasBottle();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasDamagingItem()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || HasBombs()
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Spinner);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasSword()
        {
            return CanUse(Item.Progressive_Sword);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatAeralfos()
        {
            return (
                CanUse(Item.Progressive_Clawshot)
                && (
                    HasSword()
                    || CanUse(Item.Ball_and_Chain)
                    || CanUse(Item.Shadow_Crystal)
                    || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatArmos()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUse(Item.Spinner)
                || CanUseBacksliceAsSword();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBabaSerpent()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword();
        }

        public static bool CanDefeatHangingBabaSerpent()
        {
            return (CanUse(Item.Boomerang) || CanUse(Item.Progressive_Bow))
                && LogicFunctions.CanDefeatBabaSerpent();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBabyGohma()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUseBacksliceAsSword();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBari()
        {
            return CanUseWaterBombs() || CanUse(Item.Progressive_Clawshot);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBeamos()
        {
            return CanUse(Item.Ball_and_Chain) || CanUse(Item.Progressive_Bow) || HasBombs();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBigBaba()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Spinner)
                || HasBombs()
                || CanUseBacksliceAsSword();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatChu()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBokoblin()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        public static bool CanDefeatBokoblinRed()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || ((GetItemCount(Item.Progressive_Bow) >= 3) && CanGetArrows())
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
                || (CanDoDifficultCombat() && (CanUse(Item.Iron_Boots) || CanUse(Item.Spinner)))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBombfish()
        {
            return (
                (
                    CanUse(Item.Iron_Boots)
                    || Randomizer.SSettings.logicRules == LogicRules.Glitched
                        && CanUse(Item.Magic_Armor)
                )
                && (
                    HasSword()
                    || CanUse(Item.Progressive_Clawshot)
                    || (HasShield() && GetItemCount(Item.Progressive_Hidden_Skill) >= 2)
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBombling()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Progressive_Clawshot)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBomskit()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBubble()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBulblin()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatChilfos()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Spinner)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatChuWorm()
        {
            return (
                (
                    HasSword()
                    || CanUse(Item.Ball_and_Chain)
                    || CanUse(Item.Progressive_Bow)
                    || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                    || CanUse(Item.Spinner)
                    || CanUse(Item.Shadow_Crystal)
                    || CanUseBacksliceAsSword()
                ) && (HasBombs() || CanUse(Item.Progressive_Clawshot))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDarknut()
        {
            return HasSword()
                || (CanDoDifficultCombat() && (HasBombs() || CanUse(Item.Ball_and_Chain)));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDekuBaba()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || (HasShield() && GetItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDekuLike()
        {
            return (HasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDodongo()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDinalfos()
        {
            return (HasSword() || CanUse(Item.Ball_and_Chain) || CanUse(Item.Shadow_Crystal));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFireBubble()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFireKeese()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFireToadpoli()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanUse(Item.Hylian_Shield) && GetItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || (CanDoDifficultCombat() && CanUse(Item.Shadow_Crystal))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFreezard()
        {
            return CanUse(Item.Ball_and_Chain);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatGoron()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || (HasShield() && (GetItemCount(Item.Progressive_Hidden_Skill) >= 2))
                || CanUse(Item.Slingshot)
                || (CanDoDifficultCombat() && CanUse(Item.Lantern))
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatGhoulRat()
        {
            return CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatGuay()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || (CanDoDifficultCombat() && CanUse(Item.Spinner))
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Slingshot)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatHelmasaur()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatHelmasaurus()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatIceBubble()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatIceKeese()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatPoe()
        {
            return CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKargarok()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKeese()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatLeever()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatLizalfos()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatMiniFreezard()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatMoldorm()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatPoisonMite()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Lantern)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatPuppet()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatRat()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatRedeadKnight()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowBeast()
        {
            return HasSword() || (CanUse(Item.Shadow_Crystal) && CanMidnaCharge());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowBulblin()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowDekuBaba()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || (HasShield() && GetItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Clawshot)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowInsect()
        {
            return CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowKargarok()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowKeese()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowVermin()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShellBlade()
        {
            return (
                CanUseWaterBombs()
                || (
                    HasSword()
                    && (CanUse(Item.Iron_Boots) || (CanDoNicheStuff() && CanUse(Item.Magic_Armor)))
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatSkullfish()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatSkulltula()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStalfos()
        {
            return (CanSmash());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStalhound()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStalchild()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatTektite()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatTileWorm()
        {
            return (
                (
                    HasSword()
                    || CanUse(Item.Ball_and_Chain)
                    || CanUse(Item.Progressive_Bow)
                    || CanUse(Item.Shadow_Crystal)
                    || CanUse(Item.Spinner)
                    || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                    || HasBombs()
                    || CanUseBacksliceAsSword()
                ) && CanUse(Item.Boomerang)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatToado()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatWaterToadpoli()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (HasShield() && GetItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || CanDoDifficultCombat() && (CanUse(Item.Shadow_Crystal))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatTorchSlug()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatWalltula()
        {
            return (
                CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Boomerang)
                || CanUse(Item.Progressive_Clawshot)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatWhiteWolfos()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatYoungGohma()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatZantHead()
        {
            return (CanUse(Item.Shadow_Crystal) || HasSword()) || CanUseBacksliceAsSword();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatOok()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDangoro()
        {
            return (
                (
                    HasSword()
                    || CanUse(Item.Shadow_Crystal)
                    || (
                        CanDoNicheStuff() && CanUse(Item.Ball_and_Chain)
                        || (CanUse(Item.Progressive_Bow) && HasBombs())
                    )
                ) && CanUse(Item.Iron_Boots)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatCarrierKargarok()
        {
            return CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatTwilitBloat()
        {
            return CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDekuToad()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatSkullKid()
        {
            return CanUse(Item.Progressive_Bow);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinBridge()
        {
            return CanUse(Item.Progressive_Bow);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinDesert()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Shadow_Crystal)
                || GetItemCount(Item.Progressive_Bow) > 2
                || CanUseBacksliceAsSword()
                || (
                    CanDoDifficultCombat()
                    && (
                        CanUse(Item.Spinner)
                        || CanUse(Item.Iron_Boots)
                        || HasBombs()
                        || GetItemCount(Item.Progressive_Bow) >= 2
                    )
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinCastle()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Shadow_Crystal)
                || GetItemCount(Item.Progressive_Bow) > 2
                || (
                    CanDoDifficultCombat()
                    && (
                        CanUse(Item.Spinner)
                        || CanUse(Item.Iron_Boots)
                        || HasBombs()
                        || CanUseBacksliceAsSword()
                    )
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDeathSword()
        {
            return (
                HasSword()
                && (
                    CanUse(Item.Boomerang)
                    || CanUse(Item.Progressive_Bow)
                    || CanUse(Item.Progressive_Clawshot)
                )
                && CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDarkhammer()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Progressive_Bow)
                || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || (CanDoDifficultCombat() && CanUseBacksliceAsSword())
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatPhantomZant()
        {
            return (CanUse(Item.Shadow_Crystal) || HasSword());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDiababa()
        {
            return CanLaunchBombs()
                || (
                    CanUse(Item.Boomerang)
                    && (
                        HasSword()
                        || CanUse(Item.Ball_and_Chain)
                        || (CanDoNicheStuff() && CanUse(Item.Iron_Boots))
                        || CanUse(Item.Shadow_Crystal)
                        || HasBombs()
                        || (CanDoDifficultCombat() && CanUseBacksliceAsSword())
                    )
                );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFyrus()
        {
            return (
                CanUse(Item.Progressive_Bow)
                && CanUse(Item.Iron_Boots)
                && (HasSword() || (CanDoDifficultCombat() && CanUseBacksliceAsSword()))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatMorpheel()
        {
            return (
                (
                    CanUse(Item.Zora_Armor)
                    && CanUse(Item.Iron_Boots)
                    && HasSword()
                    && CanUse(Item.Progressive_Clawshot)
                )
                || (
                    CanDoNicheStuff()
                    && (CanUse(Item.Progressive_Clawshot) && CanDoAirRefill() && HasSword())
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStallord()
        {
            return (
                (CanUse(Item.Spinner) && HasSword())
                || (CanDoDifficultCombat() && CanUse(Item.Spinner))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBlizzeta()
        {
            return CanUse(Item.Ball_and_Chain);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatArmogohma()
        {
            return (CanUse(Item.Progressive_Bow) && CanUse(Item.Progressive_Dominion_Rod));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatArgorok()
        {
            return (
                GetItemCount(Item.Progressive_Clawshot) >= 2
                && GetItemCount(Item.Progressive_Sword) >= 2
                && (CanUse(Item.Iron_Boots) || (CanDoNicheStuff() && CanUse(Item.Magic_Armor)))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatZant()
        {
            return (
                (GetItemCount(Item.Progressive_Sword) >= 3)
                && (
                    CanUse(Item.Boomerang)
                    && CanUse(Item.Progressive_Clawshot)
                    && CanUse(Item.Ball_and_Chain)
                    && (CanUse(Item.Iron_Boots) || (CanDoNicheStuff() && CanUse(Item.Magic_Armor)))
                    && (
                        CanUse(Item.Zora_Armor)
                        || (
                            Randomizer.SSettings.logicRules == LogicRules.Glitched
                            && CanDoAirRefill()
                        )
                    )
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatGanondorf()
        {
            return CanUse(Item.Shadow_Crystal)
                && (GetItemCount(Item.Progressive_Sword) >= 3)
                && CanUse(Item.Progressive_Hidden_Skill);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanSmash()
        {
            return (CanUse(Item.Ball_and_Chain) || HasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanBurnWebs()
        {
            return CanUse(Item.Lantern) || HasBombs() || CanUse(Item.Ball_and_Chain);
        }

        public static bool CanDestroyWebsWithoutLantern()
        {
            return HasBombs() || CanUse(Item.Ball_and_Chain);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasRangedItem()
        {
            return (
                CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Progressive_Clawshot)
                || CanUse(Item.Boomerang)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasShield()
        {
            return (
                CanUse(Item.Hylian_Shield)
                || (
                    Randomizer.Rooms.RoomDict["Kakariko Malo Mart"].ReachedByPlaythrough
                    && !Randomizer.SSettings.shuffleShopItems
                )
                || (
                    Randomizer.Rooms.RoomDict["Castle Town Goron House"].ReachedByPlaythrough
                    && !Randomizer.SSettings.shuffleShopItems
                )
                || Randomizer.Rooms.RoomDict["Death Mountain Hot Spring"].ReachedByPlaythrough
            );
        }

        public static bool CanUseBottledFairy()
        {
            return HasBottle() && Randomizer.Rooms.RoomDict["Lake Hylia"].ReachedByPlaythrough;
        }

        public static bool CanUseBottledFairies()
        {
            return HasBottles() && Randomizer.Rooms.RoomDict["Lake Hylia"].ReachedByPlaythrough;
        }

        public static bool CanUseOilBottle()
        {
            return CanUse(Item.Lantern) && CanUse(Item.Coro_Bottle);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanLaunchBombs()
        {
            return ((CanUse(Item.Boomerang) || CanUse(Item.Progressive_Bow)) && HasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCutHangingWeb()
        {
            return (
                CanUse(Item.Progressive_Clawshot)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Boomerang)
                || CanUse(Item.Ball_and_Chain)
            );
        }

        public static int GetPlayerHealth()
        {
            double playerHealth = 3.0; // start at 3 since we have 3 hearts.

            playerHealth = playerHealth + (GetItemCount(Item.Piece_of_Heart) * 0.2); //Pieces of heart are 1/5 of a heart.
            playerHealth = playerHealth + GetItemCount(Item.Heart_Container);

            return (int)playerHealth;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanKnockDownHCPainting()
        {
            return (
                CanUse(Item.Progressive_Bow)
                || (
                    CanDoNicheStuff()
                    && (
                        HasBombs()
                        || (HasSword() && GetItemCount(Item.Progressive_Hidden_Skill) >= 6)
                    )
                )
                || (
                    Randomizer.SSettings.logicRules == LogicRules.Glitched
                    && ((HasSword() && CanDoMoonBoots()) || CanDoBSMoonBoots())
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanBreakMonkeyCage()
        {
            return (
                HasSword()
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Shadow_Crystal)
                || HasBombs()
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Progressive_Clawshot)
                || (
                    CanDoNicheStuff()
                    && HasShield()
                    && GetItemCount(Item.Progressive_Hidden_Skill) >= 2
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanPressMinesSwitch()
        {
            return CanUse(Item.Iron_Boots) || (CanDoNicheStuff() && CanUse(Item.Ball_and_Chain));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanFreeAllMonkeys()
        {
            return (
                CanBreakMonkeyCage()
                && (
                    CanUse(Item.Lantern)
                    || (
                        (Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysy)
                        && (HasBombs() || CanUse(Item.Iron_Boots))
                    )
                )
                && CanBurnWebs()
                && CanUse(Item.Boomerang)
                && CanDefeatBokoblin()
                && (
                    (GetItemCount(Item.Forest_Temple_Small_Key) >= 4)
                    || (Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysy)
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanKnockDownHangingBaba()
        {
            return (
                CanUse(Item.Progressive_Bow)
                || CanUse(Item.Progressive_Clawshot)
                || CanUse(Item.Boomerang)
                || CanUse(Item.Slingshot)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanBreakWoodenDoor()
        {
            return (
                CanUse(Item.Shadow_Crystal) || HasSword() || CanSmash() || CanUseBacksliceAsSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasBombs()
        {
            return (
                CanUse(Item.Filled_Bomb_Bag)
                && (
                    Randomizer.Rooms.RoomDict[
                        "Kakariko Barnes Bomb Shop Lower"
                    ].ReachedByPlaythrough
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Eldin Field Water Bomb Fish Grotto"
                        ].ReachedByPlaythrough && CanUse(Item.Progressive_Fishing_Rod)
                    )
                    || Randomizer.Rooms.RoomDict["City in The Sky Entrance"].ReachedByPlaythrough
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanUseWaterBombs()
        {
            return (
                CanUse(Item.Filled_Bomb_Bag)
                && (
                    Randomizer.Rooms.RoomDict[
                        "Kakariko Barnes Bomb Shop Lower"
                    ].ReachedByPlaythrough
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Eldin Field Water Bomb Fish Grotto"
                        ].ReachedByPlaythrough && CanUse(Item.Progressive_Fishing_Rod)
                    )
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Kakariko Barnes Bomb Shop Lower"
                        ].ReachedByPlaythrough
                        && Randomizer.Rooms.RoomDict["Castle Town Malo Mart"].ReachedByPlaythrough
                    )
                )
            );
        }

        /// <summary>
        /// This is a temporary function that ensures arrows can be refilled for bow usage in Faron Woods/FT.
        /// </summary>
        public static bool CanGetArrows()
        {
            return (
                Randomizer.Rooms.RoomDict["Lost Woods"].ReachedByPlaythrough
                || (
                    CanCompleteGoronMines()
                    && Randomizer.Rooms.RoomDict["Kakariko Malo Mart"].ReachedByPlaythrough
                )
                || (
                    Randomizer.Rooms.RoomDict[
                        "Castle Town Goron House Balcony"
                    ].ReachedByPlaythrough && !Randomizer.SSettings.shuffleShopItems
                )
            );
        }

        public static bool CanRefillOil()
        {
            // Note: we need to assume the worse-case scenario that the player
            // has run out of oil when checking if they can refill the Lantern.
            // This also prevents stack overflows where we check if they can
            // refill oil in order to use Lantern in order to refill oil, etc.
            // So for going through giant webs in order to find oil refills,
            // using the Lantern is not valid.
            return (
                Randomizer.Rooms.RoomDict["North Faron Woods"].ReachedByPlaythrough
                || Randomizer.Rooms.RoomDict["South Faron Woods"].ReachedByPlaythrough
                || Randomizer.Rooms.RoomDict["Arbiters Grounds Entrance"].ReachedByPlaythrough
                || (
                    Randomizer.Rooms.RoomDict["Lake Hylia Long Cave"].ReachedByPlaythrough
                    && CanSmash()
                )
                || Randomizer.Rooms.RoomDict["Ordon Seras Shop"].ReachedByPlaythrough
                || (
                    CanCompleteGoronMines()
                    && Randomizer.Rooms.RoomDict["Lower Kakariko Village"].ReachedByPlaythrough
                    && CanChangeTime()
                )
                || (
                    Randomizer.Rooms.RoomDict["Castle Town Goron House"].ReachedByPlaythrough
                    && !Randomizer.SSettings.shuffleShopItems
                )
                || Randomizer.Rooms.RoomDict["Death Mountain Hot Spring"].ReachedByPlaythrough
                || Randomizer.Rooms.RoomDict["City in The Sky Entrance"].ReachedByPlaythrough
                || (
                    Randomizer.Rooms.RoomDict["Hyrule Castle Main Hall"].ReachedByPlaythrough
                    && CanDefeatBokoblin()
                    && CanDefeatLizalfos()
                    && (GetItemCount(Item.Progressive_Clawshot) >= 2)
                    && CanDefeatDarknut()
                )
                || (
                    Randomizer.Rooms.RoomDict["Eldin Lantern Cave"].ReachedByPlaythrough
                    && CanDestroyWebsWithoutLantern()
                    && CanDefeatChu()
                )
                || (
                    Randomizer.Rooms.RoomDict["Hyrule Castle Graveyard"].ReachedByPlaythrough
                    && CanSmash()
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompletePrologue()
        {
            return (
                (
                    Randomizer.Rooms.RoomDict["North Faron Woods"].ReachedByPlaythrough
                    && CanDefeatBokoblin()
                ) || (Randomizer.SSettings.skipPrologue == true)
            );
        }

        public static bool CanCompleteGoats1()
        {
            return (
                Randomizer.Rooms.RoomDict["Ordon Ranch"].ReachedByPlaythrough
                || CanCompletePrologue()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteMDH()
        {
            return (
                (Randomizer.SSettings.skipMdh == true)
                || (
                    CanCompleteLakebedTemple()
                    && Randomizer.Rooms.RoomDict["Castle Town South"].ReachedByPlaythrough
                )
            );
            //return (CanCompleteLakebedTemple() || (Randomizer.SSettings.skipMdh == true));
        }

        public static bool CanMidnaCharge()
        {
            return CanCompleteMDH() && CanCompleteAllTwilight();
        }

        public static bool CanStrikePedestal()
        {
            return GetItemCount(Item.Progressive_Sword) >= (int)Randomizer.SSettings.totEntrance;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanClearForest()
        {
            return (
                (
                    CanCompleteForestTemple()
                    || (Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Open)
                )
                && CanCompletePrologue()
                && CanCompleteFaronTwilight()
            );
        }

        /// <summary>
        /// Can complete Faron twilight
        /// </summary>
        public static bool CanCompleteFaronTwilight()
        {
            return Randomizer.SSettings.faronTwilightCleared
                || (
                    CanCompletePrologue()
                    && Randomizer.Rooms.RoomDict["South Faron Woods"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict[
                        "Faron Woods Coros House Lower"
                    ].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict[
                        "Mist Area Near Faron Woods Cave"
                    ].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["North Faron Woods"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Ordon Spring"].ReachedByPlaythrough
                    && (
                        !Randomizer.SSettings.bonksDoDamage
                        || (
                            Randomizer.SSettings.bonksDoDamage
                            && (
                                (
                                    Randomizer.SSettings.damageMagnification
                                    != DamageMagnification.OHKO
                                ) || CanUseBottledFairies()
                            )
                        )
                    )
                );
        }

        /// <summary>
        /// Can complete Eldin twilight
        /// </summary>
        public static bool CanCompleteEldinTwilight()
        {
            return Randomizer.SSettings.eldinTwilightCleared
                || (
                    Randomizer.Rooms.RoomDict["Faron Field"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Lower Kakariko Village"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Kakariko Graveyard"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Kakariko Malo Mart"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict[
                        "Kakariko Barnes Bomb Shop Upper"
                    ].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict[
                        "Kakariko Renados Sanctuary Basement"
                    ].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Kakariko Elde Inn"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Kakariko Bug House"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Upper Kakariko Village"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Kakariko Watchtower"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Death Mountain Volcano"].ReachedByPlaythrough
                    && (
                        !Randomizer.SSettings.bonksDoDamage
                        || (
                            Randomizer.SSettings.bonksDoDamage
                            && (
                                (
                                    Randomizer.SSettings.damageMagnification
                                    != DamageMagnification.OHKO
                                ) || CanUseBottledFairies()
                            )
                        )
                    )
                );
        }

        public static bool CanCompleteLanayruTwilight()
        {
            return Randomizer.SSettings.lanayruTwilightCleared
                || (
                    (
                        Randomizer.Rooms.RoomDict["North Eldin Field"].ReachedByPlaythrough
                        || CanUse(Item.Shadow_Crystal)
                    )
                    && Randomizer.Rooms.RoomDict["Zoras Domain"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Zoras Domain Throne Room"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Upper Zoras River"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Lake Hylia"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Lake Hylia Lanayru Spring"].ReachedByPlaythrough
                    && Randomizer.Rooms.RoomDict["Castle Town South"].ReachedByPlaythrough
                    && (
                        !Randomizer.SSettings.bonksDoDamage
                        || (
                            Randomizer.SSettings.bonksDoDamage
                            && (
                                (
                                    Randomizer.SSettings.damageMagnification
                                    != DamageMagnification.OHKO
                                ) || CanUseBottledFairies()
                            )
                        )
                    )
                );
        }

        public static bool CanWarpMeteor()
        {
            return CanCompleteLanayruTwilight()
                || (
                    CanCompleteEldinTwilight()
                    && Randomizer.Rooms.RoomDict["Zoras Domain Throne Room"].ReachedByPlaythrough
                    && CanUse(Item.Shadow_Crystal)
                );
        }

        public static bool CanCompleteAllTwilight()
        {
            return (
                CanCompleteFaronTwilight()
                && CanCompleteEldinTwilight()
                && CanCompleteLanayruTwilight()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteForestTemple()
        {
            return CanUse(Item.Diababa_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteGoronMines()
        {
            return CanUse(Item.Fyrus_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteLakebedTemple()
        {
            return CanUse(Item.Morpheel_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteArbitersGrounds()
        {
            return CanUse(Item.Stallord_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteSnowpeakRuins()
        {
            return CanUse(Item.Blizzeta_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteTempleofTime()
        {
            return CanUse(Item.Armogohma_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteCityinTheSky()
        {
            return CanUse(Item.Argorok_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompletePalaceofTwilight()
        {
            return CanUse(Item.Zant_Defeated);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteAllDungeons()
        {
            return (
                CanCompleteForestTemple()
                && CanCompleteGoronMines()
                && CanCompleteLakebedTemple()
                && CanCompleteArbitersGrounds()
                && CanCompleteSnowpeakRuins()
                && CanCompleteTempleofTime()
                && CanCompleteCityinTheSky()
                && CanCompletePalaceofTwilight()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasBug()
        {
            foreach (Item bug in Randomizer.Items.goldenBugs)
            {
                if (CanUse(bug))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockOrdonaMap()
        {
            if (Randomizer.SSettings.openMap)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.OrdonaMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockFaronMap()
        {
            if (Randomizer.SSettings.openMap)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.FaronMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockEldinMap()
        {
            if (Randomizer.SSettings.openMap)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.EldinMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockLanayruMap()
        {
            if (Randomizer.SSettings.openMap)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.LanayruMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockSnowpeakMap()
        {
            if (Randomizer.SSettings.openMap || Randomizer.SSettings.skipSnowpeakEntrance)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.SnowpeakMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanUnlockGerudoMap()
        {
            if (Randomizer.SSettings.openMap)
            {
                return true;
            }
            foreach (string mapRoom in RoomFunctions.GerudoMapRooms)
            {
                if (Randomizer.Rooms.RoomDict[mapRoom].ReachedByPlaythrough)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks the setting for difficult combat. Difficult combat includes: difficult, annoying, or time consuming combat
        /// </summary>
        public static bool CanDoDifficultCombat()
        {
            // TODO: Change to use setting once it's made
            return false;
        }

        /// <sumamry>
        /// Checks the setting for niche stuff. Niche stuff includes things that may not be obvious to most players, such as damaging enemies with boots, lantern on Gorons, drained Magic Armor for heavy mod, etc.
        /// </summary>
        public static bool CanDoNicheStuff()
        {
            // TODO: Change to use setting once it's made
            return Randomizer.SSettings.logicRules == LogicRules.Glitched;
        }

        public static bool CanUseBacksliceAsSword()
        {
            return CanDoNicheStuff() && GetItemCount(Item.Progressive_Hidden_Skill) >= 3;
        }

        public static bool CanGetBugWithLantern()
        {
            // TODO: If option to not have bug models replaced becomes a thing, this function can be useful
            return false;
        }

        public static bool CanBreakHCBarrier()
        {
            switch (Randomizer.SSettings.castleRequirements)
            {
                case CastleRequirements.Open:
                {
                    return true;
                }
                case CastleRequirements.Fused_Shadows:
                {
                    return GetItemCount(Item.Progressive_Fused_Shadow)
                        >= Randomizer.SSettings.castleRequirementCount;
                }
                case CastleRequirements.Mirror_Shards:
                {
                    return GetItemCount(Item.Progressive_Mirror_Shard)
                        >= Randomizer.SSettings.castleRequirementCount;
                }
                case CastleRequirements.Dungeons:
                {
                    int dungeonCount = 0;
                    foreach (Item boss in Randomizer.Items.BossItems)
                    {
                        if (CanUse(boss))
                        {
                            dungeonCount++;
                        }
                    }
                    return dungeonCount >= Randomizer.SSettings.castleRequirementCount;
                }
                case CastleRequirements.Vanilla:
                {
                    return CanCompletePalaceofTwilight();
                }
                case CastleRequirements.Poe_Souls:
                {
                    return GetItemCount(Item.Poe_Soul)
                        >= Randomizer.SSettings.castleRequirementCount;
                }
                case CastleRequirements.Hearts:
                {
                    return GetPlayerHealth() >= Randomizer.SSettings.castleRequirementCount;
                }
            }

            return false;
        }

        public static bool CanOpenHCBKGate()
        {
            switch (Randomizer.SSettings.castleBKRequirements)
            {
                case CastleBKRequirements.None:
                {
                    return true;
                }
                case CastleBKRequirements.Fused_Shadows:
                {
                    return GetItemCount(Item.Progressive_Fused_Shadow)
                        >= Randomizer.SSettings.castleBKRequirementCount;
                }
                case CastleBKRequirements.Mirror_Shards:
                {
                    return GetItemCount(Item.Progressive_Mirror_Shard)
                        >= Randomizer.SSettings.castleBKRequirementCount;
                }
                case CastleBKRequirements.Dungeons:
                {
                    int dungeonCount = 0;
                    foreach (Item boss in Randomizer.Items.BossItems)
                    {
                        if (CanUse(boss))
                        {
                            dungeonCount++;
                        }
                    }
                    return dungeonCount >= Randomizer.SSettings.castleBKRequirementCount;
                }
                case CastleBKRequirements.Poe_Souls:
                {
                    return GetItemCount(Item.Poe_Soul)
                        >= Randomizer.SSettings.castleBKRequirementCount;
                }
                case CastleBKRequirements.Hearts:
                {
                    return GetPlayerHealth() >= Randomizer.SSettings.castleBKRequirementCount;
                }
            }

            return false;
        }

        public static bool CanBuyMagicArmor()
        {
            switch (Randomizer.SSettings.walletSize)
            {
                case WalletSize.Large:
                    return true;
                case WalletSize.Reduced:
                    return GetItemCount(Item.Progressive_Wallet) >= 2;
                default:
                    return CanUse(Item.Progressive_Wallet);
            }
        }

        // START OF GLITCHED LOGIC

        /// <summary>
        /// Check for sword or Back Slice (aka fake sword)
        /// </summary>
        public static bool HasSwordOrBS()
        {
            return CanUse(Item.Progressive_Sword)
                || GetItemCount(Item.Progressive_Hidden_Skill) >= 3;
        }

        /// <summary>
        /// Check for a usable bottle (requires lantern to avoid issues with lantern oil in all bottles)
        /// </summary>
        public static bool HasBottle()
        {
            return (
                    CanUse(Item.Empty_Bottle)
                    || CanUse(Item.Sera_Bottle)
                    || CanUse(Item.Jovani_Bottle)
                    || CanUse(Item.Coro_Bottle)
                ) && CanUse(Item.Lantern);
        }

        public static bool HasBottles()
        {
            int n = 0;
            if (CanUse(Item.Lantern))
            {
                if (CanUse(Item.Empty_Bottle))
                {
                    n++;
                }
                if (CanUse(Item.Sera_Bottle))
                {
                    n++;
                }
                if (CanUse(Item.Jovani_Bottle))
                {
                    n++;
                }
                if (CanUse(Item.Coro_Bottle))
                {
                    n++;
                }

                if (n > 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check for heavy mod (boots or MA)
        /// </summary>
        public static bool HasHeavyMod()
        {
            return CanUse(Item.Iron_Boots) || CanUse(Item.Magic_Armor);
        }

        /// <summary>
        /// Check for cutscene item (useful for cutscene dropping a bomb in specific spot)
        /// </summary>
        public static bool HasCutsceneItem()
        {
            return CanUse(Item.Progressive_Sky_Book) || HasBottle() || CanUse(Item.Horse_Call);
        }

        /// <summary>
        /// Check for if you can do LJAs
        /// </summary>
        public static bool CanDoLJA()
        {
            return HasSword() && CanUse(Item.Boomerang);
        }

        /// <summary>
        /// Check for if you can do Jump Strike LJAs
        /// </summary>
        public static bool CanDoJSLJA()
        {
            return HasSword()
                && CanUse(Item.Boomerang)
                && GetItemCount(Item.Progressive_Hidden_Skill) >= 6;
        }

        /// <summary>
        /// Check for if you can do Map Glitch
        /// </summary>
        public static bool CanDoMapGlitch()
        {
            return CanUse(Item.Shadow_Crystal)
                && Randomizer.Rooms.RoomDict["Kakariko Gorge"].ReachedByPlaythrough;
        }

        /// <summary>
        /// Check for if you can do storage (aka Reverse Door Adventure (RDA)). Note: Needs a one-handed item
        /// </summary>
        public static bool CanDoStorage()
        {
            return CanDoMapGlitch() && HasOneHandedItem();
        }

        /// <summary>
        /// Check for if you have any one-handed item
        /// </summary>
        public static bool HasOneHandedItem()
        {
            return HasSword()
                || HasBottle()
                || CanUse(Item.Boomerang)
                || CanUse(Item.Progressive_Clawshot)
                || CanUse(Item.Lantern)
                || CanUse(Item.Progressive_Bow)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Progressive_Dominion_Rod);
        }

        /// <summary>
        /// Check for if you can do Moon Boots
        /// </summary>
        public static bool CanDoMoonBoots()
        {
            return HasSword()
                && (
                    CanUse(Item.Magic_Armor)
                    || CanUse(Item.Iron_Boots) && GetItemWheelSlotCount() >= 3
                ); // Ensure you can equip something over boots
        }

        /// <summary>
        /// Check for if you can do Jump Strike Moon Boots
        /// </summary>
        public static bool CanDoJSMoonBoots()
        {
            return CanDoMoonBoots() && GetItemCount(Item.Progressive_Hidden_Skill) >= 6;
        }

        /// <summary>
        /// Check for if you can do Back Slice Moon Boots
        /// </summary>
        public static bool CanDoBSMoonBoots()
        {
            return GetItemCount(Item.Progressive_Hidden_Skill) >= 3 && CanUse(Item.Magic_Armor);
        }

        /// <summary>
        /// Check for if you can do Ending Blow Moon Boots
        /// </summary>
        public static bool CanDoEBMoonBoots()
        {
            return CanDoMoonBoots()
                && CanUse(Item.Progressive_Hidden_Skill)
                && GetItemCount(Item.Progressive_Sword) >= 2;
        }

        /// <summary>
        /// Check for if you can do Helm Splitter Moon Boots
        /// </summary>
        public static bool CanDoHSMoonBoots()
        {
            return CanDoMoonBoots()
                && GetItemCount(Item.Progressive_Hidden_Skill) >= 4
                && HasSword()
                && HasShield();
        }

        /// <summary>
        /// Check for if you can do The Amazing Fly Glitch™
        /// </summary>
        public static bool CanDoFlyGlitch()
        {
            return CanUse(Item.Progressive_Fishing_Rod) && HasHeavyMod();
        }

        /// <summary>
        /// Check for if you can swim with Water Bombs
        /// </summary>
        public static bool CanDoAirRefill()
        {
            return CanUseWaterBombs()
                && (
                    CanUse(Item.Magic_Armor)
                    || (CanUse(Item.Iron_Boots) && (GetItemWheelSlotCount() >= 3))
                ); // Ensure you can equip something over boots
        }

        /// <summary>
        /// Check for if you can do Hidden Village (glitched)
        /// </summary>
        public static bool CanDoHiddenVillageGlitched()
        {
            return CanUse(Item.Progressive_Bow)
                || CanUse(Item.Ball_and_Chain)
                || (
                    CanUse(Item.Slingshot)
                    && (
                        CanUse(Item.Shadow_Crystal)
                        || HasSword()
                        || HasBombs()
                        || CanUse(Item.Iron_Boots)
                        || CanUse(Item.Spinner)
                    )
                );
        }

        /// <summary>
        /// Check for if you can get passed FT windless bridge room (glitched)
        /// </summary>
        public static bool CanDoFTWindlessBridgeRoom()
        {
            return HasBombs() || CanDoBSMoonBoots() || CanDoJSMoonBoots();
        }

        public static bool CanClearForestGlitched()
        {
            return (
                CanCompletePrologue()
                && (
                    (Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Open)
                    || (CanCompleteForestTemple() || CanDoLJA() || CanDoMapGlitch())
                )
            );
        }

        /// <summary>
        /// Check for if Eldin twilight can be completed (glitched). Check this for if map warp can be obtained
        /// </summary>
        public static bool CanCompleteEldinTwilightGlitched()
        {
            return Randomizer.SSettings.eldinTwilightCleared || CanClearForestGlitched();
        }

        /// <summary>
        /// Check for if you need the key for getting to Lakebed Deku Toad
        ///
        public static bool CanSkipKeyToDekuToad()
        {
            return Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysy
                || GetItemCount(Item.Progressive_Hidden_Skill) >= 3
                || CanDoBSMoonBoots()
                || CanDoJSMoonBoots()
                || CanDoLJA()
                || (
                    HasBombs()
                    && (HasHeavyMod() || GetItemCount(Item.Progressive_Hidden_Skill) >= 6)
                );
        }

        // END OF GLITCHED LOGIC

        public static int GetItemCount(Item itemToBeCounted)
        {
            List<Item> itemList = Randomizer.Items.heldItems;
            int itemQuantity = 0;
            foreach (var item in itemList)
            {
                if ((item == itemToBeCounted) && CanReplenishItem(itemToBeCounted))
                {
                    itemQuantity++;
                }
            }
            return itemQuantity;
        }

        public static int GetItemWheelSlotCount()
        {
            int count = 0;

            foreach (Item item in Randomizer.Items.ItemWheelItems)
            {
                if (CanUse(item))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool VerifyItemQuantity(string itemToBeCounted, int quantity)
        {
            List<Item> itemList = Randomizer.Items.heldItems;
            int itemQuantity = 0;
            bool isQuantity = false;

            foreach (var item in itemList)
            {
                if (item.ToString() == itemToBeCounted)
                {
                    itemQuantity++;
                }
            }
            if (itemQuantity >= quantity)
            {
                isQuantity = true;
            }
            return isQuantity;
        }
    }
}
