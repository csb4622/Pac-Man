using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PacMan;

public class Session
{
    private readonly Texture2D _atlas;
    private readonly SpriteFont _font;
    private Level? _currentLevel;
    private int _lives;
    private int _score;
    private string _highScoreMessage;
    private string _highScoreLabelMessage;
    private Vector2 _highScoreLabelMessageLocation;
    private Vector2 _highScoreMessageLocation;
    private string _oneUpMessage;
    private Vector2 _onUpMessageLocation;
    private Vector2 _scoreLocation;
    private readonly int _oneUpBlinkSpeed;
    private int _currentOneUpBlinkTimer;
    private bool _drawOneUpMessage;

    public Session(Texture2D atlas, SpriteFont font)
    {
        _oneUpBlinkSpeed = 200;
        _currentOneUpBlinkTimer = 0;
        _drawOneUpMessage = true;
        _score = 0;
        _lives = 2;
        _atlas = atlas;
        _font = font;
        
        _highScoreMessage = "3333361";
        _highScoreLabelMessage = "HIGH  SCORE";
        _highScoreLabelMessageLocation = Vector2.Zero;
        _highScoreMessageLocation = Vector2.Zero;
        _oneUpMessage = "1UP";
        _onUpMessageLocation = Vector2.Zero;
        _scoreLocation = Vector2.Zero;
    }

    public void SetupHUD(Viewport viewport)
    {
        var screenDimensions = new Point(viewport.Width, viewport.Height);
        
        var highScoreMessageSize = _font.MeasureString(_highScoreMessage);
        var highScoreLabelMessageSize = _font.MeasureString(_highScoreLabelMessage);
        
        _highScoreLabelMessageLocation = new Vector2((screenDimensions.X/2f)-((highScoreLabelMessageSize.X*.9f)/2), 1);
        
        _highScoreMessageLocation = new Vector2((screenDimensions.X/2f)-((highScoreMessageSize.X*.9f)/2), 1+((highScoreLabelMessageSize.Y*.9f)-2));
        
        _onUpMessageLocation = new Vector2(_highScoreLabelMessageLocation.X-100, _highScoreLabelMessageLocation.Y);

        _scoreLocation = new Vector2(_onUpMessageLocation.X-10, _highScoreMessageLocation.Y);
    }

    public void IncreaseScore(int amount)
    {
        _score += amount;
    }

    public void GainLife()
    {
        _lives++;
    }

    public void LoseLife()
    {
        _lives--;
    }
    
    public void Reset()
    {
        _currentLevel.CleanUp();
        _currentLevel = null;
        _currentOneUpBlinkTimer = 0;
        _drawOneUpMessage = true;
        _score = 0;
        _lives = 2;
    }
    
    public void NextLevel(int? level = null)
    {
        if (!level.HasValue)
        {
            if (_currentLevel == null)
            {
                level = 1;
            }
            else
            {
                level = _currentLevel.GetLevelNumber() + 1;
            }
        }
        _currentLevel?.CleanUp();
        _currentLevel = new Level(this, _atlas, _font, level.Value, 16);
        _currentLevel.Start();
    }

    public void Update(GameTime gameTime)
    {
        _currentLevel.Update(gameTime);
    }

    public bool FinishedCurrentLevel()
    {
        return _currentLevel?.Finished() ?? false;
    }

    public bool IsGameOver()
    {
        return _lives < 0;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        DrawHud(gameTime, spriteBatch);
        _currentLevel.Draw(gameTime, spriteBatch);
    }
    
    private void DrawHud(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _currentOneUpBlinkTimer += gameTime.ElapsedGameTime.Milliseconds;
        if (_currentOneUpBlinkTimer > _oneUpBlinkSpeed)
        {
            _currentOneUpBlinkTimer = 0;
            _drawOneUpMessage = !_drawOneUpMessage;
        }
        spriteBatch.DrawString(
            _font,
            _highScoreLabelMessage,
            _highScoreLabelMessageLocation,
            Color.White,
            0f,
            Vector2.Zero,
            .9f, 
            SpriteEffects.None,
            1f);
        spriteBatch.DrawString(
            _font,
            _highScoreMessage,
            _highScoreMessageLocation,
            Color.White,
            0f,
            Vector2.Zero,
            .9f, 
            SpriteEffects.None,
            1f);
        if (_drawOneUpMessage)
        {
            spriteBatch.DrawString(
                _font,
                _oneUpMessage,
                _onUpMessageLocation,
                Color.White,
                0f,
                Vector2.Zero,
                .9f,
                SpriteEffects.None,
                1f);
        }

        spriteBatch.DrawString(
            _font,
            _score.ToString(),
            _scoreLocation,
            Color.White,
            0f,
            Vector2.Zero,
            .9f, 
            SpriteEffects.None,
            1f);
        
        for (var i = 0; i < _lives; ++i)
        {
            spriteBatch.Draw(
                _atlas,
                new Vector2(32+(i*16),
                    ((_currentLevel.Height-2)*16)+8),
                new Rectangle(new Point(16,16), new Point(16)),
                Color.White,
                0f,
                Vector2.Zero, 
                Vector2.One, 
                SpriteEffects.FlipHorizontally,
                1);
        }        
    }
}