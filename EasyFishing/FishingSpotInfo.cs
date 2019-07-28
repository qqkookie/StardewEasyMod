﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewModdingAPI;
using xTile.Dimensions;

namespace EasyFishing
{
    using ModMain = EasyFishing;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    /// <summary>FishingSpotInfo class, shamelessly borrowed from JoyofEffort mod by pomepome@Github</summary>
    internal static class FishingSpotInfo
    {
        static internal Point ProbBoxPos = new Point(100, 100);

        internal static void OnRenderedHud(object sender, EventArgs args)
        {
            if (Game1.player.CurrentTool is FishingRod rod && rod.isFishing && !ModMain.Config.DisableFishingSpotInfo)
                ShowFishingSpotInfo();
        }

        private static void ShowFishingSpotInfo()
        {
            if (!(Game1.player.CurrentTool is FishingRod rod) || !rod.isFishing)
                return;

            GameLocation location = Game1.currentLocation;
            Rectangle rectangle = new Rectangle(location.fishSplashPoint.X * 64, location.fishSplashPoint.Y * 64, 64, 64);
            Rectangle value = new Rectangle((int)rod.bobber.X - 80, (int)rod.bobber.Y - 80, 64, 64);
            bool flag = rectangle.Intersects(value);
            int clearWaterDistance = ModMain.Reflection.GetField<int>(rod, "clearWaterDistance").GetValue();
            Dictionary<int, double> dict = GetFishes(location, rod.attachments[0]?.ParentSheetIndex ?? -1, clearWaterDistance + (flag ? 1 : 0), Game1.player);
            DrawProbBox(dict);
        }

        private static void DrawProbBox(Dictionary<int, double> probs)
        {
            SpriteBatch b = Game1.spriteBatch;
            Size size = GetProbBoxSize(probs);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, ProbBoxPos.X, ProbBoxPos.Y, size.Width, size.Height, Color.White);
            const int square = (int)(Game1.tileSize / 1.5);
            int x = ProbBoxPos.X + 8;
            int y = ProbBoxPos.Y + 16;
            SpriteFont font = Game1.smallFont;
            {
                StardewValley.Object tip = null;
                foreach (KeyValuePair<int, double> kv in probs)
                {
                    string text = $"{kv.Value * 100:f1}%";
                    StardewValley.Object fish = new StardewValley.Object(kv.Key, 1);

                    fish.drawInMenu(b, new Vector2(x + 8, y), 1.0f);

                    Utility.drawTextWithShadow(b, text, font, new Vector2(x + 32 + square, y + 16), Color.Black);

                    // Cookie : Check for tool tip
                    Rectangle pic = new Rectangle(x + 8, y, fish.boundingBox.Width, fish.boundingBox.Height);
                    if (pic.Contains(Game1.getMouseX(), Game1.getMouseY()))
                        tip = fish;

                    y += square + 16;
                }

                if ( tip != null)
                {
                    string name = tip.DisplayName;
                    if (name.Length < 5)  // make text width wide enough for price.
                        name = string.Format("{0,-5}", name);
                    IClickableMenu.drawHoverText(Game1.spriteBatch, name, font, 0, 0, tip.Price);
                }

            }
        }

        private static Size GetProbBoxSize(Dictionary<int, double> probs)
        {
            int width = 16, height = 48;
            int square = (int)(Game1.tileSize / 1.5);
            SpriteFont font = Game1.smallFont;
            {
                foreach (KeyValuePair<int, double> kv in probs)
                {
                    string text = $"{kv.Value * 100:f1}%";
                    Vector2 textSize = font.MeasureString(text);
                    int w = square + (int)textSize.X + 64;
                    if (w > width)
                    {
                        width = w;
                    }
                    height += square + 16;
                }
            }
            return new Size(width, height);
        }

        private static Dictionary<int, double> GetFinalProbabilities(Dictionary<int, double> dict)
        {
            Dictionary<int, double> result = new Dictionary<int, double>();
            double ratio = 1.0;
            foreach (KeyValuePair<int, double> kv in dict)
            {
                double d = kv.Value * ratio;
                result.Add(kv.Key, d);
                ratio *= (1 - kv.Value);
            }

            return result;
        }

