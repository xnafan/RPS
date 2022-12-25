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
    private const double _speed = .1;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<GameObject> _playerObjects = new();
    private readonly Random rnd = new();
    public static Texture2D Paper, Rock, Scissors;
    public static Rectangle GameBounds { get; set; } = new Rectangle(0,0,1920, 1080);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new ();
    private SpriteFont _font;
    private string _status = "";
    private static StringBuilder _builder = new StringBuilder ();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(GameBounds,6);
    private RpsType[] _typeToConvertTo;
    private int _numberOfGameObjects = 1000;
    Stopwatch _stopWatch;
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

    public void NewGame()
    {
        _playerObjects.Clear();
        _typeToConvertTo = new RpsType[_numberOfGameObjects];
        for (int i = 0; i < _numberOfGameObjects; i++)
        {
            AddRandomPlayerObject();
        }
    }
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        //_texture = CreateTexture(GraphicsDevice, 32, 32, pixel => Color.White);
        
        Rock = Content.Load<Texture2D>("gfx/Rock_32px");
        Paper = Content.Load<Texture2D>("gfx/Paper_32px");
        Scissors = Content.Load<Texture2D>("gfx/Scissors_32px");
        TypeTextures.Add(RpsType.Rock, Rock);
        TypeTextures.Add(RpsType.Paper, Paper);
        TypeTextures.Add(RpsType.Scissors, Scissors);
        _font = Content.Load<SpriteFont>("Font");
        NewGame();
    } 

    private void AddRandomPlayerObject()
    {
        var location = new Vector2(rnd.Next(_graphics.PreferredBackBufferWidth), rnd.Next(_graphics.PreferredBackBufferHeight));

        _playerObjects.Add(new GameObject(TypeTextures[RpsType.Rock].Width) { Location = location, Speed = _speed });
    }

    protected override void Update(GameTime gameTime)
    {
        _stopWatch = Stopwatch.StartNew();
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))Exit();
        if (Keyboard.GetState().IsKeyDown(Keys.F5)) NewGame();
        if (Keyboard.GetState().IsKeyDown(Keys.Add)) { _numberOfGameObjects += 100; NewGame(); };
        if (Keyboard.GetState().IsKeyDown(Keys.Subtract)) { _numberOfGameObjects -= 100; NewGame(); };

        _partitioningHelper.Update(_playerObjects);

        int index = 0;
        foreach (var obj in _playerObjects)
        {
            obj.Update(gameTime);
            var collisions = _partitioningHelper.GetCollisions(obj);
            //Debug.WriteLine("collisions:" + collisions.Count());
            foreach (var collisionItem in collisions)
            //var collisionItem = collisions.FirstOrDefault();
            //if(collisionItem!= null)
            {
                if(obj.RpsType == RpsType.Rock && collisionItem.RpsType == RpsType.Paper) { _typeToConvertTo[index] = RpsType.Paper;}
                else if (obj.RpsType == RpsType.Paper && collisionItem.RpsType == RpsType.Scissors) { _typeToConvertTo[index] = RpsType.Scissors;  }
                else if (obj.RpsType == RpsType.Scissors&& collisionItem.RpsType == RpsType.Rock) { _typeToConvertTo[index] = RpsType.Rock;}
            }
            index++;

              //  _partitioningHelper.Remove(obj); 

        }
        for (int i = 0; i < _numberOfGameObjects; i++)
        {
            _playerObjects[i].RpsType = _typeToConvertTo[i];
        }
        var count = _playerObjects.GroupBy(x => x.RpsType).Select(group => new {
            Metric = group.Key,
            Count = group.Count()
        }).ToList();

        _builder.Clear();
        count.ForEach(c => _builder.Append(c.Metric + ":" + c.Count + Environment.NewLine));
        _status = _builder.ToString();
        _stopWatch.Stop();
        Debug.WriteLine("update: " + _stopWatch.ElapsedMilliseconds);
    }

    
    protected override void Draw(GameTime gameTime)
    {
        _stopWatch = new Stopwatch();
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _playerObjects.ForEach(obj => obj.Draw(_spriteBatch, gameTime));
        DrawWithOutline();
        _spriteBatch.End();
        _stopWatch.Stop();
        Debug.WriteLine("draw: " + _stopWatch.ElapsedMilliseconds);
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