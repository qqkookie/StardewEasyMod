using Microsoft.Xna.Framework;
using xTile.Dimensions;

using StardewValley;
using StardewModdingAPI;

namespace EasySpeedTime
{
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    internal static class JumpSwim
    {
        private static readonly float JumpHeight = 8.0f;
        private static readonly float JumpDelta = 5.0f / Game1.tileSize;

        internal static bool Jumping = false;
        private static float OldJumpVelocity = 0.0f;

        internal static void StartJump()
        {
            Farmer player = Game1.player;
            if (!Context.CanPlayerMove || player.UsingTool || player.isRidingHorse()
                || Jumping || player.yJumpVelocity > 0.0)
                return;

            OldJumpVelocity = player.yJumpVelocity;

            Location dTile = FacingLocation(3);
            GameLocation map = Game1.currentLocation;
            if (!map.isTileOnMap(dTile.X, dTile.Y))
                return;

            float jh = JumpHeight;
            if (player.swimming.Value)
            {
                if (!map.waterTiles[dTile.X, dTile.Y])
                {
                    player.changeOutOfSwimSuit();
                    player.swimming.Value = false;
                }
                else
                    jh /= 4; // water to water
            }
            player.jump(jh);
            Jumping = true;
        }

        internal static void JumpUpdateTicked()
        {
            if (!Jumping)
                return;

            Farmer player = Game1.player;
            GameLocation map = Game1.currentLocation;
            Rectangle nr = player.GetBoundingBox();
            Location dvec = FacingLocation() * Game1.tileSize;
            Point next = new Point(dvec.X, dvec.Y);

            if (player.yJumpVelocity != 0.0 || OldJumpVelocity >= 0)
            {
                bool ok1 = !map.isCollidingPosition(nr, Game1.viewport, true, 0, false, player);
                nr.Offset(next);
                bool ok2 = !map.isCollidingPosition(nr, Game1.viewport, true, 0, false, player);
                nr.Offset(next);
                bool ok3 = !map.isCollidingPosition(nr, Game1.viewport, true, 0, false, player);

                if (!ok1 || ok1 && !ok2 && ok3)
                {
                    player.canMove = false;
                    player.position.X += JumpDelta * dvec.X;
                    player.position.Y += JumpDelta * dvec.Y;
                }
                OldJumpVelocity = player.yJumpVelocity;
            }
            else
            {
                CheckForSwimSuit();

                player.canMove = true;
                Jumping = false;
            }
        }

        internal static void CheckForSwimSuit()
        {
            Farmer player = Game1.player;
            GameLocation map = Game1.currentLocation;
            Point cTile = player.getTileLocationPoint();

            if ( map.waterTiles != null && map.isTileOnMap(cTile.X, cTile.Y)
                && map.waterTiles[cTile.X, cTile.Y] != player.swimming.Value)
            {
                player.swimming.Value = !player.swimming.Value;
                if (player.swimming.Value)
                    player.changeIntoSwimsuit();
                else
                    player.changeOutOfSwimSuit();
            }
        }

        // tiles > 0  : get absolute location of n tiles away in fornt of the player
        // tiles =< 0 or no args: get relative unit vector in fornt of the player.
        private static Location FacingLocation(int tiles = 0)
        {
            int d = tiles > 0 ? tiles : 1;
            int dx = 0, dy = 0;

            int dir = Game1.player.facingDirection;
            if (dir == 1)
                dx = d;
            else if (dir == 2)
                dy = d;
            else if (dir == 3)
                dx = -d;
            else
                dy = -d;

            if (tiles > 0)
            {
                Point pxy = Game1.player.getTileLocationPoint();
                return new Location(dx + pxy.X, dy + pxy.Y);
            }
            return new Location(dx, dy);
        }
    }
}