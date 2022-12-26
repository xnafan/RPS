using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
namespace RPS;
public class RpsGame : Game
{

    //TODO: avoidance- and hunt behavior
    //      bar graph showing current distribution of R/P/S

    #region Properties
    private const double _speed = .1;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<GameObject> _gameObjects = new();
    private readonly Random rnd = new();
    public static Texture2D Paper, Rock, Scissors, Tile;
    public static Rectangle ScreenResolution = new Rectangle(0, 0, 1920, 1080);
    public static Rectangle GameBounds { get; set; } = new Rectangle(0, 0, (int)(ScreenResolution.Width *.8f), ScreenResolution.Height);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new();
    private SpriteFont _font;
    private static StringBuilder _builder = new StringBuilder();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(GameBounds, 5);
    private int _numberOfGameObjects = 1000;
    #endregion

    #region Constructor and initialization
    public RpsGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenResolution.Width;
        _graphics.PreferredBackBufferHeight = ScreenResolution.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }
    public void NewGame()
    {
        _gameObjects.Clear();
        for (int i = 0; i < _numberOfGameObjects; i++)
        {
            var location = new Vector2(rnd.Next(GameBounds.Width), rnd.Next(GameBounds.Height));

            _gameObjects.Add(new GameObject(TypeTextures[RpsType.Rock].Width) { Location = location, Speed = _speed });
        }
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Rock = Content.Load<Texture2D>("gfx/Rock_32px");
        Paper = Content.Load<Texture2D>("gfx/Paper_32px");
        Scissors = Content.Load<Texture2D>("gfx/Scissors_32px");
        Tile = Content.Load<Texture2D>("gfx/Tile_16x16");
        TypeTextures.Add(RpsType.Rock, Rock);
        TypeTextures.Add(RpsType.Paper, Paper);
        TypeTextures.Add(RpsType.Scissors, Scissors);
        _font = Content.Load<SpriteFont>("Font");
        NewGame();
    }

    #endregion

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
        if (Keyboard.GetState().IsKeyDown(Keys.F5)) NewGame();

        _partitioningHelper.Update(_gameObjects);
        foreach (var obj in _gameObjects)
        {
            obj.Update(gameTime);
            var collisions = _partitioningHelper.GetCollisions(obj);
            var collisionItem = collisions.FirstOrDefault(colObj => colObj.RpsType != obj.RpsType);
            if (collisionItem != null)
            {
                if (obj.RpsType == RpsType.Rock && collisionItem.RpsType == RpsType.Paper) { obj.RpsType = RpsType.Paper; _partitioningHelper.Remove(obj); }
                else if (obj.RpsType == RpsType.Paper && collisionItem.RpsType == RpsType.Scissors) { obj.RpsType = RpsType.Scissors; _partitioningHelper.Remove(obj); }
                else if (obj.RpsType == RpsType.Scissors && collisionItem.RpsType == RpsType.Rock) { obj.RpsType = RpsType.Rock; _partitioningHelper.Remove(obj); }
            }
        }
    }

    #region Draw and related
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _gameObjects.ForEach(obj => obj.Draw(_spriteBatch, gameTime));
        WriteStatus();
        DrawStatusPanel();
        _spriteBatch.End();
    }

    private void DrawStatusPanel()
    {
        var panelWidth = ScreenResolution.Width / 5;
        var panelHeight = ScreenResolution.Height;
        _spriteBatch.Draw(Tile, new Rectangle(GameBounds.Right, 0, panelWidth, panelHeight), Color.ForestGreen);
        float numberOfRocks = _gameObjects.Count(obj => obj.RpsType == RpsType.Rock);
        float numberOfPaper =  _gameObjects.Count(obj => obj.RpsType == RpsType.Paper);
        float numberOfScissors = _numberOfGameObjects - (numberOfRocks + numberOfPaper);
        var pctOfRock = numberOfRocks / _numberOfGameObjects;
        var pctOfPaper = numberOfPaper / _numberOfGameObjects;
        var pctOfScissors = numberOfScissors / _numberOfGameObjects;

        var bottomOfBars = ScreenResolution.Height * .9f;
        var maxHeightOfBars = ScreenResolution.Height * .7f;

        int heightOfRockInPixels = (int)(maxHeightOfBars * pctOfRock);
        int heightOfPaperInPixels = (int)(maxHeightOfBars * pctOfPaper);
        int heightOfScissorsInPixels = (int)(maxHeightOfBars * pctOfScissors);

        int widthOfBars = (int)(panelWidth / 7f);

        _spriteBatch.Draw(Tile, new Rectangle(GameBounds.Right + widthOfBars, (int)(bottomOfBars - heightOfRockInPixels), widthOfBars, (int)heightOfRockInPixels), Color.Yellow);
        _spriteBatch.Draw(Rock, new Rectangle(GameBounds.Right + widthOfBars, (int)(bottomOfBars - heightOfRockInPixels)-widthOfBars, widthOfBars, widthOfBars), Color.White);

        _spriteBatch.Draw(Tile, new Rectangle(GameBounds.Right + widthOfBars*3, (int)(bottomOfBars - heightOfPaperInPixels), widthOfBars, (int)heightOfPaperInPixels), Color.LightBlue);
        _spriteBatch.Draw(Paper, new Rectangle(GameBounds.Right + widthOfBars*3, (int)(bottomOfBars - heightOfPaperInPixels) - widthOfBars, widthOfBars, widthOfBars), Color.White);

        _spriteBatch.Draw(Tile, new Rectangle(GameBounds.Right + widthOfBars*5, (int)(bottomOfBars - heightOfScissorsInPixels), widthOfBars, (int)heightOfScissorsInPixels), Color.LightPink);
        _spriteBatch.Draw(Scissors, new Rectangle(GameBounds.Right + widthOfBars*5, (int)(bottomOfBars - heightOfScissorsInPixels) - widthOfBars, widthOfBars, widthOfBars), Color.White);





    }

    private void WriteStatus()
    {
        var count = _gameObjects.GroupBy(x => x.RpsType).Select(group => new
        {
            Metric = group.Key,
            Count = group.Count()
        }).ToList();

        _builder.Clear();
        count.ForEach(c => _builder.Append(c.Metric + ":" + c.Count + Environment.NewLine));
        DrawWithOutline(_builder.ToString());
    }
    private void DrawWithOutline(string text)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                _spriteBatch.DrawString(_font, text, Vector2.One * 10 + new Vector2(x, y), Color.Black);
            }
        }
        _spriteBatch.DrawString(_font, text, Vector2.One * 10, Color.White);
    } 
    #endregion
}