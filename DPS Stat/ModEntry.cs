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
        public static IMonitor StaticMonitor;

        public override void Entry(IModHelper helper) {
            // Initialize Harmony and apply patches
            StaticMonitor = Monitor;
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


            Monitor.Log("DPS Stat loaded with Harmony patch.", LogLevel.Info);
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        [HarmonyPostfix]
        private static void ModifyTooltipHeight(MeleeWeapon __instance, ref Point __result) {
            StaticMonitor.Log("Modifying tooltip height...", LogLevel.Info);
            __result.Y += 48;
        }

    [HarmonyPostfix]
        private static void ModifyItemDescription(MeleeWeapon __instance, SpriteBatch __0, ref int __1, ref int __2, SpriteFont __3, float __4, StringBuilder __5) {
            Utility.drawWithShadow(__0, Game1.mouseCursors2, new Vector2(__1 + 16 + 4, __2 + 16 + 4), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
            Utility.drawTextWithShadow(__0, "Hello", __3, new Vector2(__1 + 16 + 52, __2 + 16 + 52), Color.White);
            __2 += (int)Math.Max(__3.MeasureString("TT").Y, 48f);
            //StaticMonitor.Log("Modified tooltip", LogLevel.Info);
        }

        private void OnReturnedToTitle(object sender, EventArgs e) {
            harmony.UnpatchAll(ModManifest.UniqueID);
            Monitor.Log("Harmony patches removed", LogLevel.Info);
        }
    }
}