        private static Dictionary<int, double> GetFishes(GameLocation location, int bait, int waterDepth, Farmer who)
        {
            double sum = 0;
            Dictionary<int, double> dict;
            switch (location)
            {
                case Farm _:
                    dict = GetFishesFarm(waterDepth, who);
                    break;
                case MineShaft shaft:
                    dict = GetFishesMine(shaft, bait, waterDepth, who);
                    break;
                case Submarine _:
                    dict = GetFishesSubmarine();
                    break;
                default:
                    dict = GetFishes(waterDepth, who);
                    break;
            }


            Dictionary<int, double> dict2 =
                GetFinalProbabilities(dict).OrderByDescending(x => x.Value)
                    .Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value);
            sum = dict2.Sum(kv => kv.Value);
            if (1 - sum >= 0.001f)
            {
                dict2.Add(168, 1 - sum);
            }
            return dict2;
        }

        private static Dictionary<int, double> GetFishes(int waterDepth, Farmer who, string locationName = null)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();

            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
            string key = locationName ?? Game1.currentLocation.Name;
            if (key.Equals("WitchSwamp") && !Game1.MasterPlayer.mailReceived.Contains("henchmanGone") && !Game1.player.hasItemInInventory(308, 1))
            {
                return new Dictionary<int, double>
                {
                    {308,0.25}
                };
            }

            try
            {
                if (dictionary.ContainsKey(key))
                {
                    string[] array = dictionary[key].Split('/')[4 + Utility.getSeasonNumber(Game1.currentSeason)].Split(' ');
                    Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                    if (array.Length > 1)
                    {
                        for (int i = 0; i < array.Length; i += 2)
                        {
                            dictionary2.Add(array[i], array[i + 1]);
                        }
                    }

                    string[] array2 = dictionary2.Keys.ToArray();
                    Dictionary<int, string> dictionary3 = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
                    //Utility.Shuffle(random, array2);
                    foreach (string t in array2)
                    {
                        bool flag2 = true;
                        string[] array3 = dictionary3[Convert.ToInt32(t)].Split('/');
                        string[] array4 = array3[5].Split(' ');
                        int num2 = Convert.ToInt32(dictionary2[t]);
                        if (num2 == -1 || Game1.currentLocation.getFishingLocation(who.getTileLocation()) == num2)
                        {
                            int num3 = 0;
                            while (num3 < array4.Length)
                            {
                                if (Game1.timeOfDay < Convert.ToInt32(array4[num3]) ||
                                    Game1.timeOfDay >= Convert.ToInt32(array4[num3 + 1]))
                                {
                                    num3 += 2;
                                    continue;
                                }

                                flag2 = false;
                                break;
                            }
                        }

                        if (!array3[7].Equals("both"))
                        {
                            if (array3[7].Equals("rainy") && !Game1.isRaining)
                            {
                                flag2 = true;
                            }
                            else if (array3[7].Equals("sunny") && Game1.isRaining)
                            {
                                flag2 = true;
                            }
                        }

                        if (who.FishingLevel < Convert.ToInt32(array3[12]))
                        {
                            flag2 = true;
                        }

                        if (flag2)
                            continue;

                        double num4 = Convert.ToDouble(array3[10]);
                        double num5 = Convert.ToDouble(array3[11]) * num4;
                        num4 -= Math.Max(0, Convert.ToInt32(array3[9]) - waterDepth) * num5;
                        num4 += who.FishingLevel / 50f;
                        num4 = Math.Min(num4, 0.89999997615814209);
                        int num = Convert.ToInt32(t);

                        dict.Add(num, num4);
                    }
                }
            }
            catch (KeyNotFoundException knf)
            {
                ModMain.Logger.Log("KeyNotFound Exception occured. Ignoring...");
                ModMain.Logger.Log(knf.ToString());
            }

            return dict;
        }

        private static bool IsGarbage(int index)
        {
            if (index >= 167 && index <= 172)
            {
                return true;
            }
            switch (index)
            {
                case 152:
                case 153:
                case 157: return true;
            }
            return false;
        }

        private static Dictionary<int, double> GetFishesSubmarine()
        {
            return new Dictionary<int, double>
            {
                { 800, 0.1 },
                { 799, 0.18 },
                { 798, 0.28 },
                { 154, 0.1 },
                { 155, 0.08 },
                { 149, 0.05 },
                { 797, 0.01 }
            };
        }

        private static Dictionary<int, double> GetFishesMine(MineShaft shaft, int bait, int waterDepth, Farmer who)
        {
            Dictionary<int, double> dict = new Dictionary<int, double>();
            double num2 = 1.0;
            num2 += 0.4 * who.FishingLevel;
            num2 += waterDepth * 0.1;
            double p = 0;
            int level = shaft.getMineArea();
            switch (level)
            {
                case 0:
                case 10:
                    num2 += bait == 689 ? 3 : 0;
                    p = 0.02 + 0.01 * num2;
                    dict.Add(158, p);
                    break;
                case 40:
                    num2 += bait == 682 ? 3 : 0;
                    p = 0.015 + 0.009 * num2;
                    dict.Add(161, p);
                    break;
                case 80:
                    num2 += bait == 684 ? 3 : 0;
                    p = 0.01 + 0.008 * num2;
                    dict.Add(162, p);
                    break;
            }

            if (level == 10 || level == 40)
            {
                return ConcatDictionary(dict,
                    MagnifyProbabilities(
                        GetFishes(waterDepth, who, "UndergroundMine")
                            .Where(kv => !IsGarbage(kv.Key)).ToDictionary(x => x.Key, x => x.Value),
                        1 - p));
            }

            return dict;
        }

        private static Dictionary<int, double> GetFishesFarm(int waterDepth, Farmer who)
        {
            switch (Game1.whichFarm)
            {
                case 1:
                    return ConcatDictionary(MagnifyProbabilities(GetFishes(waterDepth, who, "Forest"), 0.3), MagnifyProbabilities(GetFishes(waterDepth, who, "Town"), 0.7));
                case 3:
                    return MagnifyProbabilities(GetFishes(waterDepth, who, "Forest"), 0.5);
                case 2:
                    {
                        double p = 0.05 + Game1.dailyLuck;
                        return ConcatDictionary(
                            new Dictionary<int, double> { { 734, p } },
                            MagnifyProbabilities(
                                GetFishes(waterDepth, who, "Forest"),
                                (1 - p) * 0.45)
                            );
                    }
                case 4:
                    {
                        return MagnifyProbabilities(
                            GetFishes(waterDepth, who, "Mountain"),
                            0.35);
                    }
                default:
                    return GetFishes(waterDepth, who);
            }
        }

        private static Dictionary<int, double> MagnifyProbabilities(Dictionary<int, double> dict, double ratio)
        {
            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach (KeyValuePair<int, double> kv in dict)
                result.Add(kv.Key, kv.Value * ratio);

            return result;
        }

        private static Dictionary<K, V> ConcatDictionary<K, V>(Dictionary<K, V> a, Dictionary<K, V> b)
        {
            return a.Concat(b).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
