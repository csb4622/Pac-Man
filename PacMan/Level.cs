#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PacMan.Actors;
using PacMan.Controllers;

namespace PacMan;

public class Level
{
    private int _currentWaitTime;
    private bool _waiting;
    private int _waittime;
    private Action? _waitCallback;
    
    private bool _ready;
    private string _readyMessage;
    private int _currentReadytimer;
    private int _readyDelay;
    private Vector2 _readyLocation;
    private readonly Session _session;
    private Texture2D _atlas;
    private SpriteFont _font;
    private readonly int _level;
    private readonly int _width;
    private readonly int _height;
    private readonly int _tileSize;
    private readonly Tiles _tileCreator;
    private bool _finished;

    private readonly Tile?[] _tiles;
    private readonly IList<Pellet> _superPellets;
    private readonly Pellet?[] _pellets;
    private readonly IList<Actor> _actors;

    private IList<string> _debugInfo;
    private int _remainingPellets;
    private int _startingPelletsAmount;
    private Actor? _player;

    private int _superPelletBlinkSpeed;
    private int _currentSuperPelletBlinkTimer;
    private Color _currentSuperPelletColor;
    

    

    public int Width => _width;
    public int Height => _height;

    public Level(Session session, Texture2D atlas, SpriteFont font, int level, int tileSize)
    {
        _startingPelletsAmount = 0;
        _superPellets = new List<Pellet>();
        _superPelletBlinkSpeed = 150;
        _currentSuperPelletBlinkTimer = 0;
        _currentSuperPelletColor = Color.White;
        
        _currentWaitTime = 0;
        _waiting = false;
        _waittime = 0;
        _readyMessage = "READY!";
        _readyLocation = Vector2.Zero;
        _currentReadytimer = 0;
        _readyDelay = 1000;
        _ready = false;
        _session = session;
        _debugInfo = new List<string>();
        _atlas = atlas;
        _font = font;
        _level = level;
        _width = 27;
        _height = 27;
        _tileSize = tileSize;
        _remainingPellets = 0;
        _finished = false;

        _tileCreator = new Tiles(_atlas, _tileSize);

        _tiles = new Tile[_width * _height];
        _actors = new List<Actor>(5);
        _pellets = new Pellet[(_width) * (_height)];
    }

    public int PelletsEaten()
    {
        return _startingPelletsAmount - _remainingPellets;
    }
    
    public bool IsWall(Point point)
    {
        return IsWall(point.X, point.Y);
    }

    public bool IsWall(int x, int y)
    {
        return _tiles[y * _width + x]?.IsWall() ?? false;
    }
    
    public bool Finished()
    {
        return _finished;
    }
    
    public int GetLevelNumber()
    {
        return _level;
    }

    public void SetTile(int x, int y, Tile tile)
    {
        tile.SetLocation(x, y);
        _tiles[y * _width + x] = tile;
    }

    public Tile? GetTile(int x, int y)
    {
        return y >= _height || y < 0 || x >= _width || x < 0 ? null : _tiles[y * _width + x];
    }
    public Tile? GetTile(Point location)
    {
        return GetTile(location.X, location.Y);
    }

    public Pellet? GetPellet(int x, int y)
    {
        return y >= _height || y < 0 || x >= _width || x < 0 ? null : _pellets[y * _width + x];
    }
    public Pellet? GetPellet(Point location)
    {
        return GetPellet(location.X, location.Y);
    }

    public void RemovePellet(int x, int y)
    {
        if (y >= _height || y < 0 || x >= _width || x < 0)
        {
            return;
        }

        var pellet = GetPellet(x, y);
        if (pellet != null)
        {
            _session.IncreaseScore(pellet.IsSuper() ? 50 : 10 );
            _pellets[y * _width + x] = null;
            _remainingPellets--;
            _superPellets.Remove(pellet);
        }

    }

    public void RemovePellet(Point location)
    {
        RemovePellet(location.X, location.Y);
    }

    public void AddDebugInfo(string item)
    {
        _debugInfo.Add(item);
    }
    

