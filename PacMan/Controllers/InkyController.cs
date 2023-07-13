
using Microsoft.Xna.Framework;
using PacMan.Actors;

namespace PacMan.Controllers;

public class InkyController: GhostController
{
    private readonly Actor _followActor;
    public InkyController(Level level, Actor followActor) : base( level, new Point(0, level.Height-1), 50)
    {
        _followActor = followActor;
    }

    protected override Point GetChaseTargetTile()
    {
        var location = Player.TileLocation; 
        switch (Player.LastDirection)
        {
            case Direction.Up:
                location.Y -= 2;
                location.X -= 2;
                break;
            case Direction.Down:
                location.Y += 2;
                break;
            case Direction.Left:
                location.X -= 2;
                break;
            case Direction.Right:
                location.X += 2;
                break;
        }

        var xOffset = location.X - _followActor.TileLocation.X;
        var yOffset = location.Y - _followActor.TileLocation.Y;

        location.X += xOffset;
        location.Y += yOffset;

        return location;

    }
}