using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacMan.Controllers;

namespace PacMan.Actors;

public abstract class Actor
{
    private readonly Controller _controller;
    private readonly Texture2D _atlas;
    private readonly Level _level;
    private readonly int _size;
    private readonly int _physicalSize;
    
    private Point _textureOffset;
    private SpriteEffects _effect;
    private Vector2 _location;
    private Point _tileLocation;
    private float _speed;
    private int _frameSkips;
    private float _layerDepth;
    private bool _invincible;
    protected bool _dying;
    private bool _slowable;
    
    private ControllerState? _currentState;
    private ControllerState? _previousState;
    private Point? _facing;

    private bool _active;
    
    public Vector2 Location => _location;
    public Point TileLocation => _tileLocation;

    public Direction LastDirection => PreviousState?.Direction ?? Direction.Up;
    
    protected ControllerState? CurrentState => _currentState;
    protected ControllerState? PreviousState => _previousState;
    protected Point TextureOffset => _textureOffset;

    protected Point? Facing
    {
        get => _facing;
        set => _facing = value;
    }
    protected Level Level => _level;
    protected int PhysicalSize => _physicalSize;
    public bool IsInvincible => _invincible;

    protected Actor(Texture2D atlas, Controller controller, Level level, bool slowable = false, bool invincible = false)
    {
        _slowable = slowable;
        _dying = false;
        _active = true;
        _invincible = invincible;
        _speed = 0;
        _layerDepth = 0;
        _frameSkips = 0;
        _level = level;
        _controller = controller;
        _atlas = atlas;
        _textureOffset = new Point(0, 0);
        _size = 16;
        _physicalSize = 14;
        _location = Vector2.Zero;
        _tileLocation = Point.Zero;
        _effect = SpriteEffects.None;
        
        controller.SetControlledActor(this);
    }

