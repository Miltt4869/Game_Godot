using Godot;
using System;
using System.Collections.Generic;

public partial class Eagle : BaseEnemy
{
    // Eagle-specific properties
    [Export] public float FlyHeight = 100.0f;
    [Export] public float DiveSpeed = 300.0f;
    [Export] public float FloatAmplitude = 20.0f;
    [Export] public float FloatFrequency = 2.0f;

    private float _floatTimer = 0;
    private float _baseY;
    private bool _isDiving = false;

    public override void _Ready()
    {
        // Set eagle-specific stats before base _Ready
        MaxHealth = 60;
        AttackDamage = 20;
        MoveSpeed = 100.0f;
        ScoreValue = 200;
        PatrolDistance = 200.0f;
        DetectRange = 300.0f;
        AttackRange = 80.0f;
        AttackCooldown = 2.0f;
        
        // Đẩy Thanh Máu vọt thẳng lên trời để vượt mảng Sprite của đầu Đại Bàng
        HealthBarOffset = new Vector2(-20, -75);

        base._Ready();
        _baseY = GlobalPosition.Y;
    }

    protected override void CreatePlaceholderSprites()
    {
        AnimSprite.SpriteFrames = SpriteHelper.CreateEagleSpriteFrames();
        AnimSprite.Play("walk");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead)
        {
            // Fall when dead
            Velocity = new Vector2(0, Velocity.Y + 500 * (float)delta);
            MoveAndSlide();
            return;
        }

        Vector2 velocity = Velocity;

        // Eagle doesn't use gravity - it flies!
        _floatTimer += (float)delta;

        switch (CurrentState)
        {
            case EnemyState.Patrol:
                // Horizontal patrol
                velocity.X = PatrolDirection * MoveSpeed;

                // Floating motion
                float floatY = _baseY + Mathf.Sin(_floatTimer * FloatFrequency) * FloatAmplitude;
                velocity.Y = (floatY - GlobalPosition.Y) * 5.0f;

                // Check for walls (Bounce properly using Normal to avoid sticking)
                if (IsOnWall())
                {
                    float wallNormalX = GetWallNormal().X;
                    if (Math.Abs(wallNormalX) > 0.1f)
                        PatrolDirection = wallNormalX > 0 ? 1 : -1;
                    else
                        PatrolDirection *= -1;
                    velocity.X = PatrolDirection * MoveSpeed;
                }

                // Check patrol bounds
                float distFromStart = GlobalPosition.X - StartPosition.X;
                if (distFromStart >= PatrolDistance && PatrolDirection > 0)
                {
                    PatrolDirection = -1;
                    velocity.X = PatrolDirection * MoveSpeed;
                }
                else if (distFromStart <= -PatrolDistance && PatrolDirection < 0)
                {
                    PatrolDirection = 1;
                    velocity.X = PatrolDirection * MoveSpeed;
                }

                AnimSprite.FlipH = PatrolDirection < 0;
                AnimSprite.Play("walk");
                break;

            case EnemyState.Chase:
                if (TargetPlayer != null && !TargetPlayer.IsQueuedForDeletion())
                {
                    // Hover above the player
                    Vector2 hoverPos = TargetPlayer.GlobalPosition + new Vector2(0, -FlyHeight);
                    Vector2 dirToHover = (hoverPos - GlobalPosition).Normalized();

                    // Move towards hover position
                    velocity = dirToHover * MoveSpeed * 1.5f;

                    // Add floating motion
                    velocity.Y += Mathf.Sin(_floatTimer * FloatFrequency) * FloatAmplitude;

                    float distX = Math.Abs(GlobalPosition.X - TargetPlayer.GlobalPosition.X);
                    if (distX > 5.0f)
                    {
                        AnimSprite.FlipH = (TargetPlayer.GlobalPosition.X - GlobalPosition.X) < 0;
                    }

                    // Start dive if horizontally close
                    if (distX <= AttackRange && CanAttackPlayer)
                    {
                        CurrentState = EnemyState.Attack;
                        _isDiving = true;
                    }
                }
                else
                {
                    CurrentState = EnemyState.Patrol;
                }
                AnimSprite.Play("walk");
                break;

            case EnemyState.Attack:
                if (_isDiving && TargetPlayer != null)
                {
                    // Dive attack directly at player
                    Vector2 diveDir = (TargetPlayer.GlobalPosition - GlobalPosition).Normalized();
                    velocity = diveDir * DiveSpeed;
                    AnimSprite.Play("attack");
                    
                    float distX = Math.Abs(GlobalPosition.X - TargetPlayer.GlobalPosition.X);
                    if (distX > 5.0f) 
                        AnimSprite.FlipH = diveDir.X < 0;

                    // If eagle has reached player's height or lower, pull up
                    if (GlobalPosition.Y >= TargetPlayer.GlobalPosition.Y - 20 || IsOnFloor())
                    {
                        _isDiving = false;
                        CanAttackPlayer = false;
                        AttackCooldownTimer.Start();
                    }
                }
                else
                {
                    // Fly upwards to escape
                    Vector2 targetPos = TargetPlayer != null ? TargetPlayer.GlobalPosition : GlobalPosition;
                    Vector2 escapePos = targetPos + new Vector2(0, -FlyHeight - 50);
                    Vector2 escapeDir = (escapePos - GlobalPosition).Normalized();

                    velocity = escapeDir * DiveSpeed * 0.8f;
                    AnimSprite.Play("walk");

                    // Nhìn về phía người chơi ngay cả khi đang rút lui bay lên
                    if (TargetPlayer != null)
                    {
                        float distX = Math.Abs(GlobalPosition.X - TargetPlayer.GlobalPosition.X);
                        if (distX > 5.0f) 
                            AnimSprite.FlipH = (TargetPlayer.GlobalPosition.X - GlobalPosition.X) < 0;
                    }

                    // Once high enough, go back to Chase
                    if (TargetPlayer != null && GlobalPosition.Y <= targetPos.Y - FlyHeight + 20)
                    {
                        CurrentState = EnemyState.Chase;
                    }
                    else if (TargetPlayer == null && GlobalPosition.Y <= _baseY)
                    {
                        CurrentState = EnemyState.Patrol;
                    }
                }
                break;

            case EnemyState.Hurt:
                velocity.X = 0;
                velocity.Y = (_baseY - GlobalPosition.Y) * 2.0f;
                AnimSprite.Play("hurt");
                break;

            case EnemyState.Dead:
                velocity.X = 0;
                velocity.Y += 500 * (float)delta; // Fall when dead
                break;
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
