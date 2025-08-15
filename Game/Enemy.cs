using VectorTD.Core;
using VectorTD.Graphics;

namespace VectorTD.Game;

public enum EnemyType
{
    Basic
}

public class Enemy
{
    public Vector2 Position { get; set; }
    public float MaxHealth { get; private set; }
    public float Health { get; set; }
    public float Speed { get; private set; }
    public float Size { get; private set; }
    public int Reward { get; private set; }
    public EnemyType Type { get; private set; }
    public bool IsAlive => Health > 0;
    public bool HasReachedEnd { get; set; }

    private List<Vector2> _pathPoints;
    private int _currentPathIndex;
    private float _pathProgress;

    public Enemy(EnemyType type, List<Vector2> pathPoints)
    {
        Type = type;
        _pathPoints = new List<Vector2>(pathPoints);
        _currentPathIndex = 0;
        _pathProgress = 0.0f;
        HasReachedEnd = false;

        // Set properties based on enemy type
        switch (type)
        {
            case EnemyType.Basic:
                MaxHealth = 100.0f;
                Health = MaxHealth;
                Speed = 50.0f; // pixels per second
                Size = 8.0f;
                Reward = 5;
                break;
        }

        if (_pathPoints.Count > 0)
        {
            Position = _pathPoints[0];
        }
    }

    public void Update(float deltaTime)
    {
        if (!IsAlive || HasReachedEnd || _pathPoints.Count < 2)
            return;

        // Move along the path
        float moveDistance = Speed * deltaTime;
        
        while (moveDistance > 0 && _currentPathIndex < _pathPoints.Count - 1)
        {
            Vector2 currentPoint = _pathPoints[_currentPathIndex];
            Vector2 nextPoint = _pathPoints[_currentPathIndex + 1];
            Vector2 direction = nextPoint - currentPoint;
            float segmentLength = direction.Length;

            if (segmentLength == 0)
            {
                _currentPathIndex++;
                continue;
            }

            Vector2 normalizedDirection = direction.Normalized;
            float remainingDistance = segmentLength * (1.0f - _pathProgress);

            if (moveDistance >= remainingDistance)
            {
                // Move to next path point
                moveDistance -= remainingDistance;
                _currentPathIndex++;
                _pathProgress = 0.0f;
                
                if (_currentPathIndex >= _pathPoints.Count - 1)
                {
                    // Reached the end
                    Position = _pathPoints[_pathPoints.Count - 1];
                    HasReachedEnd = true;
                    break;
                }
            }
            else
            {
                // Move along current segment
                _pathProgress += moveDistance / segmentLength;
                moveDistance = 0;
            }
        }

        // Update position based on current path progress
        if (_currentPathIndex < _pathPoints.Count - 1)
        {
            Vector2 currentPoint = _pathPoints[_currentPathIndex];
            Vector2 nextPoint = _pathPoints[_currentPathIndex + 1];
            Position = currentPoint + (nextPoint - currentPoint) * _pathProgress;
        }
    }

    public void TakeDamage(float damage)
    {
        Health = Math.Max(0, Health - damage);
    }

    public void Render(CanvasRenderer renderer)
    {
        if (!IsAlive)
            return;

        // Draw enemy based on type
        Color enemyColor = Type switch
        {
            EnemyType.Basic => Color.Red,
            _ => Color.Red
        };

        // Draw enemy as a circle
        renderer.DrawCircle(Position, Size, enemyColor);

        // Draw health bar
        if (Health < MaxHealth)
        {
            float healthBarWidth = Size * 2;
            float healthBarHeight = 3;
            Vector2 healthBarPos = new(Position.X - healthBarWidth * 0.5f, Position.Y - Size - 8);

            // Background
            renderer.DrawRectangle(healthBarPos, new Vector2(healthBarWidth, healthBarHeight), Color.DarkGray);
            
            // Health
            float healthPercent = Health / MaxHealth;
            Color healthColor = healthPercent > 0.5f ? Color.Green : (healthPercent > 0.25f ? Color.Yellow : Color.Red);
            renderer.DrawRectangle(healthBarPos, new Vector2(healthBarWidth * healthPercent, healthBarHeight), healthColor);
        }
    }

    public float GetDistanceToEnd()
    {
        if (_pathPoints.Count < 2 || _currentPathIndex >= _pathPoints.Count - 1)
            return 0;

        float totalDistance = 0;

        // Distance to next path point
        if (_currentPathIndex < _pathPoints.Count - 1)
        {
            Vector2 currentPoint = _pathPoints[_currentPathIndex];
            Vector2 nextPoint = _pathPoints[_currentPathIndex + 1];
            float segmentLength = (nextPoint - currentPoint).Length;
            totalDistance += segmentLength * (1.0f - _pathProgress);
        }

        // Distance for remaining path segments
        for (int i = _currentPathIndex + 1; i < _pathPoints.Count - 1; i++)
        {
            totalDistance += (_pathPoints[i + 1] - _pathPoints[i]).Length;
        }

        return totalDistance;
    }
}
