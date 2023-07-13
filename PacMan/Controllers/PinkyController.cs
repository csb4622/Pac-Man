
using Microsoft.Xna.Framework;

namespace PacMan.Controllers;

public class PinkyController: GhostController
{
    public PinkyController(Level level) : base( level, Point.Zero, 30)
    {
    }

    protected override Point GetChaseTargetTile()
    {
        var location = Player.TileLocation; 
        switch (Player.LastDirection)
        {
            case Direction.Up:
                location.Y -= 4;
                location.X -= 4;
                break;
            case Direction.Down:
                location.Y += 4;
                break;
            case Direction.Left:
                location.X -= 4;
                break;
            case Direction.Right:
                location.X += 4;
                break;
        }
        return location;
    }
}