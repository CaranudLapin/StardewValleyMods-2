﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PrismaticFire
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || who.ActiveObject is null || who.ActiveObject.Name != "Prismatic Shard" || !Game1.didPlayerJustRightClick(false))
                    return true;
                Vector2 vect = new Vector2((float)tileLocation.X, (float)tileLocation.Y);
                if (__instance.objects.ContainsKey(vect) && __instance.objects[vect] is Torch)
                {
                    SMonitor.Log($"giving {__instance.objects[vect].Name} prismatic fire");
                    __instance.objects[vect].modData[modKey] = "true";
                    who.reduceActiveItemByOne();
                    __instance.localSound("fireball");
                    __result = true;
                    return false;
                }
                foreach (Furniture f in __instance.furniture)
                {
                    if ((f.furniture_type.Value == 14 || f.furniture_type.Value == 16) && f.IsOn && f.boundingBox.Value.Contains((int)(vect.X * 64f), (int)(vect.Y * 64f)))
                    {
                        SMonitor.Log($"giving {f.Name} prismatic fire");
                        f.modData[modKey] = "true";
                        who.reduceActiveItemByOne();
                        __instance.localSound("fireball");
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw))]
        public class Furniture_draw_Patch
        {
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, NetVector2 ___drawPosition, NetInt ___sourceIndexOffset)
            {
                if (!Config.ModEnabled || (__instance.furniture_type.Value != 14 && __instance.furniture_type.Value != 16) || !__instance.IsOn || !__instance.modData.ContainsKey(modKey))
                    return;

                if (__instance.furniture_type.Value == 14)
                {
                    spriteBatch.Draw(cursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.boundingBox.Center.X - 12, __instance.boundingBox.Center.Y - 64)), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + x * 3047 + y * 88) % 400.0 / 100.0) * 12, 1985, 12, 11)), Utility.GetPrismaticColor(0, 3), 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f + 0.0001f);
                    spriteBatch.Draw(cursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.boundingBox.Center.X - 32 - 4, __instance.boundingBox.Center.Y - 64)), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + x * 2047 + y * 98) % 400.0 / 100.0) * 12, 1985, 12, 11)), Utility.GetPrismaticColor(0, 3), 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.getBoundingBox(new Vector2(x, y)).Bottom - 1) / 10000f + 0.0001f);
                }
                else if (__instance.furniture_type.Value == 16)
                {
                    spriteBatch.Draw(cursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.boundingBox.Center.X - 20, __instance.boundingBox.Center.Y - 105.6f)), new Rectangle?(new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + x * 3047 + y * 88) % 400.0 / 100.0) * 12, 1985, 12, 11)), Utility.GetPrismaticColor(0, 3), 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.getBoundingBox(new Vector2(x, y)).Bottom - 2) / 10000f + 0.0001f);
                }
            }
        }
        [HarmonyPatch(typeof(Torch), nameof(Torch.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) })]
        public class Torch_draw_Patch_1
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Torch.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteBatch), "Draw", new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)}))
                    {
                        SMonitor.Log("catching SpriteBatch.draw method");
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawTorch)));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Torch), nameof(Torch.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Torch_draw_Patch_2
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Torch.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(SpriteBatch), "Draw", new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float)}))
                    {
                        SMonitor.Log("catching SpriteBatch.draw method");
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.DrawTorch)));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static void DrawTorch(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Torch torch)
        {
            if (Config.ModEnabled && texture == Game1.mouseCursors && torch.modData.ContainsKey(modKey))
            {
                Color c = Utility.GetPrismaticColor(0, 3);
                if (color.R == color.G && color.G == color.B)
                    color = new Color(c.R, c.G, c.B, color.A);
                else
                    color = new Color(c.R, c.G, c.B, color.A) * 0.5f;

            }
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }
    }
}