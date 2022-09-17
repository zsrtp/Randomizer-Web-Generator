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
        /// <summary>
        /// summary text.
        /// </summary>
        public Dictionary<Token, string> TokenDict = new();

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
            bool canUseItem = false;
            if (Randomizer.Items.heldItems.Contains(item))
            {
                canUseItem = true;
            }
            return canUseItem;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanUse(string item)
        {
            bool canUseItem = false;
            foreach (var listItem in Randomizer.Items.heldItems)
            {
                if (listItem.ToString() == item)
                {
                    canUseItem = true;
                    break;
                }
            }
            return canUseItem;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasDamagingItem()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || hasBombs()
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
                || CanUse(Item.Spinner);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool HasSword()
        {
            return getItemCount(Item.Progressive_Sword) >= 1;
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatAeralfos()
        {
            return (
                (getItemCount(Item.Progressive_Clawshot) >= 1)
                && (
                    HasSword()
                    || CanUse(Item.Ball_and_Chain)
                    || CanUse(Item.Iron_Boots)
                    || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
                || (getItemCount(Item.Progressive_Clawshot) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBabaSerpent()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBabyGohma()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || (getItemCount(Item.Progressive_Clawshot) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBari()
        {
            return CanUseWaterBombs() || (getItemCount(Item.Progressive_Clawshot) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBeamos()
        {
            return CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || hasBombs();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBigBaba()
        {
            return HasSword()
                || CanUse(Item.Ball_and_Chain)
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatChu()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatBombfish()
        {
            return (
                CanUse(Item.Iron_Boots)
                && (
                    HasSword()
                    || CanUse(Item.Ordon_Shield)
                    || CanUse(Item.Wooden_Shield)
                    || CanUse(Item.Hylian_Shield)
                    || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
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
                    || (getItemCount(Item.Progressive_Bow) >= 1)
                    || CanUse(Item.Iron_Boots)
                    || CanUse(Item.Spinner)
                    || CanUse(Item.Shadow_Crystal)
                ) && (hasBombs() || (getItemCount(Item.Progressive_Clawshot) >= 1))
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDarknut()
        {
            return HasSword();
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDekuBaba()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || (getItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || CanUse(Item.Slingshot)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDekuLike()
        {
            return (
                hasBombs()
                || HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDodongo()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || (hasShield() && (getItemCount(Item.Progressive_Hidden_Skill) >= 2))
                || CanUse(Item.Slingshot)
                || CanUse(Item.Lantern)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowBeast()
        {
            return (HasSword() || (CanUse(Item.Shadow_Crystal) && CanCompleteMDH()));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShadowBulblin()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || (getItemCount(Item.Progressive_Hidden_Skill) >= 2)
                || CanUse(Item.Slingshot)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Slingshot)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatShellBlade()
        {
            return (CanUseWaterBombs() || (HasSword() && CanUse(Item.Iron_Boots)));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatSkullfish()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStalfos()
        {
            return (canSmash());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStalhound()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                    || (getItemCount(Item.Progressive_Bow) >= 1)
                    || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Shadow_Crystal)
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
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Boomerang)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatZantHead()
        {
            return (CanUse(Item.Shadow_Crystal) || HasSword());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatOok()
        {
            return (
                HasSword()
                || CanUse(Item.Ball_and_Chain)
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDangoro()
        {
            return (
                (HasSword() || CanUse(Item.Ball_and_Chain) || CanUse(Item.Shadow_Crystal))
                && CanUse(Item.Iron_Boots)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatSkullKid()
        {
            return (getItemCount(Item.Progressive_Bow) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinBridge()
        {
            return (getItemCount(Item.Progressive_Bow) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinDesert()
        {
            return (HasSword() || CanUse(Item.Ball_and_Chain) || CanUse(Item.Shadow_Crystal));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatKingBulblinCastle()
        {
            return (HasSword() || CanUse(Item.Ball_and_Chain) || CanUse(Item.Shadow_Crystal));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatDeathSword()
        {
            return (
                (HasSword() || CanUse(Item.Ball_and_Chain) || CanUse(Item.Iron_Boots))
                && (
                    CanUse(Item.Boomerang)
                    || (getItemCount(Item.Progressive_Bow) >= 1)
                    || (getItemCount(Item.Progressive_Clawshot) >= 1)
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
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Shadow_Crystal)
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
            return (
                (
                    CanUse(Item.Boomerang)
                    || (hasBombs() && (getItemCount(Item.Progressive_Bow) >= 1))
                )
                && (
                    HasSword()
                    || CanUse(Item.Ball_and_Chain)
                    || CanUse(Item.Iron_Boots)
                    || CanUse(Item.Shadow_Crystal)
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatFyrus()
        {
            return (
                (getItemCount(Item.Progressive_Bow) >= 1) && CanUse(Item.Iron_Boots) && HasSword()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatMorpheel()
        {
            return (
                CanUse(Item.Zora_Armor)
                && CanUse(Item.Iron_Boots)
                && HasSword()
                && (getItemCount(Item.Progressive_Clawshot) >= 1)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatStallord()
        {
            return (CanUse(Item.Spinner) && HasSword());
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
            return (
                (getItemCount(Item.Progressive_Bow) >= 1)
                && (getItemCount(Item.Progressive_Dominion_Rod) >= 1)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatArgorok()
        {
            return (
                (getItemCount(Item.Progressive_Clawshot) >= 2)
                && CanUse(Item.Iron_Boots)
                && (getItemCount(Item.Progressive_Sword) >= 2)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatZant()
        {
            return (
                (getItemCount(Item.Progressive_Sword) >= 3)
                && (
                    CanUse(Item.Boomerang)
                    && (getItemCount(Item.Progressive_Clawshot) >= 1)
                    && CanUse(Item.Iron_Boots)
                    && CanUse(Item.Zora_Armor)
                    && CanUse(Item.Ball_and_Chain)
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanDefeatGanondorf()
        {
            return CanUse(Item.Shadow_Crystal)
                && (getItemCount(Item.Progressive_Sword) >= 3)
                && (getItemCount(Item.Progressive_Hidden_Skill) >= 1);
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canSmash()
        {
            return (CanUse(Item.Ball_and_Chain) || hasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canBurnWebs()
        {
            return (CanUse(Item.Ball_and_Chain) || CanUse(Item.Lantern) || hasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool hasRangedItem()
        {
            return (
                CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Slingshot)
                || (getItemCount(Item.Progressive_Bow) >= 1)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
                || CanUse(Item.Boomerang)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool hasShield()
        {
            return (
                CanUse(Item.Hylian_Shield)
                || Randomizer.Rooms.RoomDict["Kakariko Village"].ReachedByPlaythrough
                || Randomizer.Rooms.RoomDict["Castle Town"].ReachedByPlaythrough
                || (Randomizer.Rooms.RoomDict["Death Mountain Volcano"].ReachedByPlaythrough && CanDefeatGoron())
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canLaunchBombs()
        {
            return (
                (CanUse(Item.Boomerang) || (getItemCount(Item.Progressive_Bow) >= 1)) && hasBombs()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCutHangingWeb()
        {
            return (
                (getItemCount(Item.Progressive_Clawshot) >= 1)
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || CanUse(Item.Boomerang)
                || CanUse(Item.Ball_and_Chain)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canKnockDownHCPainting()
        {
            return ((getItemCount(Item.Progressive_Bow) >= 1) || hasBombs());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canBreakMonkeyCage()
        {
            return (
                HasSword()
                || CanUse(Item.Iron_Boots)
                || CanUse(Item.Spinner)
                || CanUse(Item.Ball_and_Chain)
                || CanUse(Item.Shadow_Crystal)
                || ((getItemCount(Item.Progressive_Bow) >= 1) && CanGetArrows())
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canPressMinesSwitch()
        {
            return (CanUse(Item.Ball_and_Chain) || CanUse(Item.Iron_Boots));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canFreeAllMonkeys()
        {
            return (
                canBreakMonkeyCage()
                && (
                    CanUse(Item.Lantern)
                    || ((Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysey)
                     && (hasBombs() || CanUse(Item.Iron_Boots)))
                )
                && canBurnWebs()
                && CanUse(Item.Boomerang)
                && CanDefeatBokoblin()
                && (
                    (getItemCount(Item.Forest_Temple_Small_Key) >= 4)
                    || (Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysey)
                )
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canKnockDownHangingBaba()
        {
            return (
                (getItemCount(Item.Progressive_Bow) >= 1)
                || (getItemCount(Item.Progressive_Clawshot) >= 1)
                || CanUse(Item.Boomerang)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canBreakWoodenDoor()
        {
            return (CanUse(Item.Shadow_Crystal) || HasSword() || canSmash());
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool hasBombs()
        {
            return (
                (CanUse(Item.Filled_Bomb_Bag))
                && (
                    Randomizer.Rooms.RoomDict["Kakariko Village"].ReachedByPlaythrough
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Eldin Field Water Bomb Fish Grotto"
                        ].ReachedByPlaythrough && (getItemCount(Item.Progressive_Fishing_Rod) >= 1)
                    )
                    || (
                        Randomizer.Rooms.RoomDict["Kakariko Village"].ReachedByPlaythrough
                        && Randomizer.Rooms.RoomDict["Castle Town"].ReachedByPlaythrough
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
                    Randomizer.Rooms.RoomDict["Kakariko Village"].ReachedByPlaythrough
                    || (
                        Randomizer.Rooms.RoomDict[
                            "Eldin Field Water Bomb Fish Grotto"
                        ].ReachedByPlaythrough && (getItemCount(Item.Progressive_Fishing_Rod) >= 1)
                    )
                    || (
                        Randomizer.Rooms.RoomDict["Kakariko Village"].ReachedByPlaythrough
                        && Randomizer.Rooms.RoomDict["Castle Town"].ReachedByPlaythrough
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
                canLeaveForest() || Randomizer.Rooms.RoomDict["Lost Woods"].ReachedByPlaythrough
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteIntro()
        {
            return (
                (
                    HasSword()
                    && CanUse(Item.Slingshot)
                    && (getItemCount(Item.Progressive_Fishing_Rod) >= 1)
                    && (
                        CanUse(Item.North_Faron_Woods_Gate_Key)
                        || (Randomizer.SSettings.smallKeySettings == SmallKeySettings.Keysey)
                    )
                ) || (Randomizer.SSettings.skipPrologue == true)
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool CanCompleteMDH()
        {
            return (canCompleteLakebedTemple() || (Randomizer.SSettings.skipMdh == true));
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canLeaveForest()
        {
            return (
                (
                    canCompleteForestTemple()
                    || (Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Open)
                ) && CanCompleteIntro()
            );
        }

		/// <summary>
		/// Can complete Eldin twilight
		/// </summary>
		public static bool CanCompleteEldinTwilight()
		{
			return Randomizer.SSettings.eldinTwilightCleared || (CanCompleteIntro() && canLeaveForest());
		}

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteForestTemple()
        {
            return (
                Randomizer.Rooms.RoomDict["Forest Temple Boss Room"].ReachedByPlaythrough
                && CanDefeatDiababa()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteGoronMines()
        {
            return (
                Randomizer.Rooms.RoomDict["Goron Mines Boss Room"].ReachedByPlaythrough
                && CanDefeatFyrus()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteLakebedTemple()
        {
            return (
                Randomizer.Rooms.RoomDict["Lakebed Temple Boss Room"].ReachedByPlaythrough
                && CanDefeatMorpheel()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteArbitersGrounds()
        {
            return (
                Randomizer.Rooms.RoomDict["Arbiters Grounds Boss Room"].ReachedByPlaythrough
                && CanDefeatStallord()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteSnowpeakRuins()
        {
            return (
                Randomizer.Rooms.RoomDict["Snowpeak Ruins Boss Room"].ReachedByPlaythrough
                && CanDefeatBlizzeta()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteTempleofTime()
        {
            return (
                Randomizer.Rooms.RoomDict["Temple of Time Boss Room"].ReachedByPlaythrough
                && CanDefeatArmogohma()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteCityinTheSky()
        {
            return (
                Randomizer.Rooms.RoomDict["City in The Sky Boss Room"].ReachedByPlaythrough
                && CanDefeatArgorok()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompletePalaceofTwilight()
        {
            return (
                Randomizer.Rooms.RoomDict["Palace of Twilight Boss Room"].ReachedByPlaythrough
                && CanDefeatZant()
            );
        }

        /// <summary>
        /// summary text.
        /// </summary>
        public static bool canCompleteAllDungeons()
        {
            return (
                canCompleteForestTemple()
                && canCompleteGoronMines()
                && canCompleteLakebedTemple()
                && canCompleteArbitersGrounds()
                && canCompleteSnowpeakRuins()
                && canCompleteTempleofTime()
                && canCompleteCityinTheSky()
                && canCompletePalaceofTwilight()
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

        // START OF GLITCHED LOGIC

        /// <summary>
        /// Check for sword or Back Slice (aka fake sword)
        /// </summary>
        public static bool HasSwordOrBS()
        {
            return getItemCount(Item.Progressive_Sword) >= 1
                || getItemCount(Item.Progressive_Hidden_Skill) >= 3;
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
            return getItemCount(Item.Progressive_Sky_Book) >= 1
                || HasBottle()
                || CanUse(Item.Horse_Call);
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
                && getItemCount(Item.Progressive_Hidden_Skill) >= 6;
        }

        /// <summary>
        /// Check for if you can do Map Glitch
        /// </summary>
		public static bool CanDoMapGlitch()
		{
			return CanUse(Item.Shadow_Crystal) && CanCompleteEldinTwilightGlitched();
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
			return HasSword() || HasBottle() || CanUse(Item.Boomerang)
					|| getItemCount(Item.Progressive_Clawshot) >= 1 || CanUse(Item.Lantern)
					|| getItemCount(Item.Progressive_Bow) >= 1 || CanUse(Item.Slingshot)
					|| getItemCount(Item.Progressive_Dominion_Rod) >= 1;
		}

        /// <summary>
        /// Check for if you can do Moon Boots
        /// </summary>
		public static bool CanDoMoonBoots()
		{
			return HasSword() && (CanUse(Item.Magic_Armor)
				|| CanUse(Item.Iron_Boots) && GetItemWheelSlotCount() >= 3); // Ensure you can equip something over boots
		}

        /// <summary>
        /// Check for if you can do Jump Strike Moon Boots
        /// </summary>
        public static bool CanDoJSMoonBoots()
        {
            return CanDoMoonBoots() && getItemCount(Item.Progressive_Hidden_Skill) >= 6;
        }

        /// <summary>
        /// Check for if you can do Back Slice Moon Boots
        /// </summary>
        public static bool CanDoBSMoonBoots()
        {
            return getItemCount(Item.Progressive_Hidden_Skill) >= 3 && CanUse(Item.Magic_Armor);
        }

        /// <summary>
        /// Check for if you can do Ending Blow Moon Boots
        /// </summary>
		public static bool CanDoEBMoonBoots()
		{
			return CanDoMoonBoots() && getItemCount(Item.Progressive_Hidden_Skill) >= 1 && CanUse(Item.Iron_Boots);
		}

        /// <summary>
        /// Check for if you can do The Amazing Fly Glitch™
        /// </summary>
        public static bool CanDoFlyGlitch()
        {
            return getItemCount(Item.Progressive_Fishing_Rod) >= 1 && HasHeavyMod();
        }

        /// <summary>
        /// Check for if you can swim with Water Bombs
        /// </summary>
		public static bool CanDoAirRefill()
		{
			return CanUseWaterBombs()
				&& (HasSword() || getItemCount(Item.Progressive_Clawshot) >= 1)
				&& (CanUse(Item.Magic_Armor)
				|| CanUse(Item.Iron_Boots) && GetItemWheelSlotCount() >= 3); // Ensure you can equip something over boots
		}

        /// <summary>
        /// Check for if you can do Hidden Village (glitched)
        /// </summary>
        public static bool CanDoHiddenVillageGlitched()
        {
            return getItemCount(Item.Progressive_Bow) >= 1
                || CanUse(Item.Ball_and_Chain)
                || (
                    CanUse(Item.Slingshot)
                    && (
                        CanUse(Item.Shadow_Crystal)
                        || HasSword()
                        || hasBombs()
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
			return hasBombs() || CanDoBSMoonBoots() || CanDoJSMoonBoots();
		}

		public static bool CanLeaveForestGlitched()
		{
			return
			(
				CanCompleteIntro() &&
				(
					(Randomizer.SSettings.faronWoodsLogic == FaronWoodsLogic.Open) ||
					(canCompleteForestTemple() || CanDoLJA() || CanDoMapGlitch())
				)
			);
		}

		/// <summary>
		/// Check for if Eldin twilight can be completed (glitched). Check this for if map warp can be obtained
		/// </summary>
		public static bool CanCompleteEldinTwilightGlitched()
		{
			return Randomizer.SSettings.eldinTwilightCleared || CanLeaveForestGlitched();
		}

		// END OF GLITCHED LOGIC

		public static int getItemCount(Item itemToBeCounted)
        {
            List<Item> itemList = Randomizer.Items.heldItems;
            int itemQuantity = 0;
            foreach (var item in itemList)
            {
                if (item == itemToBeCounted)
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
        public static bool verifyItemQuantity(string itemToBeCounted, int quantity)
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

        /// <summary>
        /// summary text.
        /// </summary>
        public bool EvaluateRequirements(string expression)
        {
            Parser parse = new Parser();
            parse.ParserReset();
            Randomizer.Logic.TokenDict = new Tokenizer(expression).Tokenize();
            return parse.Parse();
        }
    }
}
