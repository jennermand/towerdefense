using VectorTD.Core;

namespace VectorTD.Game;

/// <summary>
/// A death marker. Shows where something died. Fades away like everything else.
/// Yellow circle. Simple. Clean. Gone in two seconds.
/// </summary>
public class DeathMarker
{
    public Vector2 Position { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; set; }

    public bool IsAlive => Life > 0;
    public float Alpha => Life / MaxLife;

    /// <summary>
    /// Updates the marker. Time passes. Life gets shorter.
    /// Eventually it reaches zero. Then it's gone.
    /// </summary>
    public void Update(float deltaTime)
    {
        Life -= deltaTime;
    }
}
