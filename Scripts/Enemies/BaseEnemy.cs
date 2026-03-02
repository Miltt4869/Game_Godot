using Godot;
using System;
using System.Collections.Generic;

public partial class BaseEnemy : CharacterBody2D
{
    // Stats
    [Export] public int MaxHealth = 50;
    [Export] public int AttackDamage = 10;
    [Export] public float MoveSpeed = 80.0f;
    [Export] public float Gravity = 980.0f;
    [Export] public int ScoreValue = 100;

    // Patrol
    [Export] public float PatrolDistance = 150.0f;
    [Export] public float DetectRange = 250.0f;
    [Export] public float AttackRange = 40.0f;
    [Export] public float AttackCooldown = 1.0f;

    // State
    protected int Health;
    protected bool IsDead = false;
    protected bool IsHurt = false;
    protected bool CanAttackPlayer = true;

    // Patrol
    protected Vector2 StartPosition;
    protected int PatrolDirection = 1;

    // Components
    protected AnimatedSprite2D AnimSprite;
    protected Area2D DetectArea;
    protected Area2D AttackArea;
    protected Timer AttackCooldownTimer;
    protected Timer HurtTimer;
    protected Timer DeathTimer;

    // Player reference
    protected Player TargetPlayer;

    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    protected EnemyState CurrentState = EnemyState.Patrol;

    public override void _Ready()
    {
        Health = MaxHealth;
        StartPosition = GlobalPosition;
        AddToGroup("enemies");

        // Get components
        AnimSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Auto-create sprites if none assigned
        if (AnimSprite.SpriteFrames == null)
        {
            CreatePlaceholderSprites();
        }

        // Create attack cooldown timer
        AttackCooldownTimer = new Timer();
        AttackCooldownTimer.WaitTime = AttackCooldown;
        AttackCooldownTimer.OneShot = true;
        AttackCooldownTimer.Timeout += () => { CanAttackPlayer = true; };
        AddChild(AttackCooldownTimer);

        // Create hurt timer
        HurtTimer = new Timer();
        HurtTimer.WaitTime = 0.3f;
        HurtTimer.OneShot = true;
        HurtTimer.Timeout += () => { IsHurt = false; CurrentState = EnemyState.Patrol; };
        AddChild(HurtTimer);

        // Create death timer
        DeathTimer = new Timer();
        DeathTimer.WaitTime = 1.0f;
        DeathTimer.OneShot = true;
        DeathTimer.Timeout += OnDeathTimerTimeout;
        AddChild(DeathTimer);

        // Setup detect area if it exists
        if (HasNode("DetectArea"))
        {
            DetectArea = GetNode<Area2D>("DetectArea");
            DetectArea.BodyEntered += OnDetectAreaBodyEntered;
            DetectArea.BodyExited += OnDetectAreaBodyExited;
        }

        // Setup attack area if it exists  
        if (HasNode("HitArea"))
        {
            AttackArea = GetNode<Area2D>("HitArea");
            AttackArea.BodyEntered += OnHitAreaBodyEntered;
        }

        AnimSprite.AnimationFinished += OnAnimationFinished;
    }

    /// <summary>
    /// Override in subclasses for custom sprites. Default creates colored rectangles.
    /// </summary>
    protected virtual void CreatePlaceholderSprites()
    {
        var tex = SpriteHelper.CreateColoredRect(32, 32, Colors.Red);
        var animations = new Dictionary<string, Texture2D[]>
        {
            { "walk", new Texture2D[] { tex } },
            { "attack", new Texture2D[] { tex } },
            { "hurt", new Texture2D[] { tex } },
            { "die", new Texture2D[] { tex } }
        };
        AnimSprite.SpriteFrames = SpriteHelper.BuildSpriteFrames(animations);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead) return;

        Vector2 velocity = Velocity;

        // Apply gravity
        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
        }

        switch (CurrentState)
        {
            case EnemyState.Patrol:
                velocity.X = PatrolDirection * MoveSpeed;
                AnimSprite.FlipH = PatrolDirection < 0;

                // Check patrol bounds
                if (Math.Abs(GlobalPosition.X - StartPosition.X) >= PatrolDistance)
                {
                    PatrolDirection *= -1;
                }

                // Check for walls
                if (IsOnWall())
                {
                    PatrolDirection *= -1;
                }

                AnimSprite.Play("walk");
                break;

            case EnemyState.Chase:
                if (TargetPlayer != null && !TargetPlayer.IsQueuedForDeletion())
                {
                    float dirToPlayer = Mathf.Sign(TargetPlayer.GlobalPosition.X - GlobalPosition.X);
                    velocity.X = dirToPlayer * MoveSpeed * 1.5f;
                    AnimSprite.FlipH = dirToPlayer < 0;

                    float dist = GlobalPosition.DistanceTo(TargetPlayer.GlobalPosition);
                    if (dist <= AttackRange && CanAttackPlayer)
                    {
                        CurrentState = EnemyState.Attack;
                        velocity.X = 0;
                    }
                }
                else
                {
                    CurrentState = EnemyState.Patrol;
                }
                AnimSprite.Play("walk");
                break;

            case EnemyState.Attack:
                velocity.X = 0;
                if (!IsHurt)
                {
                    AnimSprite.Play("attack");
                }
                break;

            case EnemyState.Hurt:
                velocity.X = 0;
                AnimSprite.Play("hurt");
                break;

            case EnemyState.Dead:
                velocity.X = 0;
                break;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        Health -= damage;
        IsHurt = true;
        CurrentState = EnemyState.Hurt;
        HurtTimer.Start();

        // Flash effect
        AnimSprite.Modulate = new Color(1, 0.3f, 0.3f);
        var tween = CreateTween();
        tween.TweenProperty(AnimSprite, "modulate", Colors.White, 0.3f);

        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        IsDead = true;
        CurrentState = EnemyState.Dead;
        AnimSprite.Play("die");
        GameManager.Instance.AddScore(ScoreValue);

        // Disable collisions
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
        if (HasNode("DetectArea/CollisionShape2D"))
            GetNode<CollisionShape2D>("DetectArea/CollisionShape2D").SetDeferred("disabled", true);
        if (HasNode("HitArea/CollisionShape2D"))
            GetNode<CollisionShape2D>("HitArea/CollisionShape2D").SetDeferred("disabled", true);
        DeathTimer.Start();
    }

    private void OnDeathTimerTimeout()
    {
        QueueFree();
    }

    protected virtual void OnAnimationFinished()
    {
        if (AnimSprite.Animation == "attack")
        {
            if (TargetPlayer != null)
            {
                CurrentState = EnemyState.Chase;
            }
            else
            {
                CurrentState = EnemyState.Patrol;
            }
            CanAttackPlayer = false;
            AttackCooldownTimer.Start();
        }
    }

    private void OnDetectAreaBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            TargetPlayer = player;
            if (CurrentState == EnemyState.Patrol)
            {
                CurrentState = EnemyState.Chase;
            }
        }
    }

    private void OnDetectAreaBodyExited(Node2D body)
    {
        if (body is Player)
        {
            TargetPlayer = null;
            CurrentState = EnemyState.Patrol;
        }
    }

    private void OnHitAreaBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            player.TakeDamage(AttackDamage);
        }
    }
}
