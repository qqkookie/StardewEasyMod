using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace EasyMine
{
    public class SkullElevatorMenu : MineElevatorMenu
    {
        public SkullElevatorMenu()
            : base()
        {
            this.initialize(0, 0, 0, 0, true);
            if (Game1.gameMode != (byte)3 || Game1.player == null || Game1.eventUp)
                return;
            Game1.player.Halt();
            this.elevators.Clear();

            int maxElevators = (int)((double)((Game1.player.deepestMineLevel - 120) / SkullElevator.ElevatorStep) / SkullElevator.DifficultyScale);
            maxElevators = Math.Max(1, maxElevators);
            this.width = maxElevators > 50 ? (484 + IClickableMenu.borderWidth * 2) : Math.Min(220 + IClickableMenu.borderWidth * 2, maxElevators * 44 + IClickableMenu.borderWidth * 2);
            this.height = Math.Max(64 + IClickableMenu.borderWidth * 3, maxElevators * 44 / (this.width - IClickableMenu.borderWidth) * 44 + 64 + IClickableMenu.borderWidth * 3);
            this.xPositionOnScreen = (Game1.viewport.Width - this.width) / 2;
            this.yPositionOnScreen = (Game1.viewport.Height - this.height) / 2;
            Game1.playSound("crystal");
            int x1 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
            int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x1, y, 44, 44), string.Concat(0)));
            int x2 = x1 + 64 - 20;
            if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
            {
                x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                y += 44;
            }
            for (int index = 1; index <= maxElevators; ++index)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), string.Concat((index * SkullElevator.ElevatorStep))));
                x2 = x2 + 64 - 20;
                if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
                {
                    x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                    y += 44;
                }
            }
            this.initializeUpperRightCloseButton();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!this.isWithinBounds(x, y))
            {
                Game1.exitActiveMenu();
                return;
            }

            bool flag = false;

            foreach (ClickableComponent current in this.elevators)
            {
                if (!current.containsPoint(x, y))
                    continue;

                int floor = Convert.ToInt32(current.name);

                int? mineLevel = (Game1.currentLocation as MineShaft)?.mineLevel;
                int level = floor + 120;
                if (mineLevel.GetValueOrDefault() == level & mineLevel.HasValue)
                    return;
                Game1.playSound("smallSelect");
                if (floor == 0)
                {
                    if (Game1.mine == null || Game1.currentLocation.Name == "SkullCave")
                        return;
                    Game1.warpFarmer("SkullCave", 3, 4, 2);
                    Game1.exitActiveMenu();
                    Game1.changeMusicTrack("none");
                    flag = true;
                }
                else
                {
                    if (Game1.mine != null && (Game1.currentLocation).Equals(Game1.mine) && floor == Game1.mine.mineLevel)
                        return;
                    Game1.player.ridingMineElevator = true;
                    Game1.enterMine(level);
                    Game1.exitActiveMenu();
                    flag = true;
                }

            }
            if (!flag)
                base.receiveLeftClick(x, y, true);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            foreach (ClickableComponent current in this.elevators)
            {
                int floor = Convert.ToInt32(current.name);
                Vector2 vector2 = new Vector2(current.bounds.X + 16 + NumberSprite.numberOfDigits(floor) * 6,
                    current.bounds.Y + 24 - NumberSprite.getHeight() / 4);

                bool skull = ((Game1.CurrentMineLevel == floor + 120) && Game1.currentLocation.Equals(Game1.mine))
                    || (floor == 0 && Game1.currentLocation.Name == "SkullCave");

                NumberSprite.draw(floor, b, vector2, skull ? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
            }

            this.drawMouse(b);
        }
    }
}
