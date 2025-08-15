namespace VectorTD.Core;

/// <summary>
/// A color. Red, green, blue, and alpha. The way computers see the world.
/// Four numbers that make every shade. Simple and complete.
/// </summary>
public struct Color
{
    /// <summary>Red component. How much red is in this color. Zero to one.</summary>
    public float R { get; set; }

    /// <summary>Green component. How much green is in this color. Zero to one.</summary>
    public float G { get; set; }

    /// <summary>Blue component. How much blue is in this color. Zero to one.</summary>
    public float B { get; set; }

    /// <summary>Alpha component. How see-through this color is. One is solid. Zero is invisible.</summary>
    public float A { get; set; }

    /// <summary>
    /// Creates a color. Mix red, green, and blue. Add alpha for transparency.
    /// Four numbers make any color you can see. And some you can't.
    /// </summary>
    public Color(float r, float g, float b, float a = 1.0f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>Pure white. All colors at full strength. Bright and clean.</summary>
    public static Color White => new(1.0f, 1.0f, 1.0f, 1.0f);

    /// <summary>Pure black. No color at all. Dark and empty.</summary>
    public static Color Black => new(0.0f, 0.0f, 0.0f, 1.0f);

    /// <summary>Pure red. The color of blood and fire. Strong and dangerous.</summary>
    public static Color Red => new(1.0f, 0.0f, 0.0f, 1.0f);

    /// <summary>Pure green. The color of grass and life. Natural and growing.</summary>
    public static Color Green => new(0.0f, 1.0f, 0.0f, 1.0f);

    /// <summary>Pure blue. The color of sky and water. Deep and calm.</summary>
    public static Color Blue => new(0.0f, 0.0f, 1.0f, 1.0f);

    /// <summary>Yellow. Red and green mixed. Bright like the sun.</summary>
    public static Color Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);

    /// <summary>Cyan. Green and blue mixed. Cool like ice.</summary>
    public static Color Cyan => new(0.0f, 1.0f, 1.0f, 1.0f);

    /// <summary>Magenta. Red and blue mixed. Electric and unnatural.</summary>
    public static Color Magenta => new(1.0f, 0.0f, 1.0f, 1.0f);

    /// <summary>Orange. Red with some green. Warm like autumn.</summary>
    public static Color Orange => new(1.0f, 0.5f, 0.0f, 1.0f);

    /// <summary>Purple. Blue with some red. Royal and mysterious.</summary>
    public static Color Purple => new(0.5f, 0.0f, 1.0f, 1.0f);

    /// <summary>Gray. All colors equally mixed. Neutral and balanced.</summary>
    public static Color Gray => new(0.5f, 0.5f, 0.5f, 1.0f);

    /// <summary>Dark gray. Gray but darker. Closer to black than white.</summary>
    public static Color DarkGray => new(0.3f, 0.3f, 0.3f, 1.0f);

    /// <summary>Light gray. Gray but lighter. Closer to white than black.</summary>
    public static Color LightGray => new(0.7f, 0.7f, 0.7f, 1.0f);

    /// <summary>
    /// Converts to array. Four numbers in order. Red, green, blue, alpha.
    /// Sometimes other code needs it this way. Arrays are simple.
    /// </summary>
    public float[] ToArray() => new[] { R, G, B, A };
}
