
namespace PacMan;

public static class Program
{
    public static void Main(params string[] args)
    {
        using var game = new PacMan.PacManGame();
        game.Run();        
    }
}