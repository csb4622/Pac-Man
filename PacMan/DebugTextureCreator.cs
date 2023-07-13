using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PacMan;

public class DebugTextureCreator
{
    private GraphicsDevice _device; 
    private static DebugTextureCreator _instance;
    private Texture2D _debugTexture;
    
    public static DebugTextureCreator Current => _instance ??= new DebugTextureCreator();

    public void Initialize(GraphicsDevice device)
    {
        _device = device;
    }

    public Texture2D CreateTexture()
    {
        if (_debugTexture == null)
        {
            _debugTexture = new Texture2D(_device, 1, 1);
            _debugTexture.SetData<Color>(new Color[] { Color.White });
        }
        return _debugTexture;
    }
    
}