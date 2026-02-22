using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects.Trinkets;
using StardewValley.Projectiles;

namespace DPS_Stat;

public class TrinketStat {
    public static void Init(IModHelper helper, Harmony harmony) {
        harmony.Patch(
            original: AccessTools.Method(typeof(Item), nameof(Item.drawTooltip)),
            postfix: new HarmonyMethod(typeof(TrinketStat), nameof(AppendStat))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Item), nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons)),
            postfix: new HarmonyMethod(typeof(TrinketStat), nameof(ModifyTooltipHeight))
        );
    }

    // public virtual void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
   [HarmonyPostfix]
    private static void AppendStat(Item __instance, SpriteBatch __0, ref int __1, 
        ref int __2, SpriteFont __3, float __4) {
        if (__instance is Trinket t) {
            if (t.GetEffect() is IceOrbTrinketEffect iceOrb) {
                Utility.drawWithShadow(__0, Game1.objectSpriteSheet, new Vector2(__1 + 16 + 4, __2 + 16 + 4), new Rectangle(65, 577, 15, 15), Color.White, 0f, Vector2.Zero, 40f/15f, flipped: false, 1f);
                Utility.drawTextWithShadow(__0, (iceOrb.FreezeTime / iceOrb.ProjectileDelay).ToString("P0"), __3, new Vector2(__1 + 16 + 52, __2 + 16 + 12), Game1.textColor * 0.9f * __4);
            }
            else if (t.GetEffect() is MagicQuiverTrinketEffect quiver) {
                Utility.drawWithShadow(__0, Game1.mouseCursors_1_6, new Vector2(__1 + 16 + 4, __2 + 16 + 4), new Rectangle(502, 430, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
                Utility.drawTextWithShadow(__0, $"{Math.Round((quiver.MinDamage + quiver.MaxDamage) * 500 / quiver.ProjectileDelay)} DPS", __3, new Vector2(__1 + 16 + 52, __2 + 16 + 12), Game1.textColor * 0.9f * __4);
            }
        }
    }

    private static void ModifyTooltipHeight(Item __instance, ref Point __result, ref int __3) {
        if (__instance is Trinket t) {
            if (t.GetEffect() is IceOrbTrinketEffect || t.GetEffect() is MagicQuiverTrinketEffect) {
                __result.Y = __3 + 48;
            }
        }
    }
}