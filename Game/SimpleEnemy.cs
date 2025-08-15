using VectorTD.Core;

namespace VectorTD.Game;

/// <summary>
/// Enemy colors. Red dies easy. Green takes more. Blue takes much more.
/// Like the old traffic lights, but backwards.
/// </summary>
public enum EnemyColor
{
    Red,    // Base health - dies quick
    Green,  // Double health - takes work
    Blue    // Quadruple health - real trouble
}

/// <summary>
/// Enemy shapes. They break apart when killed. Like everything else.
/// Squares become triangles. Triangles become circles. Circles become nothing.
/// </summary>
public enum EnemyShape
{
    Circle,    // The end. No more splitting.
    Triangle,  // Breaks into three circles when it dies
    Square     // Breaks into two triangles when it dies
}

/// <summary>
/// An enemy. Walks the path. Takes damage. Dies. Sometimes splits into smaller enemies.
/// Each wave makes them stronger. Like life, but with more geometry.
/// </summary>
public class SimpleEnemy
{
    public int Id { get; private set; }
    public EnemyColor Color { get; private set; }
    public EnemyShape Shape { get; private set; }
    public float Health { get; set; }
    public float MaxHealth { get; private set; }
    public float Speed { get; private set; } = 60f; // pixels per second
    public int Reward { get; private set; }
    public bool HasReachedEnd { get; set; } = false;

    private static int _nextId = 1;

    // Damage flash effect
    private float _damageFlashTimer = 0f;
    public bool IsFlashing => _damageFlashTimer > 0f;


    
    // Current waypoint index and position
    public int CurrentWaypoint { get; private set; } = 0;
    public Vector2 Position { get; private set; }
    public float PathProgress { get; private set; } = 0f;

    // Waypoints matching the green/yellow/red dots in the UI
    private static readonly List<Vector2> Waypoints = new()
    {
        new(50, 50),    // Green start dot
        new(200, 50),   // Yellow dot
        new(200, 150),  // Yellow dot
        new(350, 150),  // Yellow dot
        new(350, 250),  // Yellow dot
        new(500, 250),  // Yellow dot
        new(500, 350),  // Yellow dot
        new(650, 350),  // Yellow dot
        new(650, 450),  // Yellow dot
        new(750, 450)   // Red end dot
    };

    /// <summary>
    /// Creates an enemy. Red circle by default. Weak and round.
    /// Give it color for strength. Give it shape for splitting.
    /// Give it wave multiplier for the real pain.
    /// </summary>
    public SimpleEnemy(EnemyColor color = EnemyColor.Red, EnemyShape shape = EnemyShape.Circle, float waveMultiplier = 1.0f)
    {
        InitializeEnemy(color, shape, waveMultiplier);

        // Start at the first waypoint (green dot)
        Position = Waypoints[0];
        CurrentWaypoint = 0;
    }

    /// <summary>
    /// Initializes the enemy. Sets its color, shape, and health.
    /// Each wave makes them stronger. Each color makes them tougher.
    /// Blue enemies are the worst. They take forever to kill.
    /// </summary>
    private void InitializeEnemy(EnemyColor color, EnemyShape shape, float waveMultiplier = 1.0f)
    {
        // Assign unique ID
        Id = _nextId++;

        // Set enemy type
        Color = color;
        Shape = shape;

        // Set health based on color with wave scaling
        float baseHealth = 50f; // Back to normal health for proper gameplay
        float scaledHealth = baseHealth * waveMultiplier;
        MaxHealth = Color switch
        {
            EnemyColor.Red => scaledHealth,
            EnemyColor.Green => scaledHealth * 2f,   // 200% health
            EnemyColor.Blue => scaledHealth * 4f,    // 400% health
            _ => scaledHealth
        };
        Health = MaxHealth;

        // Set reward based on difficulty and shape (smaller enemies give less reward)
        int baseReward = Color switch
        {
            EnemyColor.Red => 5,
            EnemyColor.Green => 12,   // Higher reward for tougher enemies
            EnemyColor.Blue => 25,    // Even higher reward
            _ => 5
        };

        // Adjust reward based on shape (split enemies give less reward)
        Reward = Shape switch
        {
            EnemyShape.Square => baseReward,           // Full reward
            EnemyShape.Triangle => (int)(baseReward * 0.6f),  // 60% reward
            EnemyShape.Circle => (int)(baseReward * 0.4f),    // 40% reward
            _ => baseReward
        };
    }

