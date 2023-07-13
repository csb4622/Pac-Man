using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PacMan.Screens;

namespace PacMan;

public class PacManGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _atlas;
    private SpriteFont _font;

    private Session _session;
    private Menu _menu;
    
    private GameState _state;

    public PacManGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
        
        
        _state = GameState.Menu;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        _atlas = Content.Load<Texture2D>("Sheet");
        _font = Content.Load<SpriteFont>("Font");

        _session = new Session(_atlas, _font);
        _menu = new Menu(_atlas, _font);
        DebugTextureCreator.Current.Initialize(GraphicsDevice);


        _graphics.PreferredBackBufferWidth = 432;
        _graphics.PreferredBackBufferHeight = 432;
        _graphics.ApplyChanges();
        
        _session.SetupHUD(_graphics.GraphicsDevice.Viewport);
        _menu.Reset(_graphics.GraphicsDevice.Viewport);
    }

    protected override void Update(GameTime gameTime)
    {
        Keyboard.GetState();
        if (Keyboard.HasBeenPressed(Keys.Escape))
        {
            Exit();
        }
        if (Keyboard.HasBeenPressed(Keys.Space))
        {
            Debugger.Debug = !Debugger.Debug;
        }        

        // TODO: Add your update logic here
        switch (_state)
        {
            case GameState.Menu:
                ProcessMenu(gameTime);
                break;
            case GameState.Paused:
                ProcessPause(gameTime);
                break;
            case GameState.Playing:
                ProcessFrame(gameTime);
                break;
            case GameState.Passed:
                ProcessPassed(gameTime);
                break;            
            case GameState.GameOver:
                ProcessGameOver(gameTime);
                break;            
        }
        base.Update(gameTime);
    }

    private void ProcessMenu(GameTime gameTime)
    {
        _menu.Update(gameTime);
        if (_menu.ShouldStartGame())
        {
            _state = GameState.Playing;
            _session.NextLevel();
            _menu.Reset(_graphics.GraphicsDevice.Viewport);
        }
    }
    private void ProcessPause(GameTime gameTime)
    {
        if (Keyboard.HasBeenPressed(Keys.Enter))
        {
            _state = GameState.Playing;
        }
    }
    private void ProcessPassed(GameTime gameTime)
    {
        _session.NextLevel();
        _state = GameState.Playing;
    }
    private void ProcessFrame(GameTime gameTime)
    {
        if (Keyboard.HasBeenPressed(Keys.Enter))
        {
            _state = GameState.Paused;
        }
        else
        {
            if (_session.FinishedCurrentLevel())
            {
                _state = GameState.Passed;
            }
            else if(_session.IsGameOver())
            {
                _state = GameState.GameOver;
            }
            else
            {
                _session.Update(gameTime);
            }
        }
        
    }
    private void ProcessGameOver(GameTime gameTime)
    {
        _session.Reset();
        _state = GameState.Menu;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        // TODO: Add your drawing code here
        _spriteBatch.Begin(SpriteSortMode.FrontToBack);
        switch (_state)
        {
            case GameState.Menu:
                _menu.Draw(gameTime, _spriteBatch);
                break;
            case GameState.Paused:
                _spriteBatch.DrawString(_font, "Press Enter To Resume", new Vector2(300, 200), Color.White);
                break;
            case GameState.Passed:
                break;
            case GameState.Playing:
                _session.Draw(gameTime, _spriteBatch);
                break;
            case GameState.GameOver:
                _spriteBatch.DrawString(_font, "Press Enter To Try Again", new Vector2(300, 200), Color.White);
                break;            
        }
        _spriteBatch.End();
        base.Draw(gameTime);
    }

}