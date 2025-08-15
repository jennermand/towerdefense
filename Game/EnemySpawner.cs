using VectorTD.Core;

namespace VectorTD.Game;

public class EnemySpawner
{
    private List<Vector2> _pathPoints;
    private float _spawnTimer;
    private float _spawnInterval;
    private int _currentWave;
    private int _enemiesSpawnedThisWave;
    private int _enemiesPerWave;
    private float _timeBetweenWaves;
    private float _waveTimer;
    private bool _isSpawningWave;

    public int CurrentWave => _currentWave;
    public bool IsActive { get; set; } = true;

    public EnemySpawner(List<Vector2> pathPoints)
    {
        _pathPoints = new List<Vector2>(pathPoints);
        _spawnTimer = 0.0f;
        _spawnInterval = 1.0f; // 1 second between spawns
        _currentWave = 1;
        _enemiesSpawnedThisWave = 0;
        _enemiesPerWave = 10;
        _timeBetweenWaves = 5.0f; // 5 seconds between waves
        _waveTimer = 0.0f;
        _isSpawningWave = true;
    }

    public void Update(float deltaTime, List<Enemy> enemies)
    {
        if (!IsActive)
            return;

        if (_isSpawningWave)
        {
            _spawnTimer += deltaTime;

            if (_spawnTimer >= _spawnInterval && _enemiesSpawnedThisWave < _enemiesPerWave)
            {
                // Spawn enemy
                var enemy = new Enemy(EnemyType.Basic, _pathPoints);
                enemies.Add(enemy);
                
                _enemiesSpawnedThisWave++;
                _spawnTimer = 0.0f;

                // Check if wave is complete
                if (_enemiesSpawnedThisWave >= _enemiesPerWave)
                {
                    _isSpawningWave = false;
                    _waveTimer = 0.0f;
                }
            }
        }
        else
        {
            // Wait between waves
            _waveTimer += deltaTime;

            if (_waveTimer >= _timeBetweenWaves)
            {
                // Start next wave
                _currentWave++;
                _enemiesSpawnedThisWave = 0;
                _isSpawningWave = true;
                _spawnTimer = 0.0f;

                // Increase difficulty
                _enemiesPerWave += 2; // More enemies each wave
                _spawnInterval = Math.Max(0.3f, _spawnInterval - 0.05f); // Faster spawning
            }
        }
    }
}
