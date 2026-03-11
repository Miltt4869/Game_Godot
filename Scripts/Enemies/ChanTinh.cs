using Godot;
using System;

public partial class ChanTinh : BaseEnemy
{
    [Export] public PackedScene KeyScene;
    
    private string[] _specialAttacks = { 
        "attack_melee", "attack_smash", "attack_spin", 
        "attack_fire", "attack_jump", "attack_lightning" 
    };

    private Random _random = new Random();
    private bool _isPerformingAttack = false;

    public override void _Ready()
    {
        // Stats của Chăn Tinh - Một con Boss thực thụ
        MaxHealth = 500;
        AttackDamage = 20;
        MoveSpeed = 50.0f; // Boss to lớn di chuyển CỰC CHẬM để uy lực
        ScoreValue = 5000;
        DetectRange = 800.0f;
        AttackRange = 85.0f; 
        AttackCooldown = 3.0f; // Chờ lâu hơn nữa

        base._Ready();

        // Tải SpriteFrames đa dạng cho Chăn Tinh
        AnimSprite.SpriteFrames = SpriteHelper.CreateChanTinhSpriteFrames();
        AnimSprite.Offset = new Vector2(0, -250f);
        AnimSprite.Play("idle");

        Scale = new Vector2(1.3f, 1.3f);
        ZIndex = 4000;
        
        if (_healthBarNode != null)
        {
            _healthBarNode.Scale = new Vector2(4.5f, 1.8f);
            (_healthBarNode as Node2D).ZIndex = 4005;
        }

        // Khởi tạo vùng va chạm một lần duy nhất (Tránh lỗi vật lý do cập nhật liên tục)
        UpdateCollisionShapes();

        GD.Print("[ChanTinh] Boss đã được làm chậm và ổn định vật lý.");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead) { base._PhysicsProcess(delta); return; }

        // Nếu đang thực hiện chiêu thì đứng yên
        if (_isPerformingAttack)
        {
            Velocity = new Vector2(0, Velocity.Y);
            if (!IsOnFloor()) Velocity += new Vector2(0, Gravity * (float)delta);
            MoveAndSlide();
            return;
        }

        // Logic State Machine thông minh
        if (TargetPlayer != null && IsInstanceValid(TargetPlayer) && !TargetPlayer.IsQueuedForDeletion())
        {
            float dist = GlobalPosition.X - TargetPlayer.GlobalPosition.X;
            float absDist = Math.Abs(dist);
            float dirToPlayer = dist > 0 ? -1f : 1f;

            if (absDist <= AttackRange)
            {
                Velocity = new Vector2(0, Velocity.Y);
                if (CanAttackPlayer)
                {
                    PerformRandomAttack();
                }
                else if (AnimSprite.Animation.ToString() != "attack_")
                {
                    if (AnimSprite.Animation.ToString() != "idle") AnimSprite.Play("idle");
                }
            }
            else
            {
                // Đuổi theo chậm rãi
                Velocity = new Vector2(dirToPlayer * MoveSpeed, Velocity.Y);
                if (AnimSprite.Animation.ToString() != "run" && !_isPerformingAttack) 
                    AnimSprite.Play("run");
                SetFacingDirection(dirToPlayer < 0);
            }
        }
        else
        {
            Velocity = new Vector2(0, Velocity.Y);
            if (AnimSprite.Animation.ToString() != "idle") AnimSprite.Play("idle");
            
            // Tìm Player định kỳ
            if (GD.Randi() % 30 == 0)
            {
                var players = GetTree().GetNodesInGroup("player");
                if (players.Count > 0) TargetPlayer = players[0] as Player;
            }
        }

