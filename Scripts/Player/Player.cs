using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D
{
    // Movement - Core
    [Export] public float Speed = 220.0f;
    [Export] public float Acceleration = 1200.0f;      // Tốc độ tăng tốc
    [Export] public float Deceleration = 1000.0f;      // Tốc độ giảm tốc
    [Export] public float AirAcceleration = 600.0f;    // Tăng tốc khi trên không
    [Export] public float AirDeceleration = 400.0f;    // Giảm tốc khi trên không
    
    // Jump - Cải thiện cảm giác nhảy
    [Export] public float JumpVelocity = -380.0f;
    [Export] public float Gravity = 900.0f;
    [Export] public float FallGravityMultiplier = 1.5f;   // Rơi nhanh hơn khi đi xuống
    [Export] public float JumpCutMultiplier = 0.5f;       // Giảm vận tốc khi thả phím nhảy sớm
    [Export] public float CoyoteTime = 0.12f;             // Thời gian cho phép nhảy sau khi rời nền
    [Export] public float JumpBufferTime = 0.1f;          // Thời gian buffer nhấn phím nhảy trước khi chạm đất
    
    // Movement state
    private float _coyoteTimer = 0f;
    private float _jumpBufferTimer = 0f;
    private bool _wasOnFloor = false;
    private bool _isJumping = false;
    private float _facingDirection = 1f;                  // 1 = phải, -1 = trái

    // Combat
    [Export] public int AttackDamage = 25;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public float ComboResetTime = 0.6f;  // Time before combo resets to attack1
    private bool _canAttack = true;
    private bool _isAttacking = false;
    private int _comboIndex = 0;  // 0-3 for 4 attack types
    private float _comboTimer = 0;  // Time since last attack
    private bool _comboActive = false;

    // Health
    private int _health;
    private bool _isDead = false;
    private bool _isHurt = false;

    // Components
    private AnimatedSprite2D _animatedSprite;
    private Area2D _attackArea;
    private CollisionShape2D _attackCollision;
    private Timer _attackCooldownTimer;
    private Timer _hurtTimer;

    // Signals
    [Signal] public delegate void HealthChangedEventHandler(int newHealth, int maxHealth);
    [Signal] public delegate void PlayerDiedEventHandler();

    public override void _Ready()
    {
        _health = GameManager.Instance.PlayerHealth;
        AddToGroup("player");

        // Get nodes
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite.TextureFilter = TextureFilterEnum.Nearest;
        _attackArea = GetNode<Area2D>("AttackArea");
        _attackCollision = _attackArea.GetNode<CollisionShape2D>("CollisionShape2D");
        _attackCollision.Disabled = true;

        // Xóa cache và tạo mới sprites
        SpriteHelper.ClearCache();
        CreatePlaceholderSprites();

        // Create attack cooldown timer
        _attackCooldownTimer = new Timer();
        _attackCooldownTimer.WaitTime = AttackCooldown;
        _attackCooldownTimer.OneShot = true;
        _attackCooldownTimer.Timeout += OnAttackCooldownTimeout;
        AddChild(_attackCooldownTimer);

        // Create hurt timer
        _hurtTimer = new Timer();
        _hurtTimer.WaitTime = 0.5f;
        _hurtTimer.OneShot = true;
        _hurtTimer.Timeout += OnHurtTimeout;
        AddChild(_hurtTimer);

        // Connect attack area signal
        _attackArea.BodyEntered += OnAttackAreaBodyEntered;

        // Connect animation signal
        _animatedSprite.AnimationFinished += OnAnimationFinished;

        EmitSignal(SignalName.HealthChanged, _health, GameManager.Instance.MaxPlayerHealth);
    }

    private void CreatePlaceholderSprites()
    {
        _animatedSprite.SpriteFrames = SpriteHelper.CreatePlayerSpriteFrames();
        _animatedSprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead) return;

        float dt = (float)delta;
        Vector2 velocity = Velocity;
        bool onFloor = IsOnFloor();

        // === COYOTE TIME - Cho phép nhảy muộn sau khi rời nền ===
        if (onFloor)
        {
            _coyoteTimer = CoyoteTime;
            _isJumping = false;
        }
        else
        {
            _coyoteTimer -= dt;
        }

        // === JUMP BUFFER - Ghi nhớ input nhảy ===
        if (Input.IsActionJustPressed("jump"))
        {
            _jumpBufferTimer = JumpBufferTime;
        }
        else
        {
            _jumpBufferTimer -= dt;
        }

        // === GRAVITY - Áp dụng trọng lực với fall multiplier ===
        if (!onFloor)
        {
            float gravityThisFrame = Gravity;
            
            // Rơi nhanh hơn khi đang đi xuống hoặc khi thả phím nhảy sớm
            if (velocity.Y > 0)
            {
                gravityThisFrame *= FallGravityMultiplier;
            }
            else if (velocity.Y < 0 && !Input.IsActionPressed("jump"))
            {
                // Variable jump height - thả phím sớm sẽ nhảy thấp hơn
                velocity.Y += Gravity * JumpCutMultiplier * dt;
            }
            
            velocity.Y += gravityThisFrame * dt;
            
            // Giới hạn tốc độ rơi tối đa
            velocity.Y = Mathf.Min(velocity.Y, 800f);
        }

        // === JUMP - Xử lý nhảy với coyote time và jump buffer ===
        bool canJump = (_coyoteTimer > 0 || onFloor) && !_isAttacking;
        if (_jumpBufferTimer > 0 && canJump)
        {
            velocity.Y = JumpVelocity;
            _jumpBufferTimer = 0;
            _coyoteTimer = 0;
            _isJumping = true;
        }

        // === HORIZONTAL MOVEMENT - Di chuyển ngang với acceleration ===
        float direction = Input.GetAxis("move_left", "move_right");
        
        if (!_isAttacking)
        {
            float currentAccel;
            float currentDecel;
            
            // Khác biệt acceleration trên không và trên mặt đất
            if (onFloor)
            {
                currentAccel = Acceleration;
                currentDecel = Deceleration;
            }
            else
            {
                currentAccel = AirAcceleration;
                currentDecel = AirDeceleration;
            }
            
            if (Mathf.Abs(direction) > 0.1f)
            {
                // Di chuyển - áp dụng acceleration
                float targetSpeed = direction * Speed;
                velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, currentAccel * dt);
                
                // Cập nhật hướng mặt và sprite
                _facingDirection = direction > 0 ? 1f : -1f;
                _animatedSprite.FlipH = direction < 0;

                // Flip attack area theo hướng
                var attackPos = _attackArea.Position;
                attackPos.X = _facingDirection * Math.Abs(attackPos.X);
                _attackArea.Position = attackPos;
            }
            else
            {
                // Dừng lại - áp dụng deceleration
                velocity.X = Mathf.MoveToward(velocity.X, 0, currentDecel * dt);
            }
        }
        else
        {
            // Khi đang tấn công, giảm tốc chậm hơn
            velocity.X = Mathf.MoveToward(velocity.X, 0, Deceleration * 0.3f * dt);
        }

        Velocity = velocity;
        MoveAndSlide();

        // Ghi nhớ trạng thái floor cho frame tiếp theo
        _wasOnFloor = onFloor;

        // Combo timer - reset combo if too much time passes
        if (_comboActive)
        {
            _comboTimer += dt;
            if (_comboTimer >= ComboResetTime)
            {
                _comboIndex = 0;
                _comboActive = false;
            }
        }

        // Attack on click / key press
        if (Input.IsActionJustPressed("attack") && _canAttack && !_isHurt)
        {
            Attack();
        }

        // Update animation
        UpdateAnimation(direction);
    }

    private void UpdateAnimation(float direction)
    {
        if (_isDead) return;
        if (_isAttacking) return;
        if (_isHurt)
        {
            PlayAnimationIfNotPlaying("hurt");
            return;
        }

        if (!IsOnFloor())
        {
            // Phân biệt animation nhảy lên và rơi xuống
            if (Velocity.Y < 0)
            {
                PlayAnimationIfNotPlaying("jump");
            }
            else
            {
                // Có thể dùng animation "fall" nếu có, không thì dùng "jump"
                if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation("fall"))
                {
                    PlayAnimationIfNotPlaying("fall");
                }
                else
                {
                    PlayAnimationIfNotPlaying("jump");
                }
            }
        }
        else if (Math.Abs(Velocity.X) > 10f)  // Dùng velocity thực tế thay vì input
        {
            PlayAnimationIfNotPlaying("run");
        }
        else
        {
            PlayAnimationIfNotPlaying("idle");
        }
    }

    /// <summary>
    /// Chỉ play animation nếu nó khác animation hiện tại - tránh reset animation
    /// </summary>
    private void PlayAnimationIfNotPlaying(string animName)
    {
        if (_animatedSprite.Animation != animName)
        {
            _animatedSprite.Play(animName);
            GD.Print($"Playing animation: {animName}, frames: {_animatedSprite.SpriteFrames?.GetFrameCount(animName)}");
        }
    }

    private void Attack()
    {
        _isAttacking = true;
        _canAttack = false;

        // Play the current combo attack
        string attackAnim = $"attack{_comboIndex + 1}";

        // Check if the animation exists, fallback to "attack" if not
        if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(attackAnim))
        {
            _animatedSprite.Play(attackAnim);
        }
        else
        {
            _animatedSprite.Play("attack");
        }

        _attackCollision.Disabled = false;

        // Attack lasts 0.3 seconds (since each attack is 1 frame, AnimationFinished fires too fast)
        var attackDurationTimer = GetTree().CreateTimer(0.3);
        attackDurationTimer.Timeout += () =>
        {
            _isAttacking = false;
            _attackCollision.Disabled = true;
        };

        // Advance combo
        _comboIndex = (_comboIndex + 1) % 4;
        _comboTimer = 0;
        _comboActive = true;

        _attackCooldownTimer.Start();
    }

    private void OnAnimationFinished()
    {
        // Attack completion is handled by timer in Attack()
        // This is now only used for other one-shot animations
    }

    private void OnAttackCooldownTimeout()
    {
        _canAttack = true;
    }

    private void OnHurtTimeout()
    {
        _isHurt = false;
    }

    private void OnAttackAreaBodyEntered(Node2D body)
    {
        if (body.IsInGroup("enemies"))
        {
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", AttackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead || _isHurt) return;

        _health -= damage;
        _isHurt = true;
        _comboIndex = 0;      // Reset combo when hurt
        _comboActive = false;
        _hurtTimer.Start();
        GameManager.Instance.PlayerHealth = _health;

        EmitSignal(SignalName.HealthChanged, _health, GameManager.Instance.MaxPlayerHealth);

        if (_health <= 0)
        {
            Die();
        }
        else
        {
            _animatedSprite.Play("hurt");
            // Red flash effect
            _animatedSprite.Modulate = new Color(1, 0.3f, 0.3f);
            var tween = CreateTween();
            tween.TweenProperty(_animatedSprite, "modulate", Colors.White, 0.4f);
            // Knockback
            Velocity = new Vector2(_animatedSprite.FlipH ? 200 : -200, -150);
        }
    }

    public void Heal(int amount)
    {
        _health = Math.Min(_health + amount, GameManager.Instance.MaxPlayerHealth);
        GameManager.Instance.PlayerHealth = _health;
        EmitSignal(SignalName.HealthChanged, _health, GameManager.Instance.MaxPlayerHealth);
    }

    private void Die()
    {
        _isDead = true;
        _animatedSprite.Play("die");
        _animatedSprite.Modulate = new Color(0.8f, 0.2f, 0.2f);
        var tween = CreateTween();
        tween.TweenProperty(_animatedSprite, "rotation", Mathf.Pi / 2, 0.8f);
        tween.TweenCallback(Callable.From(() =>
        {
            EmitSignal(SignalName.PlayerDied);
            GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
            var timer = GetTree().CreateTimer(1.0);
            timer.Timeout += () => GameManager.Instance.GameOver();
        }));
    }
}
