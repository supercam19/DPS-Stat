using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace DPS_Stat
{
    public class ModEntry : Mod {
        private Harmony harmony;

        public override void Entry(IModHelper helper) {
            // Initialize Harmony and apply patches
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


            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        [HarmonyPostfix]
        private static void ModifyTooltipHeight(MeleeWeapon __instance, ref Point __result) {
            if (__instance is null) return;
            if (__instance.isScythe()) return;
            __result.Y += 48;
        }

        [HarmonyPostfix]
        private static void ModifyItemDescription(MeleeWeapon __instance, SpriteBatch __0, ref int __1, ref int __2, SpriteFont __3, float __4, StringBuilder __5) {
            if (__instance is null) return;
            if (__instance.isScythe()) return;
            Utility.drawWithShadow(__0, Game1.mouseCursors, new Vector2(__1 + 16 + 4, __2 + 16 + 4), new Rectangle(80, 0, 13, 13), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 1f);
            Utility.drawTextWithShadow(__0, GetDPS(__instance) + " DPS", __3, new Vector2(__1 + 16 + 52, __2 + 16 + 12), Color.Black);
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
            float avgDmg = (float)(minDamage + maxDamage) / 2;
            float avgCrit = avgDmg * critMultiplier;
            // MeleeWeapon.cs:1447 - Displayed speed is half of actual speed
            float atksPerSec = 1000f / (400 - 20 * speed) * (type == 1 ? 4 : 1);
            float avgWithCrits = avgCrit * critChance + avgDmg * (1 - critChance);
            int DPS = (int)Math.Round(avgWithCrits * atksPerSec);
            return DPS.ToString();
        }

        private void OnReturnedToTitle(object sender, EventArgs e) {
            harmony.UnpatchAll(ModManifest.UniqueID);
        }
    }
}