    /// <summary>
    /// Updates the enemy. Moves it along the path. Time passes. Things change.
    /// Dead enemies don't move. They just lie there.
    /// </summary>
    public void Update(float deltaTime)
    {
        // Update damage flash timer
        _damageFlashTimer -= deltaTime;

        // Don't move if dead
        if (HasReachedEnd || Health <= 0) return;

        // Check if we've reached the end
        if (CurrentWaypoint >= Waypoints.Count - 1)
        {
            HasReachedEnd = true;
            return;
        }

        // Get current and next waypoint
        Vector2 currentWaypoint = Waypoints[CurrentWaypoint];
        Vector2 nextWaypoint = Waypoints[CurrentWaypoint + 1];

        // Move towards the next waypoint
        Vector2 direction = nextWaypoint - currentWaypoint;
        float distance = direction.Length;

        if (distance > 0)
        {
            Vector2 normalizedDirection = direction.Normalized;
            float moveDistance = Speed * deltaTime;

            // Move towards next waypoint
            Vector2 newPosition = Position + normalizedDirection * moveDistance;

            // Check if we've reached the next waypoint
            float distanceToNext = Vector2.Distance(newPosition, nextWaypoint);

            if (distanceToNext <= 5f) // Close enough to waypoint
            {
                // Snap to waypoint and move to next one
                Position = nextWaypoint;
                CurrentWaypoint++;
                PathProgress = (float)CurrentWaypoint / (Waypoints.Count - 1);
            }
            else
            {
                Position = newPosition;
            }
        }
    }



    /// <summary>
    /// Takes damage. Health goes down. Sometimes to zero.
    /// Flashes white when hit. Like everything that hurts.
    /// </summary>
    public void TakeDamage(float damage)
    {
        float oldHealth = Health;
        Health = Math.Max(0, Health - damage);
        _damageFlashTimer = 0.2f; // Flash for 200ms

        Console.WriteLine($"Enemy {Id} took {damage} damage: {oldHealth:F1} -> {Health:F1}");

        if (Health <= 0)
        {
            Console.WriteLine($"Enemy {Id} KILLED!");
        }
    }

    /// <summary>
    /// Creates the children when this enemy dies. Squares make triangles.
    /// Triangles make circles. Circles make nothing. Death makes more death.
    /// Each child is weaker but there are more of them. Mathematics of war.
    /// </summary>
    public List<SimpleEnemy> CreateSplitEnemies(float waveMultiplier = 1.0f)
    {
        var splitEnemies = new List<SimpleEnemy>();

        switch (Shape)
        {
            case EnemyShape.Square:
                // Square splits into 2 triangles
                for (int i = 0; i < 2; i++)
                {
                    var triangle = CreateSplitEnemy(EnemyShape.Triangle, i, 2, waveMultiplier);
                    splitEnemies.Add(triangle);
                }
                break;

            case EnemyShape.Triangle:
                // Triangle splits into 3 circles
                for (int i = 0; i < 3; i++)
                {
                    var circle = CreateSplitEnemy(EnemyShape.Circle, i, 3, waveMultiplier);
                    splitEnemies.Add(circle);
                }
                break;

            case EnemyShape.Circle:
                // Circles don't split further. The end.
                break;
        }

        return splitEnemies;
    }

    /// <summary>
    /// Checks if this enemy can split when it dies. Squares and triangles can.
    /// Circles cannot. They're the end of the line. Final form.
    /// </summary>
    public bool CanSplit()
    {
        return Shape == EnemyShape.Square || Shape == EnemyShape.Triangle;
    }

    /// <summary>
    /// Creates a single split enemy. Smaller and weaker than the parent.
    /// Spawns exactly where the parent died. Continues the journey from there.
    /// Each generation gets weaker. But there are more of them.
    /// </summary>
    private SimpleEnemy CreateSplitEnemy(EnemyShape newShape, int index, int totalCount, float waveMultiplier = 1.0f)
    {
        var splitEnemy = new SimpleEnemy();
        splitEnemy.InitializeEnemy(Color, newShape, waveMultiplier);

        // Copy EXACT position and progress from the dead enemy
        splitEnemy.Position = Position;  // Spawn exactly where parent died
        splitEnemy.CurrentWaypoint = CurrentWaypoint;
        splitEnemy.PathProgress = PathProgress;

        // Reduce health based on split level
        float healthMultiplier = newShape switch
        {
            EnemyShape.Triangle => 0.6f,  // Triangles have 60% of square health
            EnemyShape.Circle => 0.4f,    // Circles have 40% of triangle health
            _ => 1.0f
        };

        splitEnemy.MaxHealth = MaxHealth * healthMultiplier;
        splitEnemy.Health = splitEnemy.MaxHealth;

        // NO POSITION OFFSET - spawn exactly at death coordinates
        // All split enemies start at the same position where parent died

        return splitEnemy;
    }



    public float GetDistanceToEnd()
    {
        // Simple calculation based on remaining waypoints
        float distance = 0f;
        for (int i = CurrentWaypoint; i < Waypoints.Count - 1; i++)
        {
            distance += Vector2.Distance(Waypoints[i], Waypoints[i + 1]);
        }
        return distance;
    }
}
