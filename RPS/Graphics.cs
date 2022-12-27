using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RPS.Model;
using System.Collections.Generic;
namespace RPS;
public static class Graphics
{
    #region Properties
    public static Texture2D Paper, Rock, Scissors, Tile;
    public static Rectangle ScreenResolution = new Rectangle(0, 0, 1024, 768);
    public static Rectangle GameBounds { get; set; } = new Rectangle(0, 0, (int)(ScreenResolution.Width * .8f), ScreenResolution.Height);
    public static Color PaperColor = new Color(0, 224, 213), RockColor = new Color(255, 232, 74), ScissorsColor = new Color(241, 186, 244);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new();
    public static SpriteFont Font;

    public static GraphicsDeviceManager GraphicsDeviceManager;
    public static SpriteBatch SpriteBatch;
    #endregion

    #region Initialization
    public static void LoadContent(ContentManager contentManager)
    {
        Graphics.Rock = contentManager.Load<Texture2D>("gfx/Rock_32px");
        Graphics.Paper = contentManager.Load<Texture2D>("gfx/Paper_32px");
        Graphics.Scissors = contentManager.Load<Texture2D>("gfx/Scissors_32px");
        Graphics.Tile = contentManager.Load<Texture2D>("gfx/Tile_16x16");
        Graphics.TypeTextures.Add(RpsType.Rock, Graphics.Rock);
        Graphics.TypeTextures.Add(RpsType.Paper, Graphics.Paper);
        Graphics.TypeTextures.Add(RpsType.Scissors, Graphics.Scissors);
        Graphics.Font = contentManager.Load<SpriteFont>("Font");
    } 
    #endregion
    public static void ToggleFullScreen()
    {
        Graphics.GraphicsDeviceManager.IsFullScreen = !Graphics.GraphicsDeviceManager.IsFullScreen;
        Graphics.GraphicsDeviceManager.ApplyChanges();
    }
}