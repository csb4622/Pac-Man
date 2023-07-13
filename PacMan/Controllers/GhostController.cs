using System;
using Microsoft.Xna.Framework;
using PacMan.Actors;

namespace PacMan.Controllers;

public abstract class GhostController : Controller
{
    private GhostState _state;
    private readonly Level _level;
    private Direction? _previousDirection;
    private Point _targetTile;
    private Point? _nextTile;
    private Point _defaultTargetTile;
    private int _pelletCheck;
    private Point _ghostDoor;
    private Point _playerStart;
    private Actor _player;
    private int _maxStateChange;
    private int _numberOfStateChanges;
    private int _scatterTimer;
    private int _chaseTimer;
    private int _stateTimer;
    private int _currentStateTime;

    public GhostState State => _state;
    protected Actor Player => _player;


    public Point DefaultTargetTile => _defaultTargetTile;
    public Point TargetTile => _targetTile;
    public Point? NextTile => _nextTile;

    public GhostController(Level level, Point defaultTargetTile, int pelletCheck)
    {
        _level = level;
        _defaultTargetTile = defaultTargetTile;
        _pelletCheck = pelletCheck;
        _maxStateChange = 3;
        _scatterTimer = 5000;
        _chaseTimer = 20000;
        _stateTimer = _scatterTimer;
        Reset();
    }

    public void Reset()
    {
        _numberOfStateChanges = 0;
        _currentStateTime = 0;
        _state = GhostState.Home;
        _targetTile = _defaultTargetTile;
        _ghostDoor = _level.FindGhostDoorLocation()!.Value;
        _playerStart = _level.FindPlayerStartLocation()!.Value;
        _player = _level.Player();
    }

    public void ChangeState(GhostState newState)
    {
        _state = newState;
        switch (_state)
        {
            case GhostState.Chase:
            case GhostState.Frightened:
            case GhostState.Scatter:
                SwitchDirection();
                break;
        }
    }

    private bool InStartArea()
    {
        return ((Me.TileLocation.X > _ghostDoor.X - 2 && Me.TileLocation.X < _ghostDoor.X + 2) &&
                Me.TileLocation.Y == _ghostDoor.Y - 1) ||
               ((Me.TileLocation.X > _playerStart.X - 2 && Me.TileLocation.X < _playerStart.X + 2) &&
                Me.TileLocation.Y == _playerStart.Y);
    }

    public override ControllerState GetControllerState(ControllerState? previousState, GameTime gameTime)
    {
        if (_numberOfStateChanges <= _maxStateChange && _state is GhostState.Chase or GhostState.Scatter)
        {
            _currentStateTime += gameTime.ElapsedGameTime.Milliseconds;

            if (_currentStateTime > _stateTimer)
            {
                _currentStateTime = 0;
                switch (_state)
                {
                    case GhostState.Chase:
                        ChangeState(GhostState.Scatter);
                        _numberOfStateChanges++;
                        _stateTimer = _scatterTimer;
                        break;
                    case GhostState.Scatter:
                        ChangeState(GhostState.Chase);
                        _numberOfStateChanges++;
                        _stateTimer = _chaseTimer;
                        break;
                }
            }
        }
        
        _previousDirection = previousState?.Direction ?? Direction.Up;
        Direction? currentDirection = null;

        if (_state == GhostState.Home)
        {
            ProcessHomeState(gameTime);
            currentDirection = Direction.Up;
        }
        if (!IsOutOfBound())
        {
            while (!currentDirection.HasValue)
            {
                while (!_nextTile.HasValue)
                {
                    switch (_state)
                    {
                        case GhostState.LeavingHome:
                            ProcessLeavingHomeState();
                            break;
                        case GhostState.Frightened:
                            ProcessFrightenedState();
                            break;
                        case GhostState.Eaten:
                            ProcessEatenStates();
                            break;
                        case GhostState.Chase:
                        case GhostState.Scatter:
                            ProcessDefaultStates();
                            break;
                    }
                }
                if (_nextTile.HasValue)
                {
                    currentDirection = MoveToTile(_nextTile!.Value);
                }
            }
        }
        else
        {
            _nextTile = null;
            currentDirection = previousState!.Direction;
        }

        return new ControllerState(currentDirection!.Value);
    }

    protected abstract Point GetChaseTargetTile();

    protected int GetManhattanDistance(Point from, Point to)
    {
        return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
    }

    #region ProcessStates

    private void ProcessDefaultStates()
    {
        GetNewTargetTile();
        PathToTargetTile();
    }

    private void ProcessEatenStates()
    {
        GetNewTargetTile();
        if (Me.TileLocation == _targetTile)
        {
            Me.SetSpeed(.1f);
            Me.SetInvincibility(false);
            ChangeState(GhostState.LeavingHome);
            return;
        }
        PathToTargetTileWhenEaten();
    }

    private void ProcessFrightenedState()
    {
        MakeRandomMove();
    }

