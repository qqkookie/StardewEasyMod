using System;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EasySpeedTime
{
    internal class FadingMessage : IClickableMenu
    {
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
    }
}
