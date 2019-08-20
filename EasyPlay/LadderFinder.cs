using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EasyPlay
{
    using ModMain = EasyPlay;

    static class LadderFinder
    {
        private static Texture2D pixelTexture;
        private static List<Vector2> LadderStones;
        private static bool NextIsLadder;

        static IReflectionHelper Reflection => ModMain.Reflection;

        internal static void Setup()
        {
            pixelTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] colorArray = Enumerable.Range(0, 1).Select(i => Color.White).ToArray();
            pixelTexture.SetData(colorArray);

            LadderStones = new List<Vector2>();

            ModMain.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            ModMain.Events.Display.RenderedWorld += OnRenderedWorld;
            ModMain.Events.Player.Warped += OnWarped;
        }

        internal static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.mine == null)
                return;

            if (Game1.player.CurrentTool is StardewValley.Tools.Pickaxe && LadderStones.Count == 0)
                FindLadders();
            else if ( Reflection.GetField<bool>(Game1.mine, "ladderHasSpawned").GetValue())
                LadderStones.Clear();

            if (EasyPlay.Config.ForceLadder && Game1.mine.getMineArea(-1) == 121 // Skull Mine area
                && !NextIsLadder && LadderStones.Count > 0)
            {
                Random mineRandom = Reflection.GetField<Random>(Game1.mine, "mineRandom").GetValue();
                Random rng = Clone(mineRandom);

                double nr = rng.NextDouble();
                while (nr >= 0.2)
                {
                    nr = rng.NextDouble();
                    mineRandom.NextDouble();
                }
                NextIsLadder = true;
            }
        }

        internal static void FindLadders()
        {
            int stoneLeft = Reflection.GetField<NetIntDelta>(Game1.mine, "netStonesLeftOnThisLevel").GetValue().Value;
            bool ladderSpawned = Reflection.GetField<bool>(Game1.mine, "ladderHasSpawned").GetValue();

            foreach (var obj in Game1.mine.Objects.Pairs)
            {
                if (obj.Value.Name == "Stone")
                {
                    // ladder chance calculation taken from checkStoneForItems function in MineShaft class
                    Random rng = new Random((int)obj.Key.X * 1000 + (int)obj.Key.Y
                        + Game1.mine.mineLevel + (int)Game1.uniqueIDForThisGame / 2);
                    rng.NextDouble();
                    double chance = 0.02 + 1.0 / (double)Math.Max(1, stoneLeft)
                        + (double)Game1.player.LuckLevel / 100.0 + Game1.dailyLuck / 5.0;
                    if (Game1.mine.characters.Count == 0)
                        chance += 0.04;

                    if (!ladderSpawned && (stoneLeft == 0 || rng.NextDouble() < chance))
                        LadderStones.Add(obj.Key);
                }
            }
        }

        internal static void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.mine == null || !(Game1.player.CurrentTool is StardewValley.Tools.Pickaxe))
                return;

            foreach (var item in LadderStones)
            {
                Rectangle rect = new Rectangle((int)(item.X * Game1.tileSize - Game1.viewport.X),
                         (int)(item.Y * Game1.tileSize - Game1.viewport.Y), Game1.tileSize, Game1.tileSize);
                DrawRectangle(rect, Color.Coral);
            }
        }

        private static void DrawRectangle(Rectangle rect, Color color)
        {
            Game1.spriteBatch.Draw(pixelTexture, new Rectangle(rect.Left, rect.Top, rect.Width, 3), color);
            Game1.spriteBatch.Draw(pixelTexture, new Rectangle(rect.Left, rect.Bottom, rect.Width, 3), color);
            Game1.spriteBatch.Draw(pixelTexture, new Rectangle(rect.Left, rect.Top, 3, rect.Height), color);
            Game1.spriteBatch.Draw(pixelTexture, new Rectangle(rect.Right, rect.Top, 3, rect.Height), color);
        }

        internal static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (Game1.mine == null)
                return;
            LadderStones.Clear();
            NextIsLadder = false;
        }

        private static T Clone<T>(T source)
        {
            IFormatter fmt = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                fmt.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)fmt.Deserialize(stream);
            }
        }
    }
}
