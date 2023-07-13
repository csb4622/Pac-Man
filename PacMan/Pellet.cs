using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PacMan;

public class Pellet
{
    private readonly bool _super;
    private readonly Texture2D _atlas;
    private readonly Point _textureOffset;
    private readonly Vector2 _location;
    private readonly int _size;

    private Color _color;

    public Pellet(Texture2D atlas, int size, bool super, Vector2 location)
    {
        _color = Color.White;
        _atlas = atlas;
        _size = size;
        _super = super;
        _location = location;
        _textureOffset = new Point(13, (_super ? 0 : 1));
    }

    public void SetColor(Color color)
    {
        _color = color;
    }
    
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (Debugger.Debug)
        {
            var area = _super
                ? new Rectangle((int)_location.X*_size+4, (int)_location.Y*_size+4, 4, 4)
                : new Rectangle((int)_location.X*_size+8, (int)_location.Y*_size+8, 2, 2);
         spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(), area, Color.Green);   
        }
        else
        {
            spriteBatch.Draw(
                _atlas,
                new Vector2(_location.X * _size, _location.Y * _size),
                new Rectangle(_textureOffset.X * _size, _textureOffset.Y * _size, _size, _size),
                _color,
                0f,
                Vector2.Zero,
                Vector2.One,
                SpriteEffects.None,
                .4f);
        }
    }

    public bool IsSuper()
    {
        return _super;
    }
    
    
}