using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PacMan;

public class Tiles
{
    private readonly Texture2D _atlas;
    private readonly int _size;
    public Tiles(Texture2D atlas, int size)
    {
        _size = size;
        _atlas = atlas;
    }

    public Tile TopLeftCorner() =>
        new(_atlas, new Point(0, 2), SpriteEffects.None, 0, _size);
    public Tile TopRightCorner() =>
        new(_atlas, new Point(0, 2), SpriteEffects.FlipHorizontally, 0,  _size);    
    public Tile BottomRightCorner() =>
        new(_atlas, new Point(0, 2), SpriteEffects.FlipHorizontally|SpriteEffects.FlipVertically, 0,  _size);
    public Tile BottomLeftCorner() =>
        new(_atlas, new Point(0, 2), SpriteEffects.FlipVertically, 0,  _size);
    
    public Tile Horizontal() =>
        new(_atlas, new Point(1, 2), SpriteEffects.None, 0,  _size);
    
    public Tile Vertical() =>
        new(_atlas, new Point(0, 3), SpriteEffects.None, 0,  _size);
    
    public Tile DownEnd() =>
        new(_atlas, new Point(2, 2), SpriteEffects.None, 0,  _size);
    public Tile UpEnd() =>
        new(_atlas, new Point(2, 2), SpriteEffects.FlipVertically, 0,  _size);
    public Tile RightEnd() =>
        new(_atlas, new Point(2, 3), SpriteEffects.None, 0,  _size);
    public Tile LeftEnd() =>
        new(_atlas, new Point(2, 3), SpriteEffects.FlipHorizontally, 0,  _size);    
    
    public Tile DownInsideT() =>
        new(_atlas, new Point(1, 3), SpriteEffects.None, 0,  _size);
    public Tile UpInsideT() =>
        new(_atlas, new Point(1, 3), SpriteEffects.FlipVertically, 0,  _size);
    public Tile RightInsideT() =>
        new(_atlas, new Point(1, 4), SpriteEffects.None, 0,  _size);
    public Tile LeftInsideT() =>
        new(_atlas, new Point(1, 4), SpriteEffects.FlipHorizontally, 0, _size);
    
    public Tile SolidMask() =>
        new(_atlas, new Point(0, 4), SpriteEffects.None, .8f, _size, true, false, false, true);    
    public Tile Mask() =>
        new(_atlas, new Point(0, 4), SpriteEffects.None, .8f, _size, false, false, false, true);
    public Tile NoPellet(bool slowsGhost = false) =>
        new(_atlas, new Point(0, 4), SpriteEffects.None, 0, _size, false, false, false, true, slowsGhost);    
    public Tile PlayerStart() =>
        new(_atlas, new Point(0, 1), SpriteEffects.None, 0, _size, false, true, false, true);
    public Tile GhostDoor() =>
        new(_atlas, new Point(2, 4), SpriteEffects.None, 0, _size, false, false, true);    
}