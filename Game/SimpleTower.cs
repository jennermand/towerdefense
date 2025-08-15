using VectorTD.Core;
using System.Linq;

namespace VectorTD.Game;

/// <summary>
/// Tower colors. Red shoots red best. Green shoots green best. Blue shoots blue best.
/// Simple as that. Like choosing the right tool for the job.
/// </summary>
public enum TowerColor
{
    Red,    // Kills red enemies quick. Others take longer.
    Green,  // Kills green enemies quick. Others take longer.
    Blue    // Kills blue enemies quick. Others take longer.
}

/// <summary>
/// How towers pick their targets. Closest is safe. Strongest is smart.
/// Both have their place. Like different ways to fight.
/// </summary>
public enum TargetingMode
{
    Closest,    // Shoots whatever's nearest. Fast and simple.
    Strongest   // Shoots whatever's toughest. Slow but wise.
}

/// <summary>
/// A tower. Sits in one place. Shoots at enemies. Gets stronger with money.
/// Has a color. Has a range. Has a temper when enemies get too close.
/// </summary>
public class SimpleTower
{
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public Vector2 Position { get; private set; }
    public TowerColor Color { get; private set; }
    public TargetingMode TargetingMode { get; private set; } = TargetingMode.Closest;

    public float Damage { get; private set; }
    public float Range { get; private set; }
    public float FireRate { get; private set; }

    // Base stats for calculations
    private float BaseDamage { get; set; }
    private float BaseRange { get; set; } = 80f;
    private float BaseFireRate { get; set; } = 3f;
    
    public int DamageLevel { get; private set; } = 1;
    public int RangeLevel { get; private set; } = 1;
    public int FireRateLevel { get; private set; } = 1;

    // Base costs for different tower colors
    private int BaseCost => Color switch
    {
        TowerColor.Red => 10,
        TowerColor.Green => 15,
        TowerColor.Blue => 25,
        _ => 10
    };

    // Upgrade costs based on specified formula
    public int RangeUpgradeCost => BaseCost + (RangeLevel * 5);
    public int FireRateUpgradeCost => BaseCost + (FireRateLevel * 5);
    public int DamageUpgradeCost => (BaseCost + (DamageLevel * 5)) * 2; // Twice as expensive

    // Laser properties
    public bool IsLaserVisible => _laserTimer > 0f && _currentTarget != null && _currentTarget.Health > 0 && !_currentTarget.HasReachedEnd;
    public Vector2? LaserTarget => (_currentTarget != null && _currentTarget.Health > 0 && !_currentTarget.HasReachedEnd) ? _currentTarget.Position : null;

    private float _fireTimer = 0f;
    private SimpleEnemy? _currentTarget;
    private float _laserTimer = 0f;
    private const float LaserDuration = 0.3f; // Laser visible for 300ms

    public SimpleTower(int gridX, int gridY, TowerColor color = TowerColor.Red)
    {
        GridX = gridX;
        GridY = gridY;
        Color = color;

        // Set base stats based on tower color
        BaseDamage = Color switch
        {
            TowerColor.Red => 25f,
            TowerColor.Green => 30f,   // Slightly higher base damage
            TowerColor.Blue => 35f,    // Highest base damage
            _ => 25f
        };

        // Initialize current stats from base stats
        UpdateStats();

        // Convert grid position to world position (assuming 40px grid cells)
        Position = new Vector2(gridX * 40 + 20, gridY * 40 + 20);
    }

    /// <summary>
    /// Updates the tower. Time passes. Enemies move. Tower decides who dies next.
    /// Shoots when ready. Misses when target moves. War is like that.
    /// </summary>
    public void Update(float deltaTime, List<SimpleEnemy> enemies)
    {
        _fireTimer += deltaTime;
        _laserTimer -= deltaTime; // Countdown laser visibility

        // Find target based on targeting mode
        _currentTarget = FindTargetEnemy(enemies);

        // Fire at target (with safety check)
        if (_currentTarget != null && _currentTarget.Health > 0 && !_currentTarget.HasReachedEnd && _fireTimer >= 1f / FireRate)
        {
            Fire(_currentTarget);
            _fireTimer = 0f;
            _laserTimer = LaserDuration; // Show laser for brief moment
        }
    }

    private SimpleEnemy? FindTargetEnemy(List<SimpleEnemy> enemies)
    {
        var validEnemies = enemies.Where(e => e.Health > 0 && !e.HasReachedEnd && Vector2.Distance(Position, e.Position) <= Range).ToList();

        if (!validEnemies.Any()) return null;

        return TargetingMode switch
        {
            TargetingMode.Closest => FindClosestEnemy(validEnemies),
            TargetingMode.Strongest => FindStrongestEnemy(validEnemies),
            _ => FindClosestEnemy(validEnemies)
        };
    }

