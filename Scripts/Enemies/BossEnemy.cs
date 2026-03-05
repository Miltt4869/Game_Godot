using Godot;

public partial class BossEnemy : BaseEnemy
{
    [Export] public PackedScene KeyScene;

    public override void _Ready()
    {
        // Stats của Đại Boss
        MaxHealth = 300;
        AttackDamage = 25;
        MoveSpeed = 100.0f;
        ScoreValue = 2000;
        DetectRange = 500.0f;
        AttackRange = 80.0f;

        base._Ready();
        
        // Scale to make it look big
        Scale = new Vector2(2.5f, 2.5f);
        Modulate = new Color(0.8f, 0.2f, 0.2f); // Màu đỏ hung tợn
    }

    protected override void Die()
    {
        base.Die();
        SpawnKey();
    }

    private void SpawnKey()
    {
        GD.Print("Đại Boss gục ngã! Lối thoát đã mở.");
        
        // Kích hoạt cổng thoát (nếu có trong scene)
        var exit = GetTree().GetFirstNodeInGroup("LevelExit") as LevelExit;
        if (exit != null)
        {
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
        }
    }
}
