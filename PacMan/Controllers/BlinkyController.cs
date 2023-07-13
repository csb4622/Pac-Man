
using Microsoft.Xna.Framework;

namespace PacMan.Controllers;

public class BlinkyController: GhostController
{
    public BlinkyController(Level level) : base( level, new Point(level.Width-1, 0), 0)
    {
    }

    protected override Point GetChaseTargetTile()
    {
        return Player.TileLocation;
    }
}