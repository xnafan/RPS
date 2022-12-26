using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RPS;
public class GameObject : IBounded
{

    public int Size { get; set; }
    private bool _wasJustConverted;
    private RpsType type;

    public RpsType RpsType
    {
        get { return type; }
        set
        {
            if (value == type) return;
            type = value;
            _wasJustConverted = true;
        }
    }

    public Vector2 Location { get; set; }
    public double Direction { get; set; }
    public double Speed { get; set; }

    private static readonly Random rnd = new Random();

    public Rectangle GetBounds() { return new Rectangle((int)Location.X - Size / 2, (int)Location.Y - Size / 2, Size, Size); }

    public GameObject(int size)
    {
        Size = size;
        RpsType = Enum.GetValues<RpsType>()[rnd.Next(Enum.GetValues<RpsType>().Length)];
        Direction = RandomDirectionInRadian();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var pulseBounds = GetBounds();
        if (_wasJustConverted) { pulseBounds.Size = new Point(pulseBounds.Width + pulseBounds.Width / 4, pulseBounds.Height + pulseBounds.Width / 4); }
        pulseBounds.Offset(-pulseBounds.Width / 4, -pulseBounds.Width / 4);
        _wasJustConverted = false;
        spriteBatch.Draw(RpsGame.TypeTextures[RpsType], pulseBounds, Color.White);
    }

    public void Update(GameTime gameTime)
    {
        Vector2 newLocation;
        Rectangle newBounds = GetBounds();
        do
        {
            TurnSlightly();
            newLocation = Location + AngleInRadianToVector2(Direction) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * (float)Speed;
            newBounds.Offset(newLocation - Location);
        }
        while (!RpsGame.GameBounds.Contains(newBounds));
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
        switch (RpsType)
        {
            case RpsType.Paper: return Color.White;
            case RpsType.Scissors: return Color.Silver;
            case RpsType.Rock: return Color.Black;
        }
        throw new Exception($"{RpsType} is invalid type.");
    }
}