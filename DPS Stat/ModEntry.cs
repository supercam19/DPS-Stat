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
    public class ModEntry : Mod
    {
        private Harmony harmony;
        public static IMonitor StaticMonitor;

        public override void Entry(IModHelper helper) {
            // Initialize Harmony and apply patches
            harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new[] {typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof(Texture2D), typeof(Rectangle), typeof(Color), typeof(Color), typeof(float), typeof(int), typeof(int)}),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModifyItemDescription))
            );
            StaticMonitor = Monitor;

            Monitor.Log("DPS Stat loaded with Harmony patch.", LogLevel.Info);
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }
        
        [HarmonyPrefix]
        private static void ModifyItemDescription(MeleeWeapon __instance, ref string[] __8, ref Item __9) {
            if (__9 == null || __9 is not MeleeWeapon) {
                return;
            }
            string[] altered;
            if (__8 != null) {
                altered = new string[__8.Length + 1];
                for (int i = 0; i < __8.Length; i++) {
                    altered[i] = __8[i];
                }
            }
            else {
                altered = new string[1];
            }

            altered[altered.Length - 1] = "1";
            __8 = altered;

            
            StaticMonitor.Log("Modified buffs array", LogLevel.Info);
        }

        private void OnReturnedToTitle(object sender, EventArgs e) {
            harmony.UnpatchAll(ModManifest.UniqueID);
            Monitor.Log("Harmony patches removed", LogLevel.Info);
        }
    }
}