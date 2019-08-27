using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

using StardewValley;

namespace EasySpeedTime
{
    internal static class AutoGate
    {
        public static SerializableDictionary<Vector2, Fence> GateList = new SerializableDictionary<Vector2, Fence>();
        private static Point OldTile = new Point();
        private static Vector2[] OldAdjTiles = new Vector2[] { };

        internal static void CheckGates()
        {
            if (!(Game1.currentLocation is StardewValley.Locations.BuildableGameLocation))
                return;

            Point NewTile = Game1.player.getTileLocationPoint();
            if (OldTile == NewTile)
                return;

            Vector2[] newAdjTiles = Utility.getAdjacentTileLocationsArray(Game1.player.getTileLocation());
            var tilesComing = newAdjTiles.Where(p => !OldAdjTiles.Contains(p));

            // Open coming gates
            GateList.AsParallel().Where(p => tilesComing.Contains(p.Key)
                    && (p.Value.gatePosition.Value == Fence.gateClosedPosition)
                    && p.Value.checkForAction(Game1.player, true))
                    .ForAll((gate => gate.Value.gatePosition.Value = Fence.gateOpenedPosition));

            var tilesGoing = OldAdjTiles.Where(p => !newAdjTiles.Contains(p));

            // Close going gates
            var qlist = GateList.AsParallel().Where(p => tilesGoing.Contains(p.Key)
                && (p.Value.gatePosition.Value == Fence.gateOpenedPosition));
            if ( qlist.Any())
            {
                qlist.ForAll((gate => gate.Value.gatePosition.Value = Fence.gateClosedPosition));
                Game1.playSound("doorClose");
            }

            OldAdjTiles = newAdjTiles;
        }

        internal static void UpdateGateList()
        {
            if (!(Game1.currentLocation is Farm))
                return;

            GateList = new SerializableDictionary<Vector2, Fence>();

            OldTile = new Point();
            OldAdjTiles = new Vector2[] { };

            Game1.currentLocation.Objects.AsParallel().OfType<Dictionary<Vector2, Fence>>().SelectMany(d => d)
                .Where(kv => (kv.Value is Fence gate) && gate.isGate.Value &&
                    (gate.name.Contains("Fence") || gate.name.Contains("Gate")))
                .ForAll(gate => GateList.Add(gate.Key, gate.Value));
        }
    }
}
