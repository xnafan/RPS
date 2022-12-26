using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RPS.Model;
using RPS.Tools;
using System;
using System.Collections.Generic;
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
    private List<GameObject> _gameObjects = new();
    private readonly Random rnd = new();
    public static Texture2D Paper, Rock, Scissors, Tile;
    public static Rectangle ScreenResolution = new Rectangle(0, 0, 1920, 1080);
    public static Rectangle GameBounds { get; set; } = new Rectangle(0, 0, (int)(ScreenResolution.Width *.8f), ScreenResolution.Height);
    public static Dictionary<RpsType, Texture2D> TypeTextures { get; set; } = new();
    private SpriteFont _font;
    private static StringBuilder _builder = new StringBuilder();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(GameBounds, 5);
    private int _numberOfGameObjects = 500;
    private Color _paperColor = new Color(0, 224, 213), _rockColor = new Color(255, 232, 74), _scissorsColor = new Color(241, 186, 244);
    private KeyboardState _previousState;
    private bool _intelligentBehavior = true;
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
            var location = new Vector2((int)(GameBounds.Width * .1f) + rnd.Next((int)(GameBounds.Width*.8f)), (int)(GameBounds.Height * .1f) + rnd.Next((int)(GameBounds.Height * .8f)));

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

    #region Update and related
    protected override void Update(GameTime gameTime)
    {
        CheckKeyboardInputAndReact();

        _partitioningHelper.Update(_gameObjects);
        foreach (var obj in _gameObjects)
        {
            var collisions = HandleCollisions(obj);
            if (_intelligentBehavior)
            {
                ChangeDirectionForHuntAndAvoidance(obj, collisions); 
            }
            obj.Update(gameTime);
        }
    }

    private void ChangeDirectionForHuntAndAvoidance(GameObject obj, IEnumerable<GameObject> collisions)
    {
        var collisionCandidates = _partitioningHelper.GetCollisionCandidates(obj);
        var nonCollisionObjectsOfOtherTypes = collisionCandidates.Except(collisions).Where(otherObj => otherObj.RpsType != obj.RpsType);
        if(!nonCollisionObjectsOfOtherTypes.Any(otherObj => otherObj.RpsType != obj.RpsType)) {return;}
        var closestNonCollisionNeighbor = nonCollisionObjectsOfOtherTypes.OrderBy(otherObj => Math.Abs(otherObj.Location.X - obj.Location.X) + Math.Abs(otherObj.Location.Y - obj.Location.Y)).First();
        if (obj.RpsType.Beats(closestNonCollisionNeighbor.RpsType))
        {
            obj.Direction = DirectionTo(obj.Location, closestNonCollisionNeighbor.Location);
        }
        else
        {
            obj.Direction = DirectionTo(closestNonCollisionNeighbor.Location, obj.Location);
        }
    }

    private double DirectionTo(Vector2 from, Vector2 to)
    {
        var angleVector = to - from;
        return Math.Atan2(angleVector.Y, angleVector.X);
    }

    private IEnumerable<GameObject> HandleCollisions(GameObject obj)
    {
        var collisions = _partitioningHelper.GetCollisions(obj);
        var collisionItem = collisions.FirstOrDefault(colObj => colObj.RpsType != obj.RpsType);
        if (collisionItem != null)
        {
            if (obj.RpsType == RpsType.Rock && collisionItem.RpsType == RpsType.Paper) { obj.RpsType = RpsType.Paper; _partitioningHelper.Remove(obj); }
            else if (obj.RpsType == RpsType.Paper && collisionItem.RpsType == RpsType.Scissors) { obj.RpsType = RpsType.Scissors; _partitioningHelper.Remove(obj); }
            else if (obj.RpsType == RpsType.Scissors && collisionItem.RpsType == RpsType.Rock) { obj.RpsType = RpsType.Rock; _partitioningHelper.Remove(obj); }
        }
        return collisions;
    }

    private void CheckKeyboardInputAndReact()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
        if (keyboardState.IsKeyDown(Keys.F5)) NewGame();
        if (keyboardState.IsKeyDown(Keys.F11) && _previousState.IsKeyUp(Keys.F11))
        {
            ToggleFullScreen();
        }
        if (keyboardState.IsKeyDown(Keys.Add) && _previousState.IsKeyUp(Keys.Add))
        {
            _numberOfGameObjects += 100;
            NewGame();
        }
        if (keyboardState.IsKeyDown(Keys.OemMinus) && _previousState.IsKeyUp(Keys.OemMinus))
        {
            _numberOfGameObjects -= 100;
            NewGame();
        }
        if (keyboardState.IsKeyDown(Keys.I) && _previousState.IsKeyUp(Keys.I))
        {
            _intelligentBehavior = !_intelligentBehavior;
        }
        _previousState = keyboardState;
    }

    private void ToggleFullScreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
    } 
    #endregion

    #region Draw and related
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        _gameObjects.ForEach(obj => obj.Draw(_spriteBatch, gameTime));
        DrawStatusPanel();
        _spriteBatch.End();
    }

    private void DrawStatusPanel()
    {
        var panelWidth = ScreenResolution.Width / 5;
        var panelHeight = ScreenResolution.Height;
        _spriteBatch.Draw(Tile, new Rectangle(GameBounds.Right, 0, panelWidth, panelHeight), Color.ForestGreen);

        float numberOfRocks = _gameObjects.Count(obj => obj.RpsType == RpsType.Rock);
        float numberOfPaper = _gameObjects.Count(obj => obj.RpsType == RpsType.Paper);
        float numberOfScissors = _numberOfGameObjects - (numberOfRocks + numberOfPaper);
        
        int widthOfBars = (int)(panelWidth / 7f);

        int leftOfRockBar = GameBounds.Right + widthOfBars;
        int leftOfPaperBar = GameBounds.Right + widthOfBars * 3;
        int leftOfScissorsBar = GameBounds.Right + widthOfBars * 5;

        DrawBar((int)numberOfRocks, Rock, _rockColor, leftOfRockBar, widthOfBars);

        DrawBar((int)numberOfPaper, Paper, _paperColor, leftOfPaperBar, widthOfBars);

        DrawBar((int)numberOfScissors, Scissors, _scissorsColor, leftOfScissorsBar, widthOfBars);

    }

    private void DrawBar(int amount, Texture2D texture, Color color, int leftOfBar, int widthOfBars)
    {
        var bottomOfBars = ScreenResolution.Height * .95f;
        var maxHeightOfBars = ScreenResolution.Height * .85f;
        var percentage = amount / (float)_numberOfGameObjects;

        int heightOfBar = (int)(maxHeightOfBars * percentage);
        int topOfBar = (int)(bottomOfBars - heightOfBar) - widthOfBars;

        var border = 2;
        _spriteBatch.Draw(Tile, new Rectangle(leftOfBar-border, (int)(bottomOfBars - heightOfBar  - widthOfBars) - border, widthOfBars+border *2, (int)heightOfBar  + widthOfBars + border*2), Color.Black);
        _spriteBatch.Draw(Tile, new Rectangle(leftOfBar, (int)(bottomOfBars - heightOfBar), widthOfBars, (int)heightOfBar), color);
        _spriteBatch.Draw(texture, new Rectangle(leftOfBar, topOfBar, widthOfBars, widthOfBars), Color.White);

        WriteCentered(amount.ToString(), new Vector2(leftOfBar + widthOfBars / 2, topOfBar - widthOfBars / 2), Color.White, true);

        //WriteCentered(((int)(percentage*100)) + "%", new Vector2(leftOfBar + widthOfBars / 2, bottomOfBars + widthOfBars), Color.White, true);
    }
    private void WriteCentered(string text, Vector2 location, Color color, bool shadow = false)
    {
        var textSize = _font.MeasureString(text);
        var actualLocation = location - textSize / 2;

        if (shadow)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    _spriteBatch.DrawString(_font, text, actualLocation + new Vector2(x, y), Color.Black);
                }
            }
        }

        _spriteBatch.DrawString(_font, text, actualLocation, color);
    }
    #endregion
}