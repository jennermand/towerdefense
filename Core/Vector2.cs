namespace VectorTD.Core;

/// <summary>
/// A point in space. Two numbers. X and Y. Simple as coordinates on a map.
/// Everything has a place. This tells you where.
/// </summary>
public struct Vector2
{
    /// <summary>
    /// How far right. Positive goes right. Negative goes left.
    /// Like reading a book. Left to right. Simple.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// How far down. Positive goes down. Negative goes up.
    /// Like gravity. Down is natural. Up takes work.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Creates a point. Give it X and Y. It knows where it is.
    /// Two numbers make a place. Mathematics of location.
    /// </summary>
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// The origin. Where everything starts. Zero and zero. Nothing and nowhere.
    /// But also the center. The beginning. The point of reference.
    /// </summary>
    public static Vector2 Zero => new(0, 0);

    /// <summary>
    /// One and one. Unity in both directions. Equal and balanced.
    /// Sometimes you need a standard. This is it.
    /// </summary>
    public static Vector2 One => new(1, 1);

    /// <summary>
    /// How far from zero. The distance from nothing to here.
    /// Pythagoras knew this. Ancient wisdom in modern code.
    /// </summary>
    public float Length => MathF.Sqrt(X * X + Y * Y);

    /// <summary>
    /// Length squared. Faster to calculate. No square root needed.
    /// Sometimes close enough is good enough. Speed matters.
    /// </summary>
    public float LengthSquared => X * X + Y * Y;

    /// <summary>
    /// This vector made unit length. Same direction, different size.
    /// Points the same way but travels exactly one unit.
    /// Direction without distance. Pure intent.
    /// </summary>
    public Vector2 Normalized
    {
        get
        {
            var length = Length;
            return length > 0 ? new Vector2(X / length, Y / length) : Zero;
        }
    }

    /// <summary>Addition. Two points become one. Simple arithmetic.</summary>
    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>Subtraction. The difference between here and there.</summary>
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>Multiplication. Makes things bigger or smaller. Scale matters.</summary>
    public static Vector2 operator *(Vector2 a, float scalar) => new(a.X * scalar, a.Y * scalar);

    /// <summary>Division. The opposite of multiplication. Makes things smaller usually.</summary>
    public static Vector2 operator /(Vector2 a, float scalar) => new(a.X / scalar, a.Y / scalar);

    /// <summary>
    /// Distance between two points. How far apart they are.
    /// The space between here and there. Measured in units.
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b) => (a - b).Length;

    /// <summary>
    /// Distance squared. Faster to calculate. Good for comparisons.
    /// When you only need to know which is closer. Not how much closer.
    /// </summary>
    public static float DistanceSquared(Vector2 a, Vector2 b) => (a - b).LengthSquared;

    /// <summary>
    /// Linear interpolation. Smooth movement from one point to another.
    /// T is time. Zero is start. One is end. Between is the journey.
    /// </summary>
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return a + (b - a) * t;
    }

    /// <summary>
    /// Converts to text. Shows the numbers. Makes debugging easier.
    /// Sometimes you need to see where things are. This shows you.
    /// </summary>
    public override string ToString() => $"({X:F2}, {Y:F2})";
}
