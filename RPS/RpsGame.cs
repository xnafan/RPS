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
    private readonly Random _random = new();
    private int _numberOfGameObjects = 150;
    private const double _speed = .1;
    private bool _intelligentBehavior = true;
    private List<GameObject> _gameObjects = new();
    private GamePartitioningHelper<GameObject> _partitioningHelper = new GamePartitioningHelper<GameObject>(Graphics.GameBounds, 5);
    private KeyboardState _previousState;
    #endregion

    #region Constructor and initialization
    public RpsGame()
    {
        Graphics.GraphicsDeviceManager = new GraphicsDeviceManager(this);
        Graphics.GraphicsDeviceManager.PreferredBackBufferWidth = Graphics.ScreenResolution.Width;
        Graphics.GraphicsDeviceManager.PreferredBackBufferHeight = Graphics.ScreenResolution.Height;
        Graphics.GraphicsDeviceManager.IsFullScreen = true;
        Graphics.GraphicsDeviceManager.ApplyChanges();
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }
    public void NewGame()
    {
        _gameObjects.Clear();
        AddGameObjects(_numberOfGameObjects);
    }

    private void AddGameObjects(int quantityToAdd)
    {
        if (quantityToAdd < 0) {
            for (int i = 0; i < Math.Abs(quantityToAdd); i++)
            {
                if (_gameObjects.Any())
                {
                    _gameObjects.RemoveAt(0); 
                }
            }
            return;
        }

        for (int i = 0; i < quantityToAdd; i++)
        {
            var location = new Vector2((int)(Graphics.GameBounds.Width * .1f) + _random.Next((int)(Graphics.GameBounds.Width * .8f)), (int)(Graphics.GameBounds.Height * .1f) + _random.Next((int)(Graphics.GameBounds.Height * .8f)));

            _gameObjects.Add(new GameObject(Graphics.TypeTextures[RpsType.Rock].Width) { Location = location, Speed = _speed });
        }
    }

    protected override void LoadContent()
    {
        Graphics.SpriteBatch = new SpriteBatch(GraphicsDevice);
        Graphics.LoadContent(Content);
        NewGame();
    }

    #endregion

    #region Update and related
    protected override void Update(GameTime gameTime)
    {
        ReactToKeyboardInput();

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
        if (!nonCollisionObjectsOfOtherTypes.Any(otherObj => otherObj.RpsType != obj.RpsType)) { return; }
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

    private void ReactToKeyboardInput()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape)) Exit();
        if (keyboardState.IsKeyDown(Keys.F5)) NewGame();
        if (keyboardState.IsKeyDown(Keys.F11) && _previousState.IsKeyUp(Keys.F11))
        {
            Graphics.ToggleFullScreen();
        }
        if (keyboardState.IsKeyDown(Keys.Add) && _previousState.IsKeyUp(Keys.Add))
        {
            AddGameObjects(100);
        }
        if (keyboardState.IsKeyDown(Keys.OemMinus) && _previousState.IsKeyUp(Keys.OemMinus))
        {
            AddGameObjects(-100);
        }
        if (keyboardState.IsKeyDown(Keys.I) && _previousState.IsKeyUp(Keys.I))
        {
            _intelligentBehavior = !_intelligentBehavior;
        }
        _previousState = keyboardState;
    }

    
    #endregion

    #region Draw and related
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        Graphics.SpriteBatch.Begin();
        _gameObjects.ForEach(obj => obj.Draw(Graphics.SpriteBatch, gameTime));
        DrawStatusPanel();
        Graphics.SpriteBatch.End();
    }

    private void DrawStatusPanel()
    {
        var panelWidth = Graphics.ScreenResolution.Width / 5;
        var panelHeight = Graphics.ScreenResolution.Height;
        Graphics.SpriteBatch.Draw(Graphics.Tile, new Rectangle(Graphics.GameBounds.Right, 0, panelWidth, panelHeight), Color.ForestGreen);

        float numberOfRocks = _gameObjects.Count(obj => obj.RpsType == RpsType.Rock);
        float numberOfPaper = _gameObjects.Count(obj => obj.RpsType == RpsType.Paper);
        float numberOfScissors = _gameObjects.Count - (numberOfRocks + numberOfPaper);

        int widthOfBars = (int)(panelWidth / 7f);

        int leftOfRockBar = Graphics.GameBounds.Right + widthOfBars;
        int leftOfPaperBar = Graphics.GameBounds.Right + widthOfBars * 3;
        int leftOfScissorsBar = Graphics.GameBounds.Right + widthOfBars * 5;

        DrawBar((int)numberOfRocks, Graphics.Rock, Graphics.RockColor, leftOfRockBar, widthOfBars);

        DrawBar((int)numberOfPaper, Graphics.Paper, Graphics.PaperColor, leftOfPaperBar, widthOfBars);

        DrawBar((int)numberOfScissors, Graphics.Scissors, Graphics.ScissorsColor, leftOfScissorsBar, widthOfBars);

    }

    private void DrawBar(int amount, Texture2D texture, Color color, int leftOfBar, int widthOfBars)
    {
        var bottomOfBars = Graphics.ScreenResolution.Height * .95f;
        var maxHeightOfBars = Graphics.ScreenResolution.Height * .85f;
        var percentage = amount / (float)_gameObjects.Count;

        int heightOfBar = (int)(maxHeightOfBars * percentage);
        int topOfBar = (int)(bottomOfBars - heightOfBar) - widthOfBars;

        var border = 2;
        Graphics.SpriteBatch.Draw(Graphics.Tile, new Rectangle(leftOfBar - border, (int)(bottomOfBars - heightOfBar - widthOfBars) - border, widthOfBars + border * 2, heightOfBar + widthOfBars + border * 2), Color.Black);
        Graphics.SpriteBatch.Draw(Graphics.Tile, new Rectangle(leftOfBar, (int)(bottomOfBars - heightOfBar), widthOfBars, heightOfBar), color);
        Graphics.SpriteBatch.Draw(texture, new Rectangle(leftOfBar, topOfBar, widthOfBars, widthOfBars), Color.White);

        WriteCentered(amount.ToString(), new Vector2(leftOfBar + widthOfBars / 2, topOfBar - widthOfBars / 2), Color.White, true);
    }
    private void WriteCentered(string text, Vector2 location, Color color, bool shadow = false)
    {
        var textSize = Graphics.Font.MeasureString(text);
        var actualLocation = location - textSize / 2;

        if (shadow)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Graphics.SpriteBatch.DrawString(Graphics.Font, text, actualLocation + new Vector2(x, y), Color.Black);
                }
            }
        }

        Graphics.SpriteBatch.DrawString(Graphics.Font, text, actualLocation, color);
    }
    #endregion
}