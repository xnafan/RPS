using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace RPS.Model;
public class GameObject : IBounded
{
    #region Properties
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
    #endregion

    public GameObject(int size)
    {
        Size = size;
        RpsType = Enum.GetValues<RpsType>()[rnd.Next(Enum.GetValues<RpsType>().Length)];
        Direction = GetRandomDirectionInRadian();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var pulseBounds = GetBounds();
        if (_wasJustConverted) { pulseBounds.Size = new Point(pulseBounds.Width + pulseBounds.Width / 4, pulseBounds.Height + pulseBounds.Width / 4); }
        pulseBounds.Offset(-pulseBounds.Width / 4, -pulseBounds.Width / 4);
        _wasJustConverted = false;
        spriteBatch.Draw(Graphics.TypeTextures[RpsType], pulseBounds, Color.White);
    }

    public void Update(GameTime gameTime)
    {
        Rectangle newBounds;
        Vector2 newLocation;
        do
        {
            newBounds = GetBounds();
            TurnSlightly();
            newLocation = Location + AngleInRadianToVector2(Direction) * (float)gameTime.ElapsedGameTime.TotalMilliseconds * (float)Speed;
            newBounds.Offset(newLocation - Location);
        }
        while (!Graphics.GameBounds.Contains(newBounds));
        Location = newLocation;
    }

    private void TurnSlightly()
    {
        var randomDirectionChangeInRadians = rnd.NextDouble() * Math.PI / 8 - Math.PI / 16;
        Direction += randomDirectionChangeInRadians;
    }
    private Vector2 AngleInRadianToVector2(double angleInRadian)
    {
        return new Vector2(MathF.Cos((float)angleInRadian), MathF.Sin((float)angleInRadian));
    }
    private double GetRandomDirectionInRadian()
    {
        return rnd.NextDouble() * Math.PI * 2;
    }

    public override string ToString()
    {
        return $"{RpsType} at ({Location.X.ToString("0.0")},{Location.Y.ToString("0.0")})";
    }
}