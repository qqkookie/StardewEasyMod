using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace EasySpeedTime
{
    internal static class RunningClock
    {
        private static int TimeInterval;
        private static int LastGameTime;

        internal static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!Game1.displayHUD || Game1.eventUp || Game1.currentBillboard != 0 
                || Game1.gameMode != 3 || Game1.freezeControls || Game1.panMode || Game1.HostPaused)
                return;

            int now = Game1.timeOfDay; // 600-2600, 1600 for PM 4:00

            if ( !Context.IsMultiplayer || Context.IsMainPlayer)
                TimeInterval = Game1.gameTimeInterval;
            else if (now == LastGameTime)
                TimeInterval += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
            else
            {
                LastGameTime = now;
                TimeInterval = 0;
            }

            // added ticks for one min. by local time dialation
            int add = (int) Game1.MasterPlayer.currentLocation?.getExtraMillisecondsPerInGameMinuteForThisLocation() / 10 ;
            int myTime = now + TimeInterval / (700 + add);

            // update every two minutes, except every 10 mins.
            myTime -= myTime % 2;
            if (myTime % 10 == 0)
                return;

            SpriteFont displayFont = Game1.dialogueFont;
            string Display = Game1.getTimeOfDayString(myTime);
            if (myTime % 100 < 10)
                Display = Display.Replace(":", ":0");

            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
            {
                displayFont = Game1.smallFont;
                Display += Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs."
                    + ((myTime < 1200 || myTime >= 2400) ? "10370": "10371"));
            }

            // Draw digital clock face
            SpriteBatch SB = e.SpriteBatch;
            Rectangle sourceRect = new Rectangle(333, 431, 71, 43);
            Vector2 moneyBoxPos = Game1.dayTimeMoneyBox.position;
            Vector2 offset = new Vector2(108f, 112f);
            Rectangle bounds = new Rectangle(360, 459, 40, 9);

            SB.Draw(Game1.mouseCursors, moneyBoxPos + offset, bounds,
                Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

            int timeShakeTimer = Game1.dayTimeMoneyBox.timeShakeTimer;
            Vector2 timeShake = new Vector2(shake(timeShakeTimer), shake(timeShakeTimer));
            Vector2 displaySize = displayFont.MeasureString(Display);
            Vector2 displayPos = new Vector2(sourceRect.X * 0.55f, sourceRect.Y * 0.31f) - displaySize / 2.0f + timeShake;

            Color displaycolor= Game1.textColor;
            if (now >= 2400)
                displaycolor = Color.Red;
            else if (! Game1.shouldTimePass() && !Game1.fadeToBlack
                    && (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 < 1000.0))
                displaycolor *= 0.5f;

            Utility.drawTextWithShadow(SB, Display, displayFont,
                moneyBoxPos + displayPos, displaycolor, 1f, -1f, -1, -1, 1f, 3);
        }

        private static float shake(int time) { return ( time > 0 ? Game1.random.Next(-2, 3) : 0.0f); }
    }
}
