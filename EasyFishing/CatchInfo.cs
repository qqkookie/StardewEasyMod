using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyFishing
{
    using ModMain = EasyFishing;

    /// <summary>CatchInfo class, shamelessly borrowed from JoyofEffort mod by pomepome@Github</summary>
    internal static class CatchInfo
    {
        internal static void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs args)
        {
            if (!ModMain.Config.DisableCatchInfo && Game1.activeClickableMenu is BobberBar)
                ShowCatchInfo();

            if (!ModMain.Config.DisableExtraInfo)
                ExtraInfo.DrawFishHover(args);
        }

        private static void ShowCatchInfo()
        {

            if (!(Game1.activeClickableMenu is BobberBar bar))
                return;

            SpriteBatch batch = Game1.spriteBatch;
            SpriteFont font = Game1.smallFont;

            IReflectionHelper reflection = ModMain.Reflection;
            ITranslationHelper translation = ModMain.Translation;

            int width = 0, height = 120;

            float scale = 1.0f;

            int whitchFish = reflection.GetField<int>(bar, "whichFish").GetValue();
            int fishSize = reflection.GetField<int>(bar, "fishSize").GetValue();
            int fishQuality = reflection.GetField<int>(bar, "fishQuality").GetValue();
            bool treasure = reflection.GetField<bool>(bar, "treasure").GetValue();
            bool treasureCaught = reflection.GetField<bool>(bar, "treasureCaught").GetValue();
            float treasureAppearTimer = reflection.GetField<float>(bar, "treasureAppearTimer").GetValue() / 1000;

            StardewValley.Object fish = new StardewValley.Object(whitchFish, 1);

            // string speciesText = TryFormat(translation.Get("fishinfo.species").ToString(), fish.DisplayName);
            // string sizeText = TryFormat(translation.Get("fishinfo.size").ToString(), GetFinalSize(fishSize));
            string speciesText = translation.Get("fishinfo.species").ToString() + fish.DisplayName;
            string sizeText = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14082") + " " + FishSizeUnit(fishSize);

            string qualityText1 = translation.Get("fishinfo.quality").ToString();
            string qualityText2 = translation.Get(GetKeyForQuality(fishQuality)).ToString();
            string incomingText = TryFormat(translation.Get("fishinfo.treasure.incoming").ToString(), treasureAppearTimer);
            string appearedText = translation.Get("fishinfo.treasure.appear").ToString();
            string caughtText = translation.Get("fishinfo.treasure.caught").ToString();

            {
                Vector2 size = font.MeasureString(speciesText) * scale;
                if (size.X > width)
                {
                    width = (int)size.X;
                }
                height += (int)size.Y;
                size = font.MeasureString(sizeText) * scale;
                if (size.X > width)
                {
                    width = (int)size.X;
                }
                height += (int)size.Y;
                Vector2 temp = font.MeasureString(qualityText1);
                Vector2 temp2 = font.MeasureString(qualityText2);
                size = new Vector2(temp.X + temp2.X, Math.Max(temp.Y, temp2.Y));
                if (size.X > width)
                {
                    width = (int)size.X;
                }
                height += (int)size.Y;
            }

            if (treasure)
            {
                if (treasureAppearTimer > 0)
                {
                    Vector2 size = font.MeasureString(incomingText) * scale;
                    if (size.X > width)
                    {
                        width = (int)size.X;
                    }
                    height += (int)size.Y;
                }
                else
                {
                    if (!treasureCaught)
                    {
                        Vector2 size = font.MeasureString(appearedText) * scale;
                        if (size.X > width)
                        {
                            width = (int)size.X;
                        }
                        height += (int)size.Y;
                    }
                    else
                    {
                        Vector2 size = font.MeasureString(caughtText) * scale;
                        if (size.X > width)
                        {
                            width = (int)size.X;
                        }
                        height += (int)size.Y;
                    }
                }
            }

            width += 64;

            int x = bar.xPositionOnScreen + bar.width + 96;
            if (x + width > Game1.viewport.Width)
            {
                x = bar.xPositionOnScreen - width - 96;
            }
            int y = (int)Cap(bar.yPositionOnScreen, 0, Game1.viewport.Height - height);

            IClickableMenu.drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);
            fish.drawInMenu(batch, new Vector2(x + width / 2 - 32, y + 16), 1.0f, 1.0f, 0.9f, false);

            Vector2 vec2 = new Vector2(x + 32, y + 96);
            DrawString(batch, font, ref vec2, speciesText, Color.Black, scale);
            DrawString(batch, font, ref vec2, sizeText, Color.Black, scale);
            DrawString(batch, font, ref vec2, qualityText1, Color.Black, scale, true);
            DrawString(batch, font, ref vec2, qualityText2, GetColorForQuality(fishQuality), scale);
            vec2.X = x + 32;
            if (treasure)
            {
                if (!treasureCaught)
                {
                    if (treasureAppearTimer > 0f)
                    {
                        DrawString(batch, font, ref vec2, incomingText, Color.Red, scale);
                    }
                    else
                    {
                        DrawString(batch, font, ref vec2, appearedText, Color.LightGoldenrodYellow, scale);
                    }
                }
                else
                {
                    DrawString(batch, font, ref vec2, caughtText, Color.ForestGreen, scale);
                }
            }
        }

        private static string FishSizeUnit(int inch)    // fish size in inch
        {
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en
                && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
                return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (int)Math.Round(inch * 2.54));
            else if (ModMain.Config.MetricSize)
                return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (int)Math.Round(inch * 2.54))
                     .Replace((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? "인치." : "in."), "cm");
            else
                return Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", inch);
        }

        private static string GetKeyForQuality(int fishQuality)
        {
            switch (fishQuality)
            {
                case 1: return "quality.silver";
                case 2: return "quality.gold";
                case 3: return "quality.iridium";
                default: return "quality.normal";
            }
        }

        private static Color GetColorForQuality(int fishQuality)
        {
            switch (fishQuality)
            {
                case 1: return Color.AliceBlue;
                case 2: return Color.Tomato;
                case 3: return Color.Purple;
                default: return Color.WhiteSmoke;
            }
        }

        private static void DrawString(SpriteBatch batch, SpriteFont font, ref Vector2 location, string text, Color color, float scale, bool next = false)
        {
            Vector2 stringSize = font.MeasureString(text) * scale;
            batch.DrawString(font, text, location, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            if (next)
            {
                location += new Vector2(stringSize.X, 0);
            }
            else
            {
                location += new Vector2(0, stringSize.Y);
            }
        }

        private static string TryFormat(string str, params object[] args)
        {
            try
            {
                string ret = string.Format(str, args);
                return ret;
            }
            catch
            {
                // ignored
            }

            return "";
        }

        public static int Cap(int n, int min, int max)
        {
            return n < min ? min : (n > max ? max : n);
        }

        internal static void RedrawFishQualitySize(SpriteBatch sb, FishingRod rod)
        {
            int fishSize = ModMain.Reflection.GetField<int>(rod, "fishSize", true).GetValue();
            int fishQuality = ModMain.Reflection.GetField<int>(rod, "fishQuality", true).GetValue();

            if (fishSize < 0)
                return;

            if (fishQuality > 0 && fishQuality <= 3)
                DrawQualityStar(sb, fishQuality);

            if ((LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en || !ModMain.Config.MetricSize)
                && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
                return;
            // For English/Korean locale Only.

            Farmer lastUser = Game1.player;
            float oscy = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));

            string sizetxt = FishSizeUnit(fishSize);
            float x = -Game1.tileSize * 2 + 8 + 205 - Game1.smallFont.MeasureString(sizetxt).X / 2.0f;
            float y = -Game1.tileSize * 5 + Game1.tileSize * 7 / 3 - 8 + oscy;
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, lastUser.position + new Vector2(x, y));

            Color color = rod.recordSize ? Color.Blue * Math.Min(1f, (float)((double)oscy / 8.0 + 1.5)) : Game1.textColor;

            var dummyTexture = new Texture2D(sb.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.OldLace });
            // paint white color over old original fish size text
            sb.Draw(dummyTexture, pos + new Vector2(-13f, 0), new Rectangle(0, 0, 120, 40), Color.White, 0.0f,
                Vector2.Zero, 1f, SpriteEffects.None, (float)((double)lastUser.getStandingY() / 10000.0 + 0.06 + 0.005));

            sb.DrawString(Game1.smallFont, sizetxt, pos, color, 0.0f, Vector2.Zero, 1f, SpriteEffects.None,
                (float)((double)lastUser.getStandingY() / 10000.0 + 1.0 / 500.0 + 0.06 + 0.006));

        }

        private static void DrawQualityStar(SpriteBatch sb, int fishQuality)
        {
            Rectangle? sourceRectangle;
            if (fishQuality == 1)
                sourceRectangle = new Rectangle(338, 400, 8, 8);
            else if (fishQuality == 2)
                sourceRectangle = new Rectangle(346, 400, 8, 8);
            else if (fishQuality == 3)
                sourceRectangle = new Rectangle(346, 392, 8, 8);
            else
                return;

            float oscy = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position + new Vector2(-80f, -176f + oscy));

            sb.Draw(Game1.mouseCursors, pos, sourceRectangle, Color.White, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, 0.0f);
        }
    }
}
