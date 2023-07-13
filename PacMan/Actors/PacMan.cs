using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacMan.Controllers;

namespace PacMan.Actors;

public class PacMan: Actor
{
    private readonly int _animationSpeed;
    
    private int _currentAnimationTime;
    private int _currentAnimationFrame;
    
    private Vector2? _pelletTileCheck;

    private readonly int _deathAnimationSpeed;
    
    private int _currentDeathAnimationTime;
    private int _currentDeathAnimationFrame;

   
    
    

    public PacMan(Texture2D atlas, Level level) : base(atlas, new InputController(), level, false, false)
    {
        SetLayerDepth(.5f);
        _animationSpeed = 100;
        _currentAnimationTime = 0;
        SetTextureOffset(Point.Zero);
        SetTileLocation(0,0);
        SetSpeed(.1f);
       // SetInvincibility(true);
        _currentAnimationFrame = 0;
        
        _deathAnimationSpeed = 100;
    
        _currentDeathAnimationTime = -400;
        _currentDeathAnimationFrame = 0;

    }

    protected override bool CanWalkOnTile(Tile? tile)
    {
        return tile == null || (base.CanWalkOnTile(tile) && !tile.IsGhostDoor());
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        if (_dying)
        {
            if (Die(gameTime))
            {
                Level.RestartPlayer();
            }
            return;
        }
        _currentAnimationTime += gameTime.ElapsedGameTime.Milliseconds;

        CheckForPellet();
        
        var nextLocation = GetNextLocation(gameTime);
        
        if (nextLocation.Item1.X != Location.X || nextLocation.Item1.Y != Location.Y)
        {

            var newLocation = SanitizeNewLocation(nextLocation);

            SetLocation(newLocation);
            
            UpdateAnimation();
        }
        else
        {
            var center = new Vector2(Location.X + 8, Location.Y + 8);
            var tileLocation = LocationToTileLocation(center);
            var location = TileLocationToLocation(tileLocation);
            SetLocation(location);
        }
    }

    private void CheckForPellet()
    {
        _pelletTileCheck = new Vector2(TileLocation.X, TileLocation.Y);
        // check if there is a pellet in the tile that pac man is in
        var pellet = Level.GetPellet(_pelletTileCheck.Value.ToPoint());
        if (pellet != null)
        {
            // If the pellet is a super pellet set Pac mans FrameSkips to three, and frighten the ghosts
            if (pellet.IsSuper())
            {
                Level.FrightenAllGhostEnemies();
                SetFrameSkips(3);
            }
            // If the pellet is a regular pellet, set pac mans FrameSkips to 1
            else
            {
                SetFrameSkips(1);
            }
            // Remove the pellet from the level
            Level.RemovePellet(TileLocation);
        }
    }
    private void UpdateAnimation()
    {
        if (_currentAnimationTime > _animationSpeed)
        {
            var animationOffset = 0;
            if (PreviousState != null && PreviousState.Direction != CurrentState!.Direction)
            {
                _currentAnimationFrame = 0;
            }
            else
            {
                _currentAnimationFrame = ++_currentAnimationFrame % 2;
            }

            switch (CurrentState!.Direction)
            {
                case Direction.Up:
                    if (!IsFlippedVertically())
                    {
                        FlipVertically();
                    }
                    animationOffset = 0;
                    break;
                case Direction.Down:
                    if (IsFlippedVertically())
                    {
                        FlipVertically();
                    }
                    animationOffset = 0;
                    break;
                case Direction.Left:
                    if (!IsFlippedHorizontally())
                    {
                        FlipHorizontally();
                    }
                    animationOffset = 1;
                    break;
                case Direction.Right:
                    if (IsFlippedHorizontally())
                    {
                        FlipHorizontally();
                    }
                    animationOffset = 1;                       
                    break;                   
            }
            _currentAnimationTime = 0;
            SetTextureOffset(_currentAnimationFrame+1, animationOffset);
        }
    }

    public override bool Die(GameTime gameTime)
    {
        base.Die(gameTime);
        if (IsFlippedHorizontally())
        {
            FlipHorizontally();
        }

        if (IsFlippedVertically())
        {
            FlipVertically();
        }
        
        _currentDeathAnimationTime += gameTime.ElapsedGameTime.Milliseconds;
        if (_currentDeathAnimationTime > _deathAnimationSpeed)
        {
            _currentDeathAnimationTime = 0;
            _currentDeathAnimationFrame++;
            if (_currentDeathAnimationFrame > 10)
            {
                _currentDeathAnimationTime = -400;
                _currentDeathAnimationFrame = 0;
                SetTextureOffset(Point.Zero);
                _dying = false;
                SetInvincibility(false);
                ClearStates();
                return true;
            }
            SetTextureOffset(_currentDeathAnimationFrame+2, 0);
        }
        return false;
    }
    

    protected override void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // if (Debugger.Debug)
        // {
        //     if (_pelletTileCheck.HasValue)
        //     {
        //         spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(), new Rectangle(((int)_pelletTileCheck.Value.X)*16, ((int)_pelletTileCheck.Value.Y)*16, 16, 16), Color.Aqua );
        //     }
        // }
    }
}