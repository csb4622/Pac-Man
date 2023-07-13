using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PacMan.Screens;

public class Menu
{
    private readonly Texture2D _atlas;
    private readonly SpriteFont _font;

    private bool _shouldStartGame;

    private Point _screenDimensions;

    private readonly string _pushStartMessage;
    private readonly string _playerMessage;
    private readonly string _pointMessage;    
    
    private Vector2 _pushStartMessageLocation;
    private Vector2 _playerMessageLocation;
    private Vector2 _pointMessageLocation;
    

    public Menu(Texture2D atlas, SpriteFont font)
    {
        _pushStartMessage = "PUSH  ENTER  KEY";
        _playerMessage = "1  PLAYER  ONLY";
        _pointMessage = "BONUS  PAC-MAN  FOR  10000  PTS";
        
        _shouldStartGame = false;
        _atlas = atlas;
        _font = font;
    }

    public void Reset(Viewport viewport)
    {
        _screenDimensions = new Point(viewport.Width, viewport.Height);
        
        var playerMessageSize = _font.MeasureString(_playerMessage);
        var pushStartMessageSize = _font.MeasureString(_pushStartMessage);
        var pointsMessageSize = _font.MeasureString(_pointMessage);
        
        _pushStartMessageLocation = new Vector2((_screenDimensions.X/2f)-(pushStartMessageSize.X/2), (_screenDimensions.Y/2f)-pushStartMessageSize.Y);
        
        _playerMessageLocation = new Vector2((_screenDimensions.X/2f)-(playerMessageSize.X/2), ((_screenDimensions.Y)-(_screenDimensions.Y/3f))-(playerMessageSize.Y/2));
        
        _pointMessageLocation = new Vector2((_screenDimensions.X/2f)-(pointsMessageSize.X/2), (_screenDimensions.Y)-((pushStartMessageSize.Y)+20));
        
        _shouldStartGame = false;
    }
    

    public void Update(GameTime gameTime)
    {
        if (Keyboard.HasBeenPressed(Keys.Enter))
        {
            _shouldStartGame = true;
        }
    }

    public bool ShouldStartGame()
    {
        return _shouldStartGame;
    }
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.DrawString(_font, _pushStartMessage, _pushStartMessageLocation, Color.Orange);
        spriteBatch.DrawString(_font, _playerMessage, _playerMessageLocation, Color.LightBlue);
        spriteBatch.DrawString(_font, _pointMessage, _pointMessageLocation, Color.Coral);
    }
}