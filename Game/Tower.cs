using VectorTD.Core;
using VectorTD.Graphics;

namespace VectorTD.Game;

public enum TowerType
{
    Basic
}

public enum UpgradeType
{
    Range,
    Damage,
    FireRate
}

public class Tower
{
    public Vector2 Position { get; set; }
    public TowerType Type { get; private set; }
    public float Range { get; private set; }
    public float Damage { get; private set; }
    public float FireRate { get; private set; } // Shots per second
    public int Cost { get; private set; }
    
    // Upgrade costs
    public int RangeUpgradeCost { get; private set; }
    public int DamageUpgradeCost { get; private set; }
    public int FireRateUpgradeCost { get; private set; }
    
    // Upgrade levels
    public int RangeLevel { get; private set; }
    public int DamageLevel { get; private set; }
    public int FireRateLevel { get; private set; }

    private float _fireTimer;
    private Enemy? _currentTarget;
    private List<Projectile> _projectiles;

    public Tower(TowerType type, Vector2 position)
    {
        Type = type;
        Position = position;
        _fireTimer = 0.0f;
        _projectiles = new List<Projectile>();
        
        // Initialize upgrade levels
        RangeLevel = 1;
        DamageLevel = 1;
        FireRateLevel = 1;

        // Set base properties based on tower type
        switch (type)
        {
            case TowerType.Basic:
                Range = 80.0f;
                Damage = 25.0f;
                FireRate = 1.0f; // 1 shot per second
                Cost = 10;
                RangeUpgradeCost = 5;
                DamageUpgradeCost = 8;
                FireRateUpgradeCost = 6;
                break;
        }
    }

    public void Update(float deltaTime, List<Enemy> enemies)
    {
        _fireTimer += deltaTime;

        // Update projectiles
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            _projectiles[i].Update(deltaTime);
            if (!_projectiles[i].IsActive)
            {
                _projectiles.RemoveAt(i);
            }
        }

        // Find target
        _currentTarget = FindNearestEnemy(enemies);

        // Fire at target
        if (_currentTarget != null && _fireTimer >= 1.0f / FireRate)
        {
            Fire(_currentTarget);
            _fireTimer = 0.0f;
        }
    }

    private Enemy? FindNearestEnemy(List<Enemy> enemies)
    {
        Enemy? nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive || enemy.HasReachedEnd)
                continue;

            float distance = Vector2.Distance(Position, enemy.Position);
            if (distance <= Range && distance < nearestDistance)
            {
                nearest = enemy;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    private void Fire(Enemy target)
    {
        var projectile = new Projectile(Position, target, Damage);
        _projectiles.Add(projectile);
    }

    public bool CanUpgrade(UpgradeType upgradeType, int playerMoney)
    {
        int cost = upgradeType switch
        {
            UpgradeType.Range => RangeUpgradeCost,
            UpgradeType.Damage => DamageUpgradeCost,
            UpgradeType.FireRate => FireRateUpgradeCost,
            _ => int.MaxValue
        };

        return playerMoney >= cost;
    }

    public int GetUpgradeCost(UpgradeType upgradeType)
    {
        return upgradeType switch
        {
            UpgradeType.Range => RangeUpgradeCost,
            UpgradeType.Damage => DamageUpgradeCost,
            UpgradeType.FireRate => FireRateUpgradeCost,
            _ => 0
        };
    }

    public void Upgrade(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Range:
                Range += 20.0f;
                RangeLevel++;
                RangeUpgradeCost = (int)(RangeUpgradeCost * 1.5f);
                break;
            case UpgradeType.Damage:
                Damage += 15.0f;
                DamageLevel++;
                DamageUpgradeCost = (int)(DamageUpgradeCost * 1.5f);
                break;
            case UpgradeType.FireRate:
                FireRate += 0.5f;
                FireRateLevel++;
                FireRateUpgradeCost = (int)(FireRateUpgradeCost * 1.5f);
                break;
        }
    }

    public void Render(CanvasRenderer renderer)
    {
        // Draw tower base
        Color towerColor = Type switch
        {
            TowerType.Basic => Color.Blue,
            _ => Color.Blue
        };

        renderer.DrawCircle(Position, 12.0f, towerColor);

        // Draw range circle when selected (this would be handled by game state)
        // For now, we'll skip this to keep it simple

        // Draw barrel pointing towards target
        if (_currentTarget != null)
        {
            Vector2 direction = (_currentTarget.Position - Position).Normalized;
            Vector2 barrelEnd = Position + direction * 15.0f;
            renderer.DrawLine(Position, barrelEnd, Color.White, 3.0f);
        }

        // Render projectiles
        foreach (var projectile in _projectiles)
        {
            projectile.Render(renderer);
        }
    }

    public void RenderRange(CanvasRenderer renderer)
    {
        // Draw range circle
        Color rangeColor = new(1.0f, 1.0f, 1.0f, 0.2f);
        renderer.DrawCircle(Position, Range, rangeColor, 64);
    }

    public bool IsInRange(Vector2 position)
    {
        return Vector2.Distance(Position, position) <= Range;
    }
}
