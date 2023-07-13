using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PacMan.Controllers;

namespace PacMan.Actors;

public class Ghost : Actor
{
    private readonly int _animationSpeed;

    private int _currentAnimationTime;
    private int _currentAnimationFrame;
    private GhostController _ghostController;
    private bool _blinking;
    private int _blinkingOffset;
    private int _frightenTimer;
    private int _currentFrightenTime;
    private int _blinkSpeed;
    private int _currentBlinkTimer;
    private int _ghostOffset;
    private int _eatenOffset;

    public bool IsFrighted => _ghostController.State == GhostState.Frightened;
    public bool WasEaten => _ghostController.State == GhostState.Eaten;
    private int YOffset => _ghostController.State == GhostState.Eaten ? 5 : _ghostOffset;

    public Ghost(Texture2D atlas, Level level, int ghostOffset, GhostController controller) : base(atlas, controller,
        level, true, false)
    {
        _blinkingOffset = 0;
        _blinking = false;
        _ghostOffset = ghostOffset;
        _frightenTimer = 3000;
        _blinkSpeed = 100;
        _currentBlinkTimer = 0;
        _currentFrightenTime = 0;

        _ghostController = controller;
        SetTextureOffset(3, 1 + _ghostOffset);
        SetLayerDepth(.6f);
        _animationSpeed = 100;
        _currentAnimationTime = 0;
        SetTileLocation(0, 0);
        SetSpeed(.1f);
        _currentAnimationFrame = 0;
    }

    protected override void OnUpdate(GameTime gameTime)
    {
        _currentAnimationTime += gameTime.ElapsedGameTime.Milliseconds;

        if (CheckIfCaughtPlayer(gameTime))
        {
            Level.InactiveAllEnemies();
            Level.Player().Die(gameTime);
        }
        
        if (IsFrighted)
        {
            Frightened(gameTime);
        }

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
    
    public void Frighten()
    {
        if (_ghostController.State is GhostState.Chase or GhostState.Frightened or GhostState.Scatter)
        {
            _ghostController.ChangeState(GhostState.Frightened);
            SetSpeed(.07f);
        }
    }

    public override bool Die(GameTime gameTime)
    {
        base.Die(gameTime);
        _dying = false;
        _currentFrightenTime = 0;
        _blinking = false;
        _currentBlinkTimer = 0;
        _blinkingOffset = 0;
        _ghostController.ChangeState(GhostState.Eaten);
        SetSpeed(.15f);
        return true;
    }


    protected override bool CanWalkOnTile(Tile? tile)
    {
        return tile == null || (base.CanWalkOnTile(tile) && (!tile.IsGhostDoor() || (_ghostController.State is GhostState.LeavingHome or GhostState.Eaten ) ));
    }

    protected override void OnDraw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (Debugger.Debug)
        {
            spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(),
                new Rectangle(new Point(_ghostController.TargetTile.X * 16, _ghostController.TargetTile.Y * 16),
                    new Point(16)), Color.Fuchsia);
            spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(),
                new Rectangle(
                    new Point(_ghostController.DefaultTargetTile.X * 16,
                        _ghostController.DefaultTargetTile.Y * 16), new Point(16)), Color.Aqua);
            if (_ghostController.NextTile.HasValue)
            {
                spriteBatch.Draw(DebugTextureCreator.Current.CreateTexture(),
                    new Rectangle(
                        new Point(_ghostController.NextTile.Value.X * 16, _ghostController.NextTile.Value.Y * 16),
                        new Point(16)), Color.Green);
            }
        }

        base.OnDraw(gameTime, spriteBatch);
    }

    private int ComputedFrightenTime()
    {
        return _frightenTimer;
    }

    private void Frightened(GameTime gameTime)
    {
        if (_blinking)
        {
            _currentBlinkTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (_currentBlinkTimer > _blinkSpeed)
            {
                _currentBlinkTimer = 0;
                _blinkingOffset = ++_blinkingOffset % 2;
            }
        }

        _currentFrightenTime += gameTime.ElapsedGameTime.Milliseconds;
        if (_currentFrightenTime > ComputedFrightenTime())
        {
            _currentFrightenTime = 0;
            _ghostController.ChangeState(GhostState.Chase);
            SetInvincibility(false);
            _blinking = false;
            _currentBlinkTimer = 0;
            _blinkingOffset = 0;
            SetSpeed(.1f);
        }
        else if (ComputedFrightenTime() - _currentFrightenTime < 1000)
        {
            _blinking = true;
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

            if (IsFrighted)
            {
                SetTextureOffset(_currentAnimationFrame + 3 + (_blinking ? _blinkingOffset*2 : 0), 5);
            }
            else
            {
                switch (CurrentState!.Direction)
                {
                    case Direction.Up:
                        if (IsFlippedVertically())
                        {
                            FlipVertically();
                        }

                        animationOffset = 4;
                        break;
                    case Direction.Down:
                        if (IsFlippedVertically())
                        {
                            FlipVertically();
                        }

                        animationOffset = 2;
                        break;
                    case Direction.Left:
                        if (!IsFlippedHorizontally())
                        {
                            FlipHorizontally();
                        }

                        animationOffset = 0;
                        break;
                    case Direction.Right:
                        if (IsFlippedHorizontally())
                        {
                            FlipHorizontally();
                        }

                        animationOffset = 0;
                        break;
                }
                SetTextureOffset(_currentAnimationFrame + animationOffset + 3, 1 + YOffset);
            }
            _currentAnimationTime = 0;
        }
    }
    
    
    private bool CheckIfCaughtPlayer(GameTime gameTime)
    {
        
            // If the player and the actor share the same tile
            if (TileLocation == Level.Player().TileLocation)
            {
                // If the actor can be killed on touch and is not invincible
                if (IsFrighted && !IsInvincible)
                {
                    Level.SetWait(1000);
                    Die(gameTime);
                    return false;
                }
                // If the actor can not be killed on touch and the play can be killed on touch and the player is not invincible
                if(!IsFrighted && !WasEaten && !Level.Player().IsInvincible)
                {
                    return true;
                }
                return false;
            }
            return false;
    }    
    
    
}