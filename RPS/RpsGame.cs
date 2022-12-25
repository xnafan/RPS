using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPS;
public class RpsGame : Game
{
    private const double _speed = .1;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<GameObject> _playerObjects = new();
    private readonly Random rnd = new();
    private Texture2D _texture;
    public static Texture2D Paper, Rock, Scissors;
    public static Rectangle GameBounds { get; set; } = new Rectangle(0,0,1920, 1080);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new ();
    private SpriteFont _font;
    private string _status = "";
    private static StringBuilder _builder = new StringBuilder ();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(GameBounds,4);

    public RpsGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = GameBounds.Width;
        _graphics.PreferredBackBufferHeight = GameBounds.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _texture = CreateTexture(GraphicsDevice, 32, 32, pixel => Color.White);
        for (int i = 0; i < 1200; i++)
        {
            AddRandomPlayerObject();
        }

        Rock = Content.Load<Texture2D>("gfx/Rock_32px");
        Paper = Content.Load<Texture2D>("gfx/Paper_32px");
        Scissors = Content.Load<Texture2D>("gfx/Scissors_32px");
        TypeTextures.Add(RpsType.Rock, Rock);
        TypeTextures.Add(RpsType.Paper, Paper);
        TypeTextures.Add(RpsType.Scissors, Scissors);
        _font = Content.Load<SpriteFont>("Font");
    } 

    private void AddRandomPlayerObject()
    {
        var location = new Vector2(rnd.Next(_graphics.PreferredBackBufferWidth), rnd.Next(_graphics.PreferredBackBufferHeight));

        _playerObjects.Add(new GameObject(_texture) { Location = location, Speed = _speed });
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))Exit();

        _partitioningHelper.Update(_playerObjects);

        foreach (var obj in _playerObjects)
        {
            obj.Update(gameTime);
            var collisions = GetCollisions(_playerObjects, obj);
            foreach (var collisionItem in collisions)
            {
                if(obj.GetType() == RpsType.Rock && collisionItem.GetType() == RpsType.Paper) { obj.SetType(RpsType.Paper); }
                else if (obj.GetType() == RpsType.Paper && collisionItem.GetType() == RpsType.Scissors) { obj.SetType(RpsType.Scissors); }
                else if (obj.GetType() == RpsType.Scissors&& collisionItem.GetType() == RpsType.Rock) { obj.SetType(RpsType.Rock); }
            }
        }

        var count = _playerObjects.GroupBy(x => x.GetType()).Select(group => new {
            Metric = group.Key,
            Count = group.Count()
        }).ToList();

        _builder.Clear();
        count.ForEach(c => _builder.Append(c.Metric + ":" + c.Count + Environment.NewLine));
        _status = _builder.ToString();
    }

    private IEnumerable<GameObject> GetCollisions(List<GameObject> playerObjects, GameObject player)
    {
        foreach (var obj in _partitioningHelper.GetCollisionCandidates(player))
        {
            if (player.GetBounds().Intersects(obj.GetBounds()) && player != obj)
            {
                yield return obj;
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _playerObjects.ForEach(obj => obj.Draw(_spriteBatch, gameTime));
        DrawWithOutline();

        _spriteBatch.End();

    }

    private void DrawWithOutline()
    {

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
        _spriteBatch.DrawString(_font, _status, Vector2.One * 10 + new Vector2(x,y), Color.Black);
            }
        }
        _spriteBatch.DrawString(_font, _status, Vector2.One * 10, Color.White);
    }

    public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
    {
        //initialize a texture
        Texture2D texture = new Texture2D(device, width, height);

        //the array holds the color for each pixel in the texture
        Color[] data = new Color[width * height];
        for (int pixel = 0; pixel < data.Count(); pixel++)
        {
            //the function applies the color according to the specified pixel
            data[pixel] = paint(pixel);
        }

        //set the color
        texture.SetData(data);

        return texture;
    }
}

public static class RectangleExtensions
{
    
}