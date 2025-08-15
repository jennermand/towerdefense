using VectorTD.Core;
using VectorTD.Graphics;

namespace VectorTD.Game;

public class Projectile
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Damage { get; private set; }
    public bool IsActive { get; set; }
    
    private Enemy _target;
    private float _speed;
    private float _lifetime;
    private const float MaxLifetime = 2.0f; // Maximum time before projectile expires

    public Projectile(Vector2 startPosition, Enemy target, float damage)
    {
        Position = startPosition;
        _target = target;
        Damage = damage;
        _speed = 200.0f; // pixels per second
        IsActive = true;
        _lifetime = 0.0f;

        // Calculate initial velocity towards target
        Vector2 direction = (target.Position - startPosition).Normalized;
        Velocity = direction * _speed;
    }

    public void Update(float deltaTime)
    {
        if (!IsActive)
            return;

        _lifetime += deltaTime;

        // Check if projectile has expired
        if (_lifetime > MaxLifetime)
        {
            IsActive = false;
            return;
        }

        // Update position
        Position += Velocity * deltaTime;

        // Check collision with target
        if (_target.IsAlive && !_target.HasReachedEnd)
        {
            float distance = Vector2.Distance(Position, _target.Position);
            if (distance <= _target.Size + 2.0f) // Small collision radius
            {
                // Hit the target
                _target.TakeDamage(Damage);
                IsActive = false;
                return;
            }

            // Update velocity to track target (simple homing)
            Vector2 directionToTarget = (_target.Position - Position).Normalized;
            Velocity = directionToTarget * _speed;
        }
        else
        {
            // Target is dead or reached end, projectile continues in straight line
            // It will expire after MaxLifetime
        }
    }

    public void Render(CanvasRenderer renderer)
    {
        if (!IsActive)
            return;

        // Draw projectile as a small yellow circle
        renderer.DrawCircle(Position, 3.0f, Color.Yellow);
    }
}
