using VectorTD.Core;
using System.Linq;

namespace VectorTD.Game;

/// <summary>
/// The game. Everything that matters. Money, enemies, towers, waves.
/// Time moves forward. Enemies spawn. Towers shoot. Players lose.
/// Sometimes they win. Not often.
/// </summary>
public class SimpleGameState
{
    public int Lives { get; private set; } = 20;
    public int Money { get; private set; } = 50;
    public int CurrentWave { get; private set; } = 1;
    public bool IsGameOver { get; private set; } = false;
    public int EnemiesKilled { get; private set; } = 0;
    public int TowersBuilt { get; private set; } = 0;
    public bool IsWaveBreak => _isWaveBreak;
    public float WaveBreakTimeRemaining => _isWaveBreak ? WaveBreakDuration - _waveBreakTimer : 0f;

    /// <summary>
    /// Has the game started. Only starts when first tower is built.
    /// No enemies spawn until then. Time to think. Time to plan.
    /// </summary>
    public bool HasGameStarted => TowersBuilt > 0;

    private List<SimpleEnemy> _enemies = new();
    private List<SimpleTower> _towers = new();
    private List<DeathMarker> _deathMarkers = new();
    private float _spawnTimer = 0f;
    private float _spawnInterval = 0.8f; // 0.8 seconds between spawns (much closer together)
    private int _enemiesSpawnedThisWave = 0;
    private int _enemiesPerWave = 3; // Start with 3 enemies in wave 1
    private float _waveBreakTimer = 0f;
    private bool _isWaveBreak = false;
    private const float WaveBreakDuration = 2f; // 2 second break between waves

    public event Action? OnGameStateChanged;

    /// <summary>
    /// Creates a new game. Gives you money to start. Not much.
    /// Enough to build a tower or two. Then you're on your own.
    /// </summary>
    public SimpleGameState()
    {
        // Initialize with some money
        Money = 100;

        // No test pixels - only create them when enemies actually die
    }

    /// <summary>
    /// Updates the game. Time moves. Enemies spawn. Towers shoot. People die.
    /// Waves end. New waves begin. Money changes hands. Lives are lost.
    /// The game continues until it doesn't.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (IsGameOver) return;



        // Handle wave break
        if (_isWaveBreak)
        {
            _waveBreakTimer += deltaTime;
            if (_waveBreakTimer >= WaveBreakDuration)
            {
                _isWaveBreak = false;
                _waveBreakTimer = 0f;
                StartNextWave();
            }
            return; // Don't spawn enemies during wave break
        }

