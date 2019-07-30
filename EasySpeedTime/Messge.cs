using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;


namespace EasySpeedTime
{
    public class Message
    {
        private static SparklingText SubText = null;
        private static Vector2 SubPos;
        private static string BoxedText;
        private static Vector2 BoxedlPos;
        private static double BoxedTimer;

        public static void OnScreen(string message, int x, int y, float timeout = 1.5f)
        {
            SubPos = new Vector2(x, y);
            SubText = new SparklingText(Game1.dialogueFont, message,
                Color.Yellow, Color.Transparent, false, 0.1, (int)(timeout*1000), 16, 1000);
        }

        public static void Boxed(string message, int x, int y, float timeout = 1.5f)
        {
            BoxedText = message;
            BoxedlPos = new Vector2(x, y);
            BoxedTimer = Game1.currentGameTime.TotalGameTime.TotalMilliseconds + timeout*1000;
        }

        internal static void Draw(SpriteBatch sb)
        {
            if (SubText != null)
            {
                if (!SubText.update(Game1.currentGameTime))
                    SubText.draw(sb, SubPos);
                else
                    SubText = null;
            }

            if (BoxedTimer > Game1.currentGameTime.TotalGameTime.TotalMilliseconds
                && !String.IsNullOrEmpty(BoxedText))
            {
                SpriteFont font = Game1.smallFont;
                int margin = Game1.tileSize * 3 / 8;
                var box = font.MeasureString(BoxedText);
                int width = (int)box.X + 2 * margin;
                //60 is "cornerSize" * 3 on SDV source
                int height = Math.Max(60, (int)box.Y + 2 * margin);

                IClickableMenu.drawTextureBox(sb, (int)BoxedlPos.X, (int)BoxedlPos.Y, width, height, Color.White);

                Vector2 tPos = new Vector2(BoxedlPos.X + margin, BoxedlPos.Y + margin + 4);
                sb.DrawString(font, BoxedText, tPos + new Vector2(2, 2), Game1.textShadowColor);
                sb.DrawString(font, BoxedText, tPos, Game1.textColor);
            }
        }
    }
}

#if false
        /*
        public SparklingText FadingText;
        private Vector2 Position;
        private int Timeout;

        /// <summary>Display a string for short duration and fades away.</summary>
        /// <param name="atimeout">time out in seconds (float).</param>
        /// <remark>If menu or event is open, it fails and beeps.</remark>
        internal FadingMessage(string amsg, int x, int y, Color acolor, float atimeout = 2.5f)
        {
            if (Game1.activeClickableMenu != null || !Context.IsPlayerFree)
            {
                this.exitThisMenu();
                return;
            }
            Position = new Vector2(x, y);
            Timeout = (int)(atimeout * 1000.0f);

            FadingText = new SparklingText(Game1.dialogueFont, amsg, acolor, Color.Transparent, false, 0.1, Timeout, 32, 500);
            Game1.activeClickableMenu = this;
        }

        public override void draw(SpriteBatch sb)
        {
            base.draw(sb);
            FadingText.draw(sb, Position);
        }

        public override void update(GameTime now)
        {
            FadingText.update(now);

            Timeout -= 1000 / 60;
            if (Timeout < 0)
            {
                Game1.activeClickableMenu = null;
                this.exitThisMenuNoSound();
            }
        }
        */
#endif
