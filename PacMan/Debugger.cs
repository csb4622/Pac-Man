namespace PacMan;

public static class Debugger
{
    private static bool _debug;
    public static bool Debug
    {
        get => _debug;
        set => _debug = value;
    }
}