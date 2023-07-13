
using Microsoft.Xna.Framework;

namespace PacMan.Controllers;

public class ClydeController: GhostController
{
    public ClydeController(Level level) : base( level, new Point(level.Width-1, level.Height-1), 70)
    {
    }

    protected override Point GetChaseTargetTile()
    {
        var distance = GetManhattanDistance(Me.TileLocation, Player.TileLocation);
        if (distance < 8)
        {
            return DefaultTargetTile;
        }
        else
        {
            return Player.TileLocation;
        }

    }
}