        if (!IsOnFloor()) Velocity += new Vector2(0, Gravity * (float)delta);
        MoveAndSlide();
    }

    private void UpdateCollisionShapes()
    {
        // Cập nhật DetectArea (Tầm nhìn của Boss)
        var detectNode = GetNodeOrNull<CollisionShape2D>("DetectArea/CollisionShape2D");
        if (detectNode != null && detectNode.Shape is RectangleShape2D)
        {
            // Quan trọng: Phải Duplicate để không ảnh hưởng đến các Shape khác dùng chung Resource
            var newShape = (RectangleShape2D)detectNode.Shape.Duplicate();
            newShape.Size = new Vector2(DetectRange * 2, 300);
            detectNode.Shape = newShape;
            GD.Print($"[ChanTinh] Vùng nhận diện độc lập: {newShape.Size}");
        }

        // Cập nhật HitArea (Tầm đánh của Boss)
        var hitNode = GetNodeOrNull<CollisionShape2D>("HitArea/CollisionShape2D");
        if (hitNode != null && hitNode.Shape is RectangleShape2D)
        {
            var newShape = (RectangleShape2D)hitNode.Shape.Duplicate();
            newShape.Size = new Vector2(AttackRange * 2 + 40, 200);
            hitNode.Shape = newShape;
            GD.Print($"[ChanTinh] Vùng tấn công độc lập: {newShape.Size}");
        }

        // Giữ nguyên vùng va chạm vật lý của Boss nhỏ gọn (60x180) để Player áp sát được
        var bodyNode = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (bodyNode != null && bodyNode.Shape is RectangleShape2D)
        {
            var newShape = (RectangleShape2D)bodyNode.Shape.Duplicate();
            newShape.Size = new Vector2(60, 180);
            bodyNode.Shape = newShape;
        }
    }

    private void PerformRandomAttack()
    {
        _isPerformingAttack = true;
        CanAttackPlayer = false;
        
        // Chọn chiêu ngẫu nhiên
        string anim = _specialAttacks[_random.Next(_specialAttacks.Length)];
        GD.Print($"[ChanTinh] Boss tung chiêu: {anim}");
        
        AnimSprite.Play(anim);
        
        // Quay mặt về phía người chơi khi bắt đầu đánh
        if (TargetPlayer != null)
        {
            SetFacingDirection(TargetPlayer.GlobalPosition.X < GlobalPosition.X);
        }

        // Tạo hiệu ứng VFX rực rỡ hơn cho Boss
        CreateAttackVFX();
        if (anim == "attack_smash" || anim == "attack_spin") CreateAttackVFX(); // Nhân đôi VFX cho chiêu to
    }

    protected override void OnAnimationFinished()
    {
        // Nếu vừa đánh xong một chiêu đặc biệt
        if (_isPerformingAttack && AnimSprite.Animation.ToString().StartsWith("attack_"))
        {
            _isPerformingAttack = false;
            CurrentState = EnemyState.Chase;
            AttackCooldownTimer.Start();
            GD.Print("[ChanTinh] Triển khai chiêu thức hoàn tất. Chuyển sang Cooldown.");
        }
        else
        {
            base.OnAnimationFinished();
        }
    }

    protected override void Die()
    {
        base.Die();
        SpawnKey();
    }

    private void SpawnKey()
    {
        GD.Print("[ChanTinh] Chăn Tinh đã gục ngã! Đang tạo Chìa khóa Boss...");
        
        // Kích hoạt cổng thoát (nếu có trong scene)
        var exit = GetTree().GetFirstNodeInGroup("LevelExit") as LevelExit;
        if (exit != null)
        {
            GD.Print("[ChanTinh] Cổng thoát đã được kích hoạt!");
            exit.Activate();
        }

        // Load key scene nếu chưa gán
        if (KeyScene == null)
        {
            KeyScene = GD.Load<PackedScene>("res://Scenes/Items/BossKey.tscn");
        }

        if (KeyScene != null)
        {
            var key = KeyScene.Instantiate<Node2D>();
            key.GlobalPosition = GlobalPosition;
            GetParent().AddChild(key);
            
            // Hiệu ứng nảy chìa khóa
            var tween = key.CreateTween();
            tween.TweenProperty(key, "position:y", key.Position.Y - 50, 0.5f).SetTrans(Tween.TransitionType.Back);
            GD.Print("[ChanTinh] Chìa khóa Boss đã xuất hiện!");
        }
    }
}