    public void SetActive(bool active)
    {
        _active = active;
    }
    public void SetInvincibility(bool newValue)
    {
        _invincible = newValue;
    }
    public void SetLocation(Point newLocation)
    {
        SetLocation(newLocation.X, newLocation.Y);
    }
    public void SetLocation(Vector2 newLocation)
    {
        _location = newLocation;
        _tileLocation = LocationToTileLocation(_location);
    }
    public void SetLocation(float x, float y)
    {
        SetLocation(new Vector2(x, y));
    }
    public void SetTileLocation(Point newLocation)
    {
        _tileLocation = newLocation;
        _location = TileLocationToLocation(_tileLocation);
        
    }
    public void SetTileLocation(Vector2 newLocation)
    {
        SetTileLocation(newLocation.ToPoint());
    }
    public void SetTileLocation(int x, int y)
    {
        SetTileLocation(new Point(x, y));
    }
    public virtual void Update(GameTime gameTime)
    {
        if (!_active)
        {
            return;
        }
        if (_frameSkips > 0)
        {
            _frameSkips--;
        }
        else
        {
            _currentState = _controller.GetControllerState(_previousState, gameTime);
            OnUpdate(gameTime);
            _previousState = _currentState;
        }
    }
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (!_active)
        {
            return;
        }
        OnDraw(gameTime, spriteBatch);
        if (Debugger.Debug)
        {
            spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(), new Rectangle(_location.ToPoint(), new Point(_size)), Color.Yellow);
            if (_facing.HasValue)
            {
                spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(),
                    new Rectangle(_facing.Value, new Point(2)), Color.Blue);
            }
        }
        else
        {
            spriteBatch.Draw(
                _atlas,
                new Vector2(_location.X, _location.Y),
                new Rectangle(_textureOffset.X*_size, _textureOffset.Y*_size, _size, _size),
                Color.White,
                0f,
                Vector2.Zero,
                Vector2.One,
                _effect,
                _layerDepth
            );            
        }
    }

    public virtual bool Die(GameTime gameTime)
    {
        _dying = true;
        _invincible = true;
        return true;
    }

    protected void ClearStates()
    {
        _previousState = null;
        _currentState = null;
    }
    
    protected virtual void OnUpdate(GameTime gameTime)
    {}
    protected virtual void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {}
    protected bool IsFlippedHorizontally()
    {
        return (_effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
    }
    protected void FlipHorizontally()
    {
        if (IsFlippedHorizontally())
        {
            _effect &= ~SpriteEffects.FlipHorizontally;    
        }
        else
        {
            _effect |= SpriteEffects.FlipHorizontally;
        }
    }
    protected bool IsFlippedVertically()
    {
        return (_effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
    }
    protected void FlipVertically()
    {
        if (IsFlippedVertically())
        {
            _effect &= ~SpriteEffects.FlipVertically;    
        }
        else
        {
            _effect |= SpriteEffects.FlipVertically;
        }
    }
    protected Vector2 GetNextLocationFromDirection(Direction direction, Vector2 startingLocation, int frameTime)
    {
        var currentTile = Level.GetTile(LocationToTileLocation(startingLocation));
        var factor = GetSpeed(currentTile) * frameTime;
        switch (direction)
        {
            case Direction.Up:
                startingLocation.Y -= factor;
                break;
            case Direction.Down:
                startingLocation.Y += factor;
                break;
            case Direction.Left:
                startingLocation.X -= factor;
                break;
            case Direction.Right:
                startingLocation.X += factor;
                break;
        }

        return startingLocation;
    }
    
    protected (Vector2, Direction) GetNextLocation(GameTime gameTime)
    {
        //var currentMidPoint = new Vector2(_location.X+_size/2f, _location.Y+_size/2f);
        //var currentTile = LocationToTileLocation(currentMidPoint);
        
        // Get the direction that was requested this frame
        var desiredDirection = CurrentState!.Direction;
        
        // Apply speed to current location in the request direction
        var nextLocation =
            GetNextLocationFromDirection(desiredDirection, Location, gameTime.ElapsedGameTime.Milliseconds);

        // Store the new location in a temp variable
        var spotToCheck = nextLocation;
        
        // Created a collision spot infront of the player
        switch (desiredDirection)
        {
            case Direction.Up:
                spotToCheck.X += 8;
                spotToCheck.Y -= 1;
            break;
            case Direction.Down:
                spotToCheck.X += 8;
                spotToCheck.Y += 17;
                break;
            case Direction.Left:
                spotToCheck.X -= 1;
                spotToCheck.Y += 8;
                break;
            case Direction.Right:
                spotToCheck.X += 17;
                spotToCheck.Y += 8;
                break;            
        }
        Facing = spotToCheck.ToPoint();

        
        // Get the tile location that the collision spot is in
        var newTileLocationAsPoint = LocationToTileLocation(Facing.Value);
        
        // Get the tile from the level
        var tile = Level.GetTile(newTileLocationAsPoint);
        
        _level.AddDebugInfo($"XOffset: {MathF.Abs(TileLocation.X - newTileLocationAsPoint.X)}");
        _level.AddDebugInfo($"YOffset: {MathF.Abs(TileLocation.Y - newTileLocationAsPoint.Y)}");
        var manhattanDistance = MathF.Abs(TileLocation.X - newTileLocationAsPoint.X) + MathF.Abs(TileLocation.Y - newTileLocationAsPoint.Y);
        _level.AddDebugInfo($"Distance: {manhattanDistance}");
        
        if (manhattanDistance > 1)
        {
            _level.AddDebugInfo("tried tp skip a tile");
        }


        // If there is a tile there and that tile is a wall recalculate the new position using the previous state
        if (!CanWalkOnTile(tile) || manhattanDistance > 1)
        {
            CurrentState!.Direction = PreviousState?.Direction ?? Direction.Up;
            desiredDirection = CurrentState!.Direction;
            nextLocation =
                GetNextLocationFromDirection(desiredDirection, Location, gameTime.ElapsedGameTime.Milliseconds);
            
            spotToCheck = nextLocation;
            switch (desiredDirection)
            {
                case Direction.Up:
                    spotToCheck.X += 8;
                    spotToCheck.Y -= 1;
                    break;
                case Direction.Down:
                    spotToCheck.X += 8;
                    spotToCheck.Y += 17;
                    break;
                case Direction.Left:
                    spotToCheck.X -= 1;
                    spotToCheck.Y += 8;
                    break;
                case Direction.Right:
                    spotToCheck.X += 17;
                    spotToCheck.Y += 8;
                    break;            
            }

            Facing = spotToCheck.ToPoint();
            
            
            newTileLocationAsPoint = LocationToTileLocation(Facing.Value);
            
            tile = Level.GetTile(newTileLocationAsPoint);
            manhattanDistance = MathF.Abs(TileLocation.X - newTileLocationAsPoint.X) + MathF.Abs(TileLocation.Y - newTileLocationAsPoint.Y);
            if (!CanWalkOnTile(tile) || manhattanDistance > 1)
            {
                nextLocation = Location;
            }
        }
        return (nextLocation, desiredDirection);
    }

    protected virtual bool CanWalkOnTile(Tile? tile)
    {
        return tile == null || !tile.IsWall();
    }
    protected Vector2 SanitizeNewLocation((Vector2, Direction) nextLocation)
    {
        var alignCoordinate = Coordinate.X;
        var coordinateValue = 0f;
        var nextTileLocation = LocationToTileLocation(Facing.Value);
        switch (nextLocation.Item2)
        {
            case Direction.Up:
            case Direction.Down:
                alignCoordinate = Coordinate.X;
                coordinateValue = nextTileLocation.X;
                break;
            case Direction.Left:
            case Direction.Right:
                alignCoordinate = Coordinate.Y;
                coordinateValue = nextTileLocation.Y;
                break;
        }

        coordinateValue *= 16;
            
        switch (alignCoordinate)
        {
            case Coordinate.X:
                nextLocation.Item1.X = coordinateValue;
                break;
            case  Coordinate.Y:
                nextLocation.Item1.Y = coordinateValue;
                break;
        }

        return nextLocation.Item1;
    }
    protected void SetFrameSkips(int frameSkips)
    {
        _frameSkips = frameSkips;
    }
    protected float GetSpeed(Tile? tile)
    {
        return _speed * (_slowable && (tile?.IsSlow() ?? false) ? .5f : 1);
    }
    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
    protected void SetTextureOffset(Point textureOffset)
    {
        _textureOffset = textureOffset;
    }
    protected void SetTextureOffset(int textureOffsetX, int textureOffsetY)
    {
        SetTextureOffset(new Point(textureOffsetX, textureOffsetY));
    }
    protected Vector2 TileLocationToLocation(Vector2 tileLocation)
    {
        return tileLocation * 16;
    }
    protected Vector2 TileLocationToLocation(Point tileLocation)
    {
        return new Vector2(tileLocation.X * 16, tileLocation.Y * 16);
    }    
    protected Point LocationToTileLocation(Vector2 location)
    {
        return LocationToTileLocation(location.ToPoint());
    }
    protected Point LocationToTileLocation(Point location)
    {
        return new Point(location.X / _size, location.Y / _size);
    }
    protected void SetLayerDepth(float layerDepth)
    {
        _layerDepth = layerDepth;
    }
    
}