    private void AddPellets()
    {
        _remainingPellets = 0;
        for (var y = 0; y < _height; ++y)
        {
            for (var x = 0; x < _width; ++x)
            {
                var tile = GetTile(x, y);
                if (tile == null)
                {
                    var isSuper = ((y is 5 or 19) && (x is 3 or 23));
                    var pellet = new Pellet(_atlas, _tileSize,  isSuper, new Vector2(x, y));
                    _pellets[y * (_width) + x] = pellet;
                    if (isSuper)
                    {
                        _superPellets.Add(pellet);
                    }
                    _remainingPellets++;
                }
            }
        }

        _startingPelletsAmount = _remainingPellets;
    }
    
    private void BuildLevel()
    {
        for (var i = 0; i < _height; ++i)
        {
            SetTile(0, i, _tileCreator.Mask());
            SetTile(1, i, _tileCreator.Mask());
            SetTile(_width-2, i, _tileCreator.Mask());
            SetTile(_width-1, i, _tileCreator.Mask());
        }
        for (var i = 0; i < _height; ++i)
        {
            SetTile(i, 0, _tileCreator.Mask());
            SetTile(i, 1, _tileCreator.Mask());
            SetTile(i, _height-2, _tileCreator.Mask());
            SetTile(i, _height-1, _tileCreator.Mask());
        }        

        var xOffset = 2;
        var yOffset = 2;
        var x = xOffset;
        var y = yOffset;

        SetTile(x,y, _tileCreator.TopLeftCorner());
        var limit = x + 11;
        while(x <limit)
        {
            SetTile(++x,y, _tileCreator.Horizontal());    
        }
        SetTile(x,y, _tileCreator.DownInsideT());
        limit = x + 11;
        while(x < limit)
        {
            SetTile(++x,y, _tileCreator.Horizontal());    
        }
        SetTile(x,y, _tileCreator.TopRightCorner());

        x = xOffset;
        y++;
        
        SetTile(x,y, _tileCreator.Vertical());
        x += 11;
        SetTile(x,y, _tileCreator.Vertical());
        x += 11;
        SetTile(x,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
            
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x+21,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.UpEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.DownInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.UpEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x,y, _tileCreator.Vertical());
        x+=6;
        SetTile(x++,y, _tileCreator.Vertical());
        x+=4;
        SetTile(x++,y, _tileCreator.Vertical());
        x+=4;
        SetTile(x++,y, _tileCreator.Vertical());
        x+=5;
        SetTile(x,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.RightInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.DownEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.LeftInsideT());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());        
        SetTile(x,y, _tileCreator.BottomRightCorner());

