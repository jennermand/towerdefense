using VectorTD.Core;
using VectorTD.Graphics;

namespace VectorTD.Game;

/// <summary>
/// The game state. Everything that matters happens here. Lives, money, towers, enemies.
/// The whole war in one place. Simple and clean. Like a good battle plan.
/// </summary>
public class GameState
{
    public int Lives { get; private set; }
    public int Money { get; private set; }
    public int CurrentWave => _enemySpawner.CurrentWave;
    public bool IsGameOver { get; private set; }

    private GameGrid _grid;
    private List<Enemy> _enemies;
    private List<Tower> _towers;
    private EnemySpawner _enemySpawner;
    private Tower? _selectedTower;
    private int _viewportWidth;
    private int _viewportHeight;

    public event Action? OnGameStateChanged;

    /// <summary>
    /// Creates a new game. Twenty lives. Fifty coins. A grid to fight on.
    /// Not much to start with. But enough. Wars have been won with less.
    /// </summary>
    public GameState()
    {
        Lives = 20;
        Money = 50;
        IsGameOver = false;

        _grid = new GameGrid(20, 15, 40.0f); // 20x15 grid with 40px cells
        _enemies = new List<Enemy>();
        _towers = new List<Tower>();
        _enemySpawner = new EnemySpawner(_grid.GetPathPoints());

        _viewportWidth = 800;
        _viewportHeight = 600;
    }

    /// <summary>
    /// Sets the viewport size. The window where war is watched.
    /// Bigger windows show more. Smaller windows show less. Simple.
    /// </summary>
    public void SetViewport(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
    }

    /// <summary>
    /// Updates the game. Time moves forward. Enemies spawn. Towers shoot.
    /// Lives are lost. Money changes hands. The war continues.
    /// Until it doesn't.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (IsGameOver)
            return;

        // Update enemy spawner
        _enemySpawner.Update(deltaTime, _enemies);

        // Update enemies
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.Update(deltaTime);

            // Check if enemy reached the end
            if (enemy.HasReachedEnd)
            {
                Lives--;
                _enemies.RemoveAt(i);
                OnGameStateChanged?.Invoke();
                
                if (Lives <= 0)
                {
                    IsGameOver = true;
                    OnGameStateChanged?.Invoke();
                }
            }
            // Remove dead enemies and give money
            else if (!enemy.IsAlive)
            {
                Money += enemy.Reward;
                _enemies.RemoveAt(i);
                OnGameStateChanged?.Invoke();
            }
        }

        // Update towers
        foreach (var tower in _towers)
        {
            tower.Update(deltaTime, _enemies);
        }
    }

    /// <summary>
    /// Renders the game. Draws everything that matters. Grid, towers, enemies.
    /// The selected tower gets special treatment. Shows its range.
    /// Like highlighting the important things in war.
    /// </summary>
    public void Render(CanvasRenderer renderer)
    {
        // Render grid
        _grid.Render(renderer);

        // Render towers
        foreach (var tower in _towers)
        {
            tower.Render(renderer);
        }

        // Render selected tower range
        if (_selectedTower != null)
        {
            _selectedTower.RenderRange(renderer);
        }

        // Render enemies
        foreach (var enemy in _enemies)
        {
            enemy.Render(renderer);
        }
    }

    /// <summary>
    /// Tries to place a tower. Needs money. Needs empty ground.
    /// Both are hard to find. Success is rare. Failure is common.
    /// Like most things worth doing.
    /// </summary>
    public bool TryPlaceTower(TowerType towerType, Vector2 position)
    {
        // Check if we can afford the tower
        int cost = GetTowerCost(towerType);
        if (Money < cost)
            return false;

        // Check if position is valid
        if (!_grid.CanPlaceTower(position))
            return false;

        // Create and place tower
        var tower = new Tower(towerType, position);
        if (_grid.PlaceTower(position, tower))
        {
            _towers.Add(tower);
            Money -= cost;
            OnGameStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the tower at a position. Either there's one there or there isn't.
    /// Simple question. Simple answer. Like most things should be.
    /// </summary>
    public Tower? GetTowerAt(Vector2 position)
    {
        return _grid.GetTowerAt(position);
    }

    /// <summary>
    /// Selects a tower. Makes it special. Gives it attention.
    /// Everyone needs to be chosen sometimes. Even towers.
    /// </summary>
    public void SelectTower(Tower? tower)
    {
        _selectedTower = tower;
    }

    /// <summary>
    /// Upgrades a tower. Makes it better. Costs money.
    /// Everything good costs something. Nothing is free.
    /// Especially in war.
    /// </summary>
    public bool UpgradeTower(Tower tower, UpgradeType upgradeType)
    {
        if (!tower.CanUpgrade(upgradeType, Money))
            return false;

        int cost = tower.GetUpgradeCost(upgradeType);
        tower.Upgrade(upgradeType);
        Money -= cost;
        OnGameStateChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Handles mouse clicks. Click on a tower, select it. Click on empty space, select nothing.
    /// Simple choices. Clear results. The way things should work.
    /// </summary>
    public void HandleMouseClick(Vector2 position)
    {
        // Check if clicking on a tower
        var tower = GetTowerAt(position);
        if (tower != null)
        {
            SelectTower(tower);
        }
        else
        {
            SelectTower(null);
        }
    }

    /// <summary>
    /// Handles mouse movement. The cursor moves. Things might change.
    /// Or they might not. Depends on what's underneath.
    /// </summary>
    public void HandleMouseMove(Vector2 position)
    {
        // Handle mouse move events if needed
    }

    /// <summary>
    /// Handles mouse press. The button goes down. Decisions are made.
    /// Towers are selected. Orders are given. War begins with a click.
    /// </summary>
    public void HandleMouseDown(Vector2 position)
    {
        // Handle mouse down events if needed
    }

    /// <summary>
    /// Handles mouse release. The button comes up. Actions complete.
    /// What was started must finish. Like everything else.
    /// </summary>
    public void HandleMouseUp(Vector2 position)
    {
        // Handle mouse up events if needed
    }

    private int GetTowerCost(TowerType towerType)
    {
        return towerType switch
        {
            TowerType.Basic => 10,
            _ => 10
        };
    }

    public void RestartGame()
    {
        Lives = 20;
        Money = 50;
        IsGameOver = false;
        
        _enemies.Clear();
        _towers.Clear();
        _grid = new GameGrid(20, 15, 40.0f);
        _enemySpawner = new EnemySpawner(_grid.GetPathPoints());
        _selectedTower = null;
        
        OnGameStateChanged?.Invoke();
    }
}
