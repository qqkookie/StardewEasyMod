using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace EasyPlay
{
    public class SkullElevatorMenuScroll : MineElevatorMenu
    { 
        private const int ELEVATORSIZE = 121;
        private const int SCROLLSTEP = 11;

        public ClickableTextureComponent upArrow;
        public ClickableTextureComponent downArrow;
        public ClickableTextureComponent scrollBar;
        public Rectangle scrollBarRunner;
        private int currentItemIndex;
        private int maxElevators;
        private bool scrolling;

        public SkullElevatorMenuScroll() : base()
        {

            this.initialize(0, 0, 0, 0, true);
            this.maxElevators = (int)(((Game1.player.deepestMineLevel - 120) / SkullElevator.ElevatorStep) / SkullElevator.DifficultyScale);
            maxElevators = Math.Max(1, maxElevators);
            if (Game1.gameMode != (byte)3 || Game1.player == null || Game1.eventUp)
                return;
            Game1.player.Halt();
            this.elevators.Clear();
            int num = 120;
            this.width = num > 50 ? (484 + IClickableMenu.borderWidth * 2) : Math.Min(220 + IClickableMenu.borderWidth * 2, num * 44 + IClickableMenu.borderWidth * 2);
            this.height = Math.Max(64 + IClickableMenu.borderWidth * 3, num * 44 / (this.width - IClickableMenu.borderWidth) * 44 + 64 + IClickableMenu.borderWidth * 3);
            this.xPositionOnScreen = (Game1.viewport.Width - this.width) / 2;
            this.yPositionOnScreen = (Game1.viewport.Height - this.height) / 2;
            Game1.playSound("crystal");
            this.upArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f, false);
            this.downArrow = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 16, this.yPositionOnScreen + this.height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f, false);

            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);

            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, this.height - 128 - this.upArrow.bounds.Height - 8);
            int x1 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
            int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
            this.elevators.Add(new ClickableComponent(new Rectangle(x1, y, 44, 44), string.Concat(0)));
            int x2 = x1 + 64 - 20;
            if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
            {
                x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                y += 44;
            }
            for (int index = 1; index <= num; ++index)
            {
                this.elevators.Add(new ClickableComponent(new Rectangle(x2, y, 44, 44), string.Concat(index * SkullElevator.ElevatorStep)));
                x2 += 44;
                if (x2 > this.xPositionOnScreen + this.width - IClickableMenu.borderWidth)
                {
                    x2 = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
                    y += 44;
                }
            }
          this.initializeUpperRightCloseButton();
        }

        private void setScrollBarToCurrentIndex()
        {
            if (this.elevators.Count <= 0)
                return;

             this.scrollBar.bounds.Y = (int) ((double)this.scrollBarRunner.Height / (double)Math.Max(1, this.maxElevators - ELEVATORSIZE + 1) * (double)this.currentItemIndex) + this.upArrow.bounds.Bottom + 4;
            if (this.currentItemIndex != this.maxElevators - ELEVATORSIZE + 1)
                return;

            this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
                return;
            this.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.upArrowPressed();
                Game1.playSound("shiny4");
            }
            else
            {
                if (direction >= 0 || this.currentItemIndex >= Math.Max(0, this.maxElevators - ELEVATORSIZE))
                    return;
                this.downArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        private void downArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            this.currentItemIndex += SCROLLSTEP;
            if (this.currentItemIndex > this.maxElevators - ELEVATORSIZE)
                this.currentItemIndex = this.maxElevators - ELEVATORSIZE + 1;
            this.setScrollBarToCurrentIndex();
        }

        private void upArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            this.currentItemIndex -= SCROLLSTEP;
            if (this.currentItemIndex < 0)
                this.currentItemIndex = 0;
            this.setScrollBarToCurrentIndex();
        }

        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
                return;
            this.leftClickHeld(x, y);
            if (!this.scrolling)
                return;

            int y1 = this.scrollBar.bounds.Y;

            this.scrollBar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + 20));
            this.currentItemIndex = Math.Min(this.maxElevators - ELEVATORSIZE + 1, Math.Max(0, (int)((this.maxElevators - ELEVATORSIZE) * ((y - this.scrollBarRunner.Y) / (double)this.scrollBarRunner.Height))));
            this.setScrollBarToCurrentIndex();

            int y2 = this.scrollBar.bounds.Y;
            if (y1 == y2)
                return;
            Game1.playSound("shiny4");
        }

        public override void performHoverAction(int x, int y)
        {
            if (GameMenu.forcePreventClose )
                return;
            this.upArrow.tryHover(x, y, 0.1f);
            this.downArrow.tryHover(x, y, 0.1f);
            this.scrollBar.tryHover(x, y, 0.1f);

            foreach (ClickableComponent current in elevators)
                current.scale = current.containsPoint(x, y) ? 2.0f: 1.0f;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.downArrow.containsPoint(x, y))
            {
                if (this.currentItemIndex >= Math.Max(0, this.maxElevators - ELEVATORSIZE))
                    return;
                this.downArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.upArrow.containsPoint(x, y))
            {
                if (this.currentItemIndex <= 0)
                    return;
                this.upArrowPressed();
                Game1.playSound("shwip");
            }
            else if (this.scrollBar.containsPoint(x, y))
                this.scrolling = true;
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width 
                && (x < this.xPositionOnScreen + this.width + 128 && y > this.yPositionOnScreen) 
                && y < this.yPositionOnScreen + this.height)
            {
                this.scrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }
            else if (this.isWithinBounds(x, y))
            {
                bool flag = false;

                foreach(ClickableComponent current in this.elevators)
                { 
                    if (!current.containsPoint(x, y))
                        continue;

                    int? mineLevel = (Game1.currentLocation as MineShaft)?.mineLevel;
                    int floor = Convert.ToInt32(current.name);
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
                        if (Game1.mine != null && Game1.currentLocation.Equals(Game1.mine) && floor == Game1.mine.mineLevel)
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
            else
                Game1.exitActiveMenu();
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            Game1.drawDialogueBox((int)this.xPositionOnScreen, this.yPositionOnScreen - 64 + 8, this.width + 21, this.height + 64, false, true, null, false, false);
            this.upperRightCloseButton.draw(b);
            this.upArrow.draw(b);
            this.downArrow.draw(b);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, false);
            this.scrollBar.draw(b);
            for (int index = 0; index < ELEVATORSIZE; ++index)
            {
                ClickableComponent elevator = this.elevators[index];
                elevator.name = string.Concat(((index + this.currentItemIndex) * SkullElevator.ElevatorStep));
                drawElevator(b, elevator);
            }
            this.drawMouse(b);
        }

        private static void drawElevator(SpriteBatch b, ClickableComponent elevator)
        {
            b.Draw(Game1.mouseCursors, new Vector2((elevator.bounds.X - 4), ( elevator.bounds.Y + 4)), new Rectangle(elevator.scale > 1.0 ? 267 : 256, 256, 10, 10), Color.Black * 0.5f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);

            b.Draw(Game1.mouseCursors, new Vector2(elevator.bounds.X, elevator.bounds.Y), new Rectangle(elevator.scale > 1.0f ? 267 : 256, 256, 10, 10), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.868f);

            int floor = Convert.ToInt32(elevator.name);
            Vector2 vector2 = new Vector2((elevator.bounds.X + 16 + NumberSprite.numberOfDigits(floor) * 6), 
                (elevator.bounds.Y + 24 - NumberSprite.getHeight() / 4));

            bool skull = ((Game1.CurrentMineLevel == floor + 120) && Game1.currentLocation.Equals(Game1.mine))
                || (floor == 0 && Game1.currentLocation.Name == "SkullCave");

            NumberSprite.draw(floor, b, vector2, skull? Color.Gray * 0.75f : Color.Gold, 0.5f, 0.86f, 1f, 0, 0);
        }
    }
}
