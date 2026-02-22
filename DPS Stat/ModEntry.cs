using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System.Diagnostics;

namespace DPS_Stat
{
    public class ModEntry : Mod {
        private Harmony harmony;
        public override void Entry(IModHelper helper) {
            harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.drawTooltip)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModifyItemDescription))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon),
                    nameof(MeleeWeapon.getExtraSpaceNeededForTooltipSpecialIcons)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModifyTooltipHeight))
            );

            TrinketStat.Init(helper, harmony);
        }

        [HarmonyPostfix]
        private static void ModifyTooltipHeight(MeleeWeapon __instance, ref Point __result) {
            if (__instance is null) return;
            if (__instance.isScythe()) return;
            __result.Y += 48;
        }

        // public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        [HarmonyPostfix]
        private static void ModifyItemDescription(MeleeWeapon __instance,
            SpriteBatch __0, ref int __1, ref int __2, SpriteFont __3, float __4, StringBuilder __5) {
            if (__instance is null) return;
            if (__instance.isScythe()) return;
            Utility.drawWithShadow(__0, Game1.mouseCursors_1_6, new Vector2(__1 + 16 + 4, __2 + 16 + 4), new Rectangle(502, 430, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
            Utility.drawTextWithShadow(__0, GetDPS(__instance) + " DPS", __3, new Vector2(__1 + 16 + 52, __2 + 16 + 12), Game1.textColor * 0.9f * __4);
            __2 += (int)Math.Max(__3.MeasureString("TT").Y, 48f);
        }

        private static string GetDPS(MeleeWeapon weapon) {
            return ComputeDPS(
                weapon.type.Value,
                weapon.minDamage.Value,
                weapon.maxDamage.Value,
                weapon.critMultiplier.Value,
                weapon.speed.Value,
                weapon.critChance.Value
            );
        }

        private static string ComputeDPS(int type, int minDamage, int maxDamage, float critMultiplier, int speed, float critChance) {
            speed = GetCappedSpeed(type, speed);
            float avgDmg = (float)(minDamage + maxDamage) / 2;
            float avgCrit = avgDmg * critMultiplier;
            // MeleeWeapon.cs:1447 - Displayed speed is half of actual speed; removes 20ms from cooldown
            float atksPerSec = 1000f / (400 - 20 * speed);
            float avgWithCrits = avgCrit * critChance + avgDmg * (1 - critChance);
            int DPS = (int)Math.Round(avgWithCrits * atksPerSec);
            return DPS.ToString();
        }

        /*
         * This function returns the max speed of a weapon considering invincibility time (limiting factor on daggers or modded weapons)
         * 
         * Daggers incur an invincibility time of 150ms, swords and clubs incur 225ms.
         * 150ms -> 12 speed
         * 225ms -> 8 speed
         * 
         * Dagger is given a base of 10 speed (200ms attack delay) thanks to experimental results from
         * https://birdhouserun.bearblog.dev/stardew-valley-weapons-dps
         * The actual dagger attack speed code is super difficult to understand, so it's hard to get a theoretical speed
         */
        private static int GetCappedSpeed(int type, int speed) {
            if (type == 1) {
                return Math.Min(13, 10 + speed);
            }

            return Math.Min(9, speed);
        }
    }
}