    private void ProcessLeavingHomeState()
    {
        GetNewTargetTile();
        if (_targetTile == Me.TileLocation)
        {
            ChangeState(GhostState.Scatter);
        }

        PathToTargetTileNoRestriction();
    }

    private void ProcessHomeState(GameTime gameTime)
    {
        if (_level.PelletsEaten() >= _pelletCheck)
        {
            ChangeState(GhostState.LeavingHome);
        }
    }

    #endregion

    private void SetTileFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                _nextTile = new Point(Me.TileLocation.X, Me.TileLocation.Y - 1);
                break;
            case Direction.Down:
                _nextTile = new Point(Me.TileLocation.X, Me.TileLocation.Y + 1);
                break;
            case Direction.Left:
                _nextTile = new Point(Me.TileLocation.X - 1, Me.TileLocation.Y);
                break;
            case Direction.Right:
                _nextTile = new Point(Me.TileLocation.X + 1, Me.TileLocation.Y);
                break;
        }
    }

    private void SwitchDirection()
    {
        if (_previousDirection.HasValue)
        {
            switch (_previousDirection)
            {
                case Direction.Up:
                    SetTileFromDirection(Direction.Down);
                    break;
                case Direction.Down:
                    SetTileFromDirection(Direction.Up);
                    break;
                case Direction.Left:
                    SetTileFromDirection(Direction.Right);
                    break;
                case Direction.Right:
                    SetTileFromDirection(Direction.Left);
                    break;
            }
        }
    }

    private Direction? MoveToTile(Point tileLocation)
    {
        var tile = _level.GetTile(tileLocation);
        if (tile?.IsWall() ?? false)
        {
            _nextTile = null;
            return null;
        }

        if (Me.TileLocation == tileLocation)
        {
            _nextTile = null;
            return null;
        }

        return GetDirectionFromLocation(tileLocation);
    }

    private Direction GetDirectionFromLocation(Point tile)
    {
        if (tile.Y < Me.TileLocation.Y)
        {
            return Direction.Up;
        }

        if (tile.X < Me.TileLocation.X)
        {
            return Direction.Left;
        }

        if (tile.Y > Me.TileLocation.Y)
        {
            return Direction.Down;
        }

        if (tile.X > Me.TileLocation.X)
        {
            return Direction.Right;
        }

        return _previousDirection.Value;
    }

    private void GetNewTargetTile()
    {
        switch (_state)
        {
            case GhostState.Home:
                _targetTile = Me.TileLocation;
                break;
            case GhostState.LeavingHome:
                _targetTile = new Point(_ghostDoor.X, _ghostDoor.Y - 1);
                break;
            case GhostState.Scatter:
                _targetTile = _defaultTargetTile;
                break;
            case GhostState.Frightened:
                _targetTile = _defaultTargetTile;
                break;
            case GhostState.Chase:
                _targetTile = GetChaseTargetTile();
                break;
            case GhostState.Eaten:
                _targetTile = GetHomeTile();
                break;
        }
    }

    private Point GetHomeTile()
    {
        return new Point(_ghostDoor.X, _ghostDoor.Y + 1);
    }

    private void MakeRandomMove()
    {
        var direction = _previousDirection.GetValueOrDefault(Direction.Up);
        switch (direction)
        {
            case Direction.Up:
                direction = GetNewRandomDirection(Direction.Up, Direction.Left, Direction.Right);
                break;
            case Direction.Down:
                direction = GetNewRandomDirection(Direction.Down, Direction.Left, Direction.Right);
                break;
            case Direction.Left:
                direction = GetNewRandomDirection(Direction.Up, Direction.Down, Direction.Left);
                break;
            case Direction.Right:
                direction = GetNewRandomDirection(Direction.Up, Direction.Down, Direction.Right);
                break;
        }

        SetTileFromDirection(direction);
    }

    private bool IsOutOfBound()
    {
        return Me.TileLocation.X is < 2 or > 23 || Me.TileLocation.Y is < 2 or > 23;
    }

    private Direction GetNewRandomDirection(params Direction[] directions)
    {
        var index = Random.Shared.Next(0, directions.Length);
        return directions[index];
    }

    private void PathToTargetTile()
    {
        var minDistanceToTarget = int.MaxValue;
        var direction = _previousDirection.GetValueOrDefault(Direction.Up);

        var upTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTile = _level.GetTile(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTile = _level.GetTile(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTilePoint = upTile != null && upTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTilePoint = downTile != null && downTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTilePoint = leftTile != null && leftTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTilePoint = rightTile != null && rightTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTileManhattanDistance = !upTilePoint.HasValue || _previousDirection!.Value == Direction.Down || InStartArea()
            ? int.MaxValue
            : GetManhattanDistance(upTilePoint!.Value, _targetTile);
        var downTileManhattanDistance = !downTilePoint.HasValue || _previousDirection!.Value == Direction.Up || InStartArea()
            ? int.MaxValue
            : GetManhattanDistance(downTilePoint!.Value, _targetTile);
        var leftTileManhattanDistance = !leftTilePoint.HasValue || _previousDirection!.Value == Direction.Right
            ? int.MaxValue
            : GetManhattanDistance(leftTilePoint!.Value, _targetTile);
        var rightTileManhattanDistance = !rightTilePoint.HasValue || _previousDirection!.Value == Direction.Left
            ? int.MaxValue
            : GetManhattanDistance(rightTilePoint!.Value, _targetTile);


        if (rightTileManhattanDistance < minDistanceToTarget)
        {
            minDistanceToTarget = rightTileManhattanDistance;
            direction = Direction.Right;
        }

        if (downTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = downTileManhattanDistance;
            direction = Direction.Down;
        }

        if (leftTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = leftTileManhattanDistance;
            direction = Direction.Left;
        }

        if (upTileManhattanDistance <= minDistanceToTarget)
        {
            direction = Direction.Up;
        }

        SetTileFromDirection(direction);
    }
    private void PathToTargetTileWhenEaten()
    {
        var minDistanceToTarget = int.MaxValue;
        var direction = _previousDirection.GetValueOrDefault(Direction.Up);

        var upTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTile = _level.GetTile(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTile = _level.GetTile(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTilePoint = upTile != null && upTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTilePoint = downTile != null && downTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTilePoint = leftTile != null && leftTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTilePoint = rightTile != null && rightTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTileManhattanDistance = !upTilePoint.HasValue || _previousDirection!.Value == Direction.Down
            ? int.MaxValue
            : GetManhattanDistance(upTilePoint!.Value, _targetTile);
        var downTileManhattanDistance = !downTilePoint.HasValue || _previousDirection!.Value == Direction.Up
            ? int.MaxValue
            : GetManhattanDistance(downTilePoint!.Value, _targetTile);
        var leftTileManhattanDistance = !leftTilePoint.HasValue || _previousDirection!.Value == Direction.Right
            ? int.MaxValue
            : GetManhattanDistance(leftTilePoint!.Value, _targetTile);
        var rightTileManhattanDistance = !rightTilePoint.HasValue || _previousDirection!.Value == Direction.Left
            ? int.MaxValue
            : GetManhattanDistance(rightTilePoint!.Value, _targetTile);


        if (rightTileManhattanDistance < minDistanceToTarget)
        {
            minDistanceToTarget = rightTileManhattanDistance;
            direction = Direction.Right;
        }

        if (downTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = downTileManhattanDistance;
            direction = Direction.Down;
        }

        if (leftTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = leftTileManhattanDistance;
            direction = Direction.Left;
        }

        if (upTileManhattanDistance <= minDistanceToTarget)
        {
            direction = Direction.Up;
        }

        SetTileFromDirection(direction);
    }    
    
    private void PathToTargetTileNoRestriction()
    {
        var minDistanceToTarget = int.MaxValue;
        var direction = _previousDirection.GetValueOrDefault(Direction.Up);

        var upTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTile = _level.GetTile(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTile = _level.GetTile(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTile = _level.GetTile(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTilePoint = upTile != null && upTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y - 1);
        var downTilePoint = downTile != null && downTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X, Me.TileLocation.Y + 1);
        var leftTilePoint = leftTile != null && leftTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X - 1, Me.TileLocation.Y);
        var rightTilePoint = rightTile != null && rightTile.IsWall()
            ? (Point?)null
            : new Point(Me.TileLocation.X + 1, Me.TileLocation.Y);
        var upTileManhattanDistance = !upTilePoint.HasValue 
            ? int.MaxValue
            : GetManhattanDistance(upTilePoint!.Value, _targetTile);
        var downTileManhattanDistance = !downTilePoint.HasValue 
            ? int.MaxValue
            : GetManhattanDistance(downTilePoint!.Value, _targetTile);
        var leftTileManhattanDistance = !leftTilePoint.HasValue 
            ? int.MaxValue
            : GetManhattanDistance(leftTilePoint!.Value, _targetTile);
        var rightTileManhattanDistance = !rightTilePoint.HasValue 
            ? int.MaxValue
            : GetManhattanDistance(rightTilePoint!.Value, _targetTile);


        if (rightTileManhattanDistance < minDistanceToTarget)
        {
            minDistanceToTarget = rightTileManhattanDistance;
            direction = Direction.Right;
        }

        if (downTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = downTileManhattanDistance;
            direction = Direction.Down;
        }

        if (leftTileManhattanDistance <= minDistanceToTarget)
        {
            minDistanceToTarget = leftTileManhattanDistance;
            direction = Direction.Left;
        }

        if (upTileManhattanDistance <= minDistanceToTarget)
        {
            direction = Direction.Up;
        }

        SetTileFromDirection(direction);
    }

    
}