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
    #region Properties
    private const double _speed = .1;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<GameObject> _playerObjects = new();
    private readonly Random rnd = new();
    public static Texture2D Paper, Rock, Scissors;
    public static Rectangle GameBounds { get; set; } = new Rectangle(0, 0, 1920, 1080);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new();
    private SpriteFont _font;
    private static StringBuilder _builder = new StringBuilder();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(GameBounds, 5);
    private int _numberOfGameObjects = 400;
    #endregion

    #region Constructor and initialization
    public RpsGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = GameBounds.Width;
        _graphics.PreferredBackBufferHeight = GameBounds.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }
    public void NewGame()
    {
        _playerObjects.Clear();
        for (int i = 0; i < _numberOfGameObjects; i++)
        {
            var location = new Vector2(rnd.Next(_graphics.PreferredBackBufferWidth), rnd.Next(_graphics.PreferredBackBufferHeight));

            _playerObjects.Add(new GameObject(TypeTextures[RpsType.Rock].Width) { Location = location, Speed = _speed });
        }
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Rock = Content.Load<Texture2D>("gfx/Rock_32px");
        Paper = Content.Load<Texture2D>("gfx/Paper_32px");
        Scissors = Content.Load<Texture2D>("gfx/Scissors_32px");
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

        _partitioningHelper.Update(_playerObjects);
        foreach (var obj in _playerObjects)
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
        _playerObjects.ForEach(obj => obj.Draw(_spriteBatch, gameTime));
        WriteStatus();
        _spriteBatch.End();
    }
    private void WriteStatus()
    {
        var count = _playerObjects.GroupBy(x => x.RpsType).Select(group => new
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