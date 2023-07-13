using Microsoft.Xna.Framework.Input;

namespace PacMan;

public static class Keyboard
{
    private static KeyboardState _currentKeyState;
    private static KeyboardState _previousKeyState;

    public static KeyboardState GetState()
    {
        _previousKeyState = _currentKeyState;
        _currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        return _currentKeyState;
    }

    public static bool IsPressed(Keys key)
    {
        return _currentKeyState.IsKeyDown(key);
    }

    public static bool HasBeenPressed(Keys key)
    {
        return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
    }
}