        // Update spawn timer - but only spawn enemies after first tower is built
        if (HasGameStarted)
        {
            _spawnTimer += deltaTime;

            if (_spawnTimer >= _spawnInterval && _enemiesSpawnedThisWave < _enemiesPerWave)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
            }
        }

        // Update enemies
        var enemiesToAdd = new List<SimpleEnemy>();

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.Update(deltaTime);

            if (enemy.HasReachedEnd)
            {
                Lives--;
                _enemies.RemoveAt(i);
                OnGameStateChanged?.Invoke();

                if (Lives <= 0)
                {
                    IsGameOver = true;
                }
            }
            else if (enemy.Health <= 0)
            {
                // Create death marker when enemy dies
                var deathMarker = new DeathMarker
                {
                    Position = enemy.Position,
                    Life = 2.0f, // 2 seconds visible
                    MaxLife = 2.0f
                };
                _deathMarkers.Add(deathMarker);

                // Process death immediately
                Money += enemy.Reward;
                EnemiesKilled++;

                // Check if enemy can split
                if (enemy.CanSplit())
                {
                    // Calculate wave multiplier for split enemies
                    float waveMultiplier = MathF.Pow(1.02f, CurrentWave - 1);
                    var splitEnemies = enemy.CreateSplitEnemies(waveMultiplier);
                    enemiesToAdd.AddRange(splitEnemies);
                }

                _enemies.RemoveAt(i);
                OnGameStateChanged?.Invoke();
            }
        }

        // Add split enemies to the list
        _enemies.AddRange(enemiesToAdd);

        // Update towers
        foreach (var tower in _towers)
        {
            tower.Update(deltaTime, _enemies);
        }

        // Update death markers
        foreach (var marker in _deathMarkers)
        {
            marker.Update(deltaTime);
        }

        // Remove expired death markers
        _deathMarkers.RemoveAll(m => !m.IsAlive);

        // Check for next wave
        if (_enemiesSpawnedThisWave >= _enemiesPerWave && _enemies.Count == 0 && !_isWaveBreak)
        {
            StartWaveBreak();
        }
    }

    /// <summary>
    /// Spawns an enemy. Each wave they get stronger. Two percent stronger.
    /// Doesn't sound like much. It adds up. Like compound interest but with death.
    /// </summary>
    private void SpawnEnemy()
    {
        // Calculate wave multiplier (2% stronger each wave)
        float waveMultiplier = MathF.Pow(1.02f, CurrentWave - 1);

        // Determine enemy type based on wave and randomness
        var (color, shape) = DetermineEnemyType();
        var enemy = new SimpleEnemy(color, shape, waveMultiplier);
        _enemies.Add(enemy);
        _enemiesSpawnedThisWave++;
    }

    /// <summary>
    /// Determines what type of enemy to spawn. Color and shape.
    /// Early waves are easy. Red enemies only. Later waves bring trouble.
    /// Green enemies in wave two. Blue enemies in wave six. Then chaos.
    /// </summary>
    private (EnemyColor color, EnemyShape shape) DetermineEnemyType()
    {
        var random = new Random();

        // Determine color based on wave (introduce stronger enemies in later waves)
        EnemyColor color = CurrentWave switch
        {
            1 => EnemyColor.Red,  // Wave 1: Only red
            2 or 3 => random.Next(100) < 80 ? EnemyColor.Red : EnemyColor.Green,  // Wave 2-3: Mostly red, some green
            4 or 5 => random.Next(100) < 60 ? EnemyColor.Red : EnemyColor.Green,  // Wave 4-5: Mix of red and green
            6 or 7 => random.Next(100) switch  // Wave 6-7: All three colors
            {
                < 50 => EnemyColor.Red,
                < 80 => EnemyColor.Green,
                _ => EnemyColor.Blue
            },
            _ => random.Next(100) switch  // Wave 8+: More challenging enemies
            {
                < 30 => EnemyColor.Red,
                < 65 => EnemyColor.Green,
                _ => EnemyColor.Blue
            }
        };

        // Random shape
        EnemyShape shape = (EnemyShape)random.Next(3);

        return (color, shape);
    }

    /// <summary>
    /// Starts a wave break. Time to breathe. Time to build. Time to prepare.
    /// The calm between storms. Use it wisely. The next wave is coming.
    /// </summary>
    private void StartWaveBreak()
    {
        _isWaveBreak = true;
        _waveBreakTimer = 0f;
        OnGameStateChanged?.Invoke();
    }

    private void StartNextWave()
    {
        CurrentWave++;
        _enemiesSpawnedThisWave = 0;
        _enemiesPerWave += 2; // Add 2 more enemies each wave
        _spawnInterval = Math.Max(0.3f, _spawnInterval - 0.05f); // Faster spawning each wave (minimum 0.3s)
        OnGameStateChanged?.Invoke();

        Console.WriteLine($"Starting Wave {CurrentWave} with {_enemiesPerWave} enemies, spawn interval: {_spawnInterval:F1}s");
    }

    /// <summary>
    /// Tries to build a tower. Needs money. Needs empty space.
    /// Red towers cost less. Blue towers cost more. Green towers cost middle.
    /// Like everything else in life. You get what you pay for.
    /// </summary>
    public bool TryBuildTower(int gridX, int gridY, TowerColor color = TowerColor.Red)
    {
        int towerCost = GetTowerCost(color);

        if (Money < towerCost) return false;
        if (GetTowerAt(gridX, gridY) != null) return false;

        var tower = new SimpleTower(gridX, gridY, color);
        _towers.Add(tower);
        Money -= towerCost;
        TowersBuilt++;
        OnGameStateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Gets the cost of a tower. Red is cheap. Green costs more. Blue costs most.
    /// You get what you pay for. Better towers cost more money.
    /// Simple economics. Supply and demand. Power has a price.
    /// </summary>
    private int GetTowerCost(TowerColor color)
    {
        return color switch
        {
            TowerColor.Red => 10,
            TowerColor.Green => 15,   // More expensive
            TowerColor.Blue => 25,    // Most expensive
            _ => 10
        };
    }

    public SimpleTower? GetTowerAt(int gridX, int gridY)
    {
        return _towers.FirstOrDefault(t => t.GridX == gridX && t.GridY == gridY);
    }

    public bool TryUpgradeTower(SimpleTower tower, string upgradeType)
    {
        int cost = upgradeType.ToLower() switch
        {
            "range" => tower.RangeUpgradeCost,
            "firerate" => tower.FireRateUpgradeCost,
            "damage" => tower.DamageUpgradeCost,
            _ => int.MaxValue
        };

        if (Money < cost) return false;

        bool success = upgradeType.ToLower() switch
        {
            "range" => tower.TryUpgradeRange(),
            "firerate" => tower.TryUpgradeFireRate(),
            "damage" => tower.TryUpgradeDamage(),
            _ => false
        };

        if (success)
        {
            Money -= cost;
            OnGameStateChanged?.Invoke();
        }

        return success;
    }



    public List<SimpleEnemy> GetEnemies() => new(_enemies);
    public List<SimpleTower> GetTowers() => new(_towers);
    public int GetEnemiesPerWave() => _enemiesPerWave;
    public List<DeathMarker> GetDeathMarkers() => new(_deathMarkers);

    public void RestartGame()
    {
        Lives = 20;
        Money = 50;
        CurrentWave = 1;
        IsGameOver = false;
        EnemiesKilled = 0;
        TowersBuilt = 0;
        _enemies.Clear();
        _towers.Clear();
        _spawnTimer = 0f;
        _spawnInterval = 2f;
        _enemiesSpawnedThisWave = 0;
        _enemiesPerWave = 5;
        OnGameStateChanged?.Invoke();
    }
}
