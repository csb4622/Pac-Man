using Microsoft.Xna.Framework;
using PacMan.Actors;

namespace PacMan.Controllers;

public abstract class Controller
{
    private Actor? _me;

    protected Actor Me => _me!;
    public abstract ControllerState GetControllerState(ControllerState? previousState, GameTime gameTime);

    public void SetControlledActor(Actor actor)
    {
        _me = actor;
    }

}

public class ControllerState
{
    public Direction Direction { get; set; }

    public ControllerState(Direction direction)
    {
        Direction = direction;
    }
}