        x = xOffset;
        y++;

        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x,y, _tileCreator.NoPellet());

        x = 0;
        y++;
        
        SetTile(x++,y, _tileCreator.SolidMask());
        SetTile(x++,y, _tileCreator.SolidMask());

        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal()); //
        SetTile(x++,y, _tileCreator.GhostDoor());  // x+=3;
        SetTile(x++,y, _tileCreator.Horizontal()); //
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.SolidMask());
        SetTile(x,y, _tileCreator.SolidMask());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        x++;
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        x++;
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x++,y, _tileCreator.NoPellet(true));
        SetTile(x,y, _tileCreator.NoPellet(true));
        
        
        x = 0;
        y++;
        
        SetTile(x++,y, _tileCreator.SolidMask());
        SetTile(x++,y, _tileCreator.SolidMask());

        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.UpEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.UpEnd());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.SolidMask());
        SetTile(x,y, _tileCreator.SolidMask());
        
        x = xOffset;
        y++;

        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.Vertical());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x,y, _tileCreator.NoPellet());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.BottomRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.DownInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        SetTile(x++,y, _tileCreator.NoPellet());
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x,y, _tileCreator.TopRightCorner());
        
        x = xOffset;
        y++;
        
        SetTile(x,y, _tileCreator.Vertical());
        x += 11;
        SetTile(x,y, _tileCreator.Vertical());
        x += 11;
        SetTile(x,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.TopRightCorner());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.TopLeftCorner());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x+=3;
        SetTile(x++,y, _tileCreator.Vertical());
        x+=6;
        SetTile(x++,y, _tileCreator.PlayerStart());
        x+=6;
        SetTile(x++,y, _tileCreator.Vertical());
        x+=3;
        SetTile(x,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.RightInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.UpEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.DownInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++,y, _tileCreator.UpEnd());
        x++;
        SetTile(x++,y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x,y, _tileCreator.LeftInsideT());
        
        x = xOffset;
        y++;
        
        SetTile(x,y, _tileCreator.Vertical());
        x += 6;
        SetTile(x,y, _tileCreator.Vertical());
        x += 5;
        SetTile(x,y, _tileCreator.Vertical());
        x += 5;
        SetTile(x,y, _tileCreator.Vertical());
        x += 6;
        SetTile(x,y, _tileCreator.Vertical());

        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.Vertical());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.UpInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x++, y, _tileCreator.DownEnd());
        x++;
        SetTile(x++,y, _tileCreator.LeftEnd());;
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.UpInsideT());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.Horizontal());
        SetTile(x++,y, _tileCreator.RightEnd());
        x++;
        SetTile(x,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x,y, _tileCreator.Vertical());
        SetTile(x+22,y, _tileCreator.Vertical());
        
        x = xOffset;
        y++;
        
        SetTile(x++,y, _tileCreator.BottomLeftCorner());
        limit = x + 21;
        while (x < limit)
        {
            SetTile(x++,y, _tileCreator.Horizontal());
        }
        SetTile(x,y, _tileCreator.BottomRightCorner());
    }

    public Point? FindPlayerStartLocation()
    {
        for (var i = 0; i < _tiles.Length; ++i)
        {
            if (_tiles[i]?.CanStartHere() ?? false)
            {
                return new Point(i % _width, i / _width);
            }
        }
        return null;
    }
    
    public Point? FindGhostDoorLocation()
    {
        for (var i = 0; i < _tiles.Length; ++i)
        {
            if (_tiles[i]?.IsGhostDoor() ?? false)
            {
                return new Point(i % _width, i / _width);
            }
        }
        return null;
    }
    
    public void Start()
    {
        BuildLevel();

        var ghostDoor = FindGhostDoorLocation();
        var readyMessageSize = _font.MeasureString(_readyMessage);
        _readyLocation = new Vector2(((ghostDoor.Value.X * _tileSize) + _tileSize / 2) - readyMessageSize.X / 2,
            (((ghostDoor.Value.Y * _tileSize )+ (3.5f * _tileSize) - readyMessageSize.Y / 2)));
        
        AddPellets();
        _player = new Actors.PacMan(_atlas, this);
        var startingLocation = FindPlayerStartLocation();
        _player.SetTileLocation(startingLocation.Value);
        SpawnGhosts();
    }
    
    public void SpawnGhosts()
    {
        var ghostDoor = FindGhostDoorLocation();
        
        var blinky = new Ghost(_atlas, this, 0, new BlinkyController(this));
        blinky.SetTileLocation(ghostDoor.Value.X, ghostDoor.Value.Y-1);
        _actors.Add(blinky);
        
        var inky = new Ghost(_atlas, this, 1, new InkyController(this, blinky));
        inky.SetTileLocation(ghostDoor.Value.X-1, ghostDoor.Value.Y+1);
        _actors.Add(inky);
        
        var pinky = new Ghost(_atlas, this, 2, new PinkyController(this));
        pinky.SetTileLocation(ghostDoor.Value.X, ghostDoor.Value.Y+1);
        _actors.Add(pinky);
        
        var clyde = new Ghost(_atlas, this, 3, new ClydeController(this));
        clyde.SetTileLocation(ghostDoor.Value.X+1, ghostDoor.Value.Y+1);
        _actors.Add(clyde);
        
    }

    public Actor Player()
    {
        return _player;
    }

    public void InactiveAllEnemies()
    {
        for (var i = 0; i < _actors.Count; ++i)
        {
            _actors[i].SetActive(false);
        }
    }

    public void FrightenAllGhostEnemies()
    {
        for (var i = 0; i < _actors.Count; ++i)
        {
            if (_actors[i] is Ghost ghost)
            {
                ghost.Frighten();
            }
        }
    }

    public void RestartPlayer()
    {
        _actors.Clear();
        _session.LoseLife();
        var startingPosition = FindPlayerStartLocation();
        _player?.SetTileLocation(startingPosition.Value);
        SpawnGhosts();
        _startingPelletsAmount = _remainingPellets;
        _ready = false;        
    }


    private void Win()
    {
        for (var i = 0; i < _tiles.Length; ++i)
        {
            _tiles[i]?.StartBlink();
        }
        SetWait(2000, Finish);
    }

    private void Finish()
    {
        _finished = true;
    }
    
    public void Update(GameTime gameTime)
    {
        _debugInfo.Clear();

        if (_ready)
        {
            _currentSuperPelletBlinkTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (_currentSuperPelletBlinkTimer > _superPelletBlinkSpeed)
            {
                _currentSuperPelletBlinkTimer = 0;
                _currentSuperPelletColor = _currentSuperPelletColor == Color.White ? Color.Black : Color.White;
                for (var i = 0; i < _superPellets.Count; ++i)
                {
                    _superPellets[i].SetColor(_currentSuperPelletColor);
                }
            }
            if (_waiting)
            {
                Wait(gameTime);
                return;
            }
            ProcessFrame(gameTime);
        }
        else
        { 
            _currentReadytimer += gameTime.ElapsedGameTime.Milliseconds;

            if (_currentReadytimer > _readyDelay)
            {
                _currentReadytimer = 0;
                _ready = true;
            }
        }

     
    }

    public void ProcessFrame(GameTime gameTime)
    {
        if (_remainingPellets < 1)
        {
            Win();
        }

        if (_player != null)
        {
            _player.Update(gameTime);
            KeepActorInBounds(_player);
        }

        for (var i = 0; i < _actors.Count; ++i)
        {
            _actors[i].Update(gameTime);
            KeepActorInBounds(_actors[i]);
        } 
    }

    private void KeepActorInBounds(Actor actor)
    {
        if (actor.TileLocation.X <= 0)
        {
            var oldLocation = actor.Location;
            actor.SetLocation(oldLocation.X+((_width-2)*(_tileSize)), oldLocation.Y);
        }
        else if (actor.TileLocation.X >= _width-2)
        {
            var oldLocation = actor.Location;
            actor.SetLocation(oldLocation.X-((_width-2)*(_tileSize)), oldLocation.Y);
        }
        if (actor.TileLocation.Y <= 0)
        {
            var oldLocation = actor.Location;
            actor.SetLocation(oldLocation.X, oldLocation.Y+((_height-2)*(_tileSize)));
        }
        else if (actor.TileLocation.Y >= _height-2)
        {
            var oldLocation = actor.Location;
            actor.SetLocation(oldLocation.X, oldLocation.Y-((_height-2)*(_tileSize)));
        }        
        
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        for (var i = 0; i < _tiles.Length; ++i)
        {
            _tiles[i]?.Draw(gameTime, spriteBatch);
        }
        for (var i = 0; i < _pellets.Length; ++i)
        {
            _pellets[i]?.Draw(gameTime, spriteBatch);
        }
        
        _player?.Draw(gameTime, spriteBatch);
        
        for (var i = 0; i < _actors.Count; ++i)
        {
            _actors[i]?.Draw(gameTime, spriteBatch);
        }

        if (!_ready)
        {
            spriteBatch.DrawString(_font, _readyMessage, _readyLocation, Color.Yellow);
        }
        
        if (Debugger.Debug)
        {
            for (var i = 0; i < _debugInfo.Count; ++i)
            {
                spriteBatch.DrawString(_font, _debugInfo[i], new Vector2(500, 20 + (i * 20)), Color.White);
            }
        }
    }

    public void SetWait(int milliseconds, Action callback = null)
    {
        _waiting = true;
        _waittime = milliseconds;
        _waitCallback = callback;
    }

    private void Wait(GameTime gameTime)
    {
        _currentWaitTime += gameTime.ElapsedGameTime.Milliseconds;
        if (_currentWaitTime > _waittime)
        {
            _currentWaitTime = 0;
            _waittime = 0;
            _waiting = false;
            _waitCallback?.Invoke();
            _waitCallback = null;
        }
    }

    public void CleanUp()
    {
        _ready = false;
        _actors.Clear();
        _player = null;
        Array.Clear(_tiles);
        _superPellets.Clear();
        Array.Clear(_pellets);
        _currentReadytimer = 0;
        _readyLocation = Vector2.Zero;
        _currentWaitTime = 0;
        _waiting = false;
        _waittime = 0;
    }
}