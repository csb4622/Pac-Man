using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PacMan;

public class Tile
{
    private readonly bool _solid;
    private readonly bool _canStart;
    private readonly bool _ghostDoor;
    private readonly bool _masked;
    private readonly bool _slow;
    private readonly Texture2D _atlas;
    private readonly Point _textureOffset;
    private readonly SpriteEffects _effect;
    private readonly float _layerDepth;
    private Color _color;
    
    private Vector2 _location;
    private readonly int _size;

    private bool _blink;
    private int _currentBlinkTimer;
    private int _blinkSpeed;

    public Tile(Texture2D atlas, Point textureOffset, SpriteEffects effect, float layerDepth, int size,
        bool solid = true, bool canStart = false, bool ghostDoor = false, bool masked = false, bool slow = false)
    {
        _color = Color.Blue;
        _blinkSpeed = 200;
        _currentBlinkTimer = 0;
        _blink = false;
        _layerDepth = layerDepth;
        _atlas = atlas;
        _textureOffset = textureOffset;
        _effect = effect;
        _location = Vector2.Zero;
        _size = size;
        _solid = solid;
        _canStart = canStart;
        _ghostDoor = ghostDoor;
        _masked = masked;
        _slow = slow;
    }

    public bool IsSlow()
    {
        return _slow;
    }
    
    
    public Vector2 GetLocation()
    {
        return _location;
    }
    
    public void SetLocation(int x, int y)
    {
        _location = new Vector2(x*_size, y*_size);
    }
    
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (Debugger.Debug)
        {
            if (IsWall() || !_masked)
            {
                spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(),
                    new Rectangle(_location.ToPoint(), new Point(_size)), Color.Red);
            }
        }
        else
        {
            if (_blink)
            {
                _currentBlinkTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_currentBlinkTimer > _blinkSpeed)
                {
                    _currentBlinkTimer = 0;
                    _color = _color == Color.White ? Color.Blue : Color.White;
                }
            }
            
            
            spriteBatch.Draw(
                _atlas,
                new Vector2(_location.X, _location.Y),
                new Rectangle(_textureOffset.X * _size, _textureOffset.Y * _size, _size, _size),
                _color, 0f, Vector2.Zero, Vector2.One, _effect, _layerDepth);
        }
    }

    public void StartBlink()
    {
        _blink = true;
        _currentBlinkTimer = 0;
    }
    public void StopBlink()
    {
        _color = Color.Blue;
        _blink = false;
        _currentBlinkTimer = 0;
    }
    
    public bool IsWall()
    {
        return _solid;
    }

    public bool CanStartHere()
    {
        return _canStart;
    }

    public bool IsGhostDoor()
    {
        return _ghostDoor;
    }
}