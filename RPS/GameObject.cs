using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;

namespace RPS;
public class GameObject : IBounded
{

    private bool _wasJustConverted;
    private RpsType type;

    public RpsType GetType()
    {
        return type;
    }

    public void SetType(RpsType value)
    {
        if (value == type) return;
        type = value;
        _wasJustConverted= true;
    }

    public Vector2 Location { get; set; }
    public double Direction { get; set; }
    public double Speed { get; set; }

    private static readonly Random rnd = new Random();

    public Texture2D Texture { get; set; }

    public Rectangle GetBounds() { return new Rectangle((int)Location.X - Texture.Width / 2, (int)Location.Y - Texture.Height / 2, Texture.Width, Texture.Height); }

    public GameObject(Texture2D texture)
    {
        Texture = texture;
        SetType(Enum.GetValues<RpsType>()[rnd.Next(Enum.GetValues<RpsType>().Length)]);
        Direction = RandomDirectionInRadian();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var bounds = GetBounds();
        if (_wasJustConverted) { bounds.Size = new Point(bounds.Width + 8, bounds.Height+ 8); }
        bounds.Offset(-4, -4);
            _wasJustConverted = false;
        spriteBatch.Draw(RpsGame.TypeTextures[GetType()], bounds, Color.White);
    }

    public void Update(GameTime gameTime)
    {
        Vector2 newLocation = new Vector2(-1, -1);
        while (!RpsGame.GameBounds.Contains((int)newLocation.X, (int)newLocation.Y))
        {
            newLocation = Location + AngleInRadianToVector2(Direction) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * (float)Speed;
            TurnSlightly();
        }
        Location = newLocation;
    }

    private void TurnSlightly()
    {
        var randomDirectionChangeInRadians = (rnd.NextDouble() * Math.PI / 8) - Math.PI / 16;
        Direction += randomDirectionChangeInRadians;
    }
    private Vector2 AngleInRadianToVector2(double angleInRadian)
    {
        return new Vector2(MathF.Cos((float)angleInRadian), MathF.Sin((float)angleInRadian));
    }
    private double RandomDirectionInRadian()
    {
        return rnd.NextDouble() * Math.PI * 2;
    }

    private Color GetColor()
    {
        switch (GetType())
        {
            case RpsType.Paper: return Color.White;
            case RpsType.Scissors: return Color.Silver;
            case RpsType.Rock: return Color.Black;
        }
        throw new Exception($"{GetType()} is invalid type.");
    }
}