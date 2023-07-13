using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PacMan.Controllers;

public class InputController: Controller
{
    public override ControllerState GetControllerState(ControllerState? previousState, GameTime gameTime)
    {
        //var direction =  Direction.Up;
        var direction = previousState?.Direction ?? Direction.Up;
        if (Keyboard.IsPressed(Keys.Up))
        {
            direction = Direction.Up;
        }
        else if (Keyboard.IsPressed(Keys.Down))
        {
            direction = Direction.Down;
        }
        else if (Keyboard.IsPressed(Keys.Left))
        {
            direction = Direction.Left;
        }
        else if (Keyboard.IsPressed(Keys.Right))
        {
            direction = Direction.Right;
        }
        return new ControllerState(direction);
    }
}