    private SimpleEnemy FindClosestEnemy(List<SimpleEnemy> validEnemies)
    {
        SimpleEnemy closest = validEnemies[0];
        float closestDistance = Vector2.Distance(Position, closest.Position);

        foreach (var enemy in validEnemies.Skip(1))
        {
            float distance = Vector2.Distance(Position, enemy.Position);
            if (distance < closestDistance)
            {
                closest = enemy;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private SimpleEnemy FindStrongestEnemy(List<SimpleEnemy> validEnemies)
    {
        SimpleEnemy strongest = validEnemies[0];
        float highestHealth = strongest.Health;

        foreach (var enemy in validEnemies.Skip(1))
        {
            if (enemy.Health > highestHealth)
            {
                strongest = enemy;
                highestHealth = enemy.Health;
            }
        }

        return strongest;
    }

    private void Fire(SimpleEnemy target)
    {
        // Calculate damage effectiveness based on color matching
        float effectiveDamage = CalculateEffectiveDamage(target);
        target.TakeDamage(effectiveDamage);
    }

    private float CalculateEffectiveDamage(SimpleEnemy target)
    {
        // Damage effectiveness matrix
        float effectiveness = (Color, target.Color) switch
        {
            // Same color = 100% effectiveness (optimal)
            (TowerColor.Red, EnemyColor.Red) => 1.0f,
            (TowerColor.Green, EnemyColor.Green) => 1.0f,
            (TowerColor.Blue, EnemyColor.Blue) => 1.0f,

            // Red tower vs other colors
            (TowerColor.Red, EnemyColor.Green) => 0.6f,  // 60% effectiveness
            (TowerColor.Red, EnemyColor.Blue) => 0.4f,   // 40% effectiveness

            // Green tower vs other colors
            (TowerColor.Green, EnemyColor.Red) => 0.7f,  // 70% effectiveness
            (TowerColor.Green, EnemyColor.Blue) => 0.6f, // 60% effectiveness

            // Blue tower vs other colors
            (TowerColor.Blue, EnemyColor.Red) => 0.5f,   // 50% effectiveness
            (TowerColor.Blue, EnemyColor.Green) => 0.7f, // 70% effectiveness

            _ => 1.0f // Default
        };

        return Damage * effectiveness;
    }

    /// <summary>
    /// Updates the tower's stats based on upgrade levels. More levels, better stats.
    /// Range and fire rate get ten percent better each level. Damage gets five percent.
    /// Mathematics of improvement. Small gains that add up.
    /// </summary>
    private void UpdateStats()
    {
        // Calculate current stats based on upgrade levels
        Range = BaseRange * (1f + (RangeLevel - 1) * 0.1f);           // +10% per level
        FireRate = BaseFireRate * (1f + (FireRateLevel - 1) * 0.1f);  // +10% per level
        Damage = BaseDamage * (1f + (DamageLevel - 1) * 0.05f);       // +5% per level
    }

    /// <summary>
    /// Upgrades the tower's range. Makes it reach farther.
    /// Longer arms. Wider vision. More enemies in reach.
    /// </summary>
    public bool TryUpgradeRange()
    {
        RangeLevel++;
        UpdateStats();
        return true;
    }

    /// <summary>
    /// Upgrades the tower's fire rate. Makes it shoot faster.
    /// Quicker trigger. Less waiting. More bullets flying.
    /// </summary>
    public bool TryUpgradeFireRate()
    {
        FireRateLevel++;
        UpdateStats();
        return true;
    }

    /// <summary>
    /// Upgrades the tower's damage. Makes it hit harder.
    /// Bigger bullets. Sharper teeth. More pain per shot.
    /// </summary>
    public bool TryUpgradeDamage()
    {
        DamageLevel++;
        UpdateStats();
        return true;
    }

    /// <summary>
    /// Gets the tower's statistics. Range, fire rate, damage, targeting mode.
    /// Numbers that tell the story. How strong it is. How it fights.
    /// </summary>
    public string GetStats()
    {
        return $"Range: {Range:F0} (Lv.{RangeLevel})\n" +
               $"Fire Rate: {FireRate:F1}/s (Lv.{FireRateLevel})\n" +
               $"Damage: {Damage:F1} (Lv.{DamageLevel})\n" +
               $"Targeting: {TargetingMode}";
    }

    /// <summary>
    /// Sets how the tower picks targets. Closest or strongest.
    /// Different strategies for different situations. Choice matters.
    /// </summary>
    public void SetTargetingMode(TargetingMode mode)
    {
        TargetingMode = mode;
    }

    /// <summary>
    /// Checks if a position is within range. Close enough to shoot.
    /// Distance matters. Too far and bullets miss. Too close and it's personal.
    /// </summary>
    public bool IsInRange(Vector2 position)
    {
        return Vector2.Distance(Position, position) <= Range;
    }
}
