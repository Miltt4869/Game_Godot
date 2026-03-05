using Godot;

public partial class BossKey : Area2D
{
    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 1; // Player
        BodyEntered += OnBodyEntered;

        // Visual placeholder (Vàng óng)
        var sprite = new ColorRect();
        sprite.Color = new Color(1, 0.9f, 0.2f);
        sprite.Size = new Vector2(20, 10);
        sprite.Position = new Vector2(-10, -5);
        AddChild(sprite);

        // Hiệu ứng lấp lánh
        var tween = CreateTween();
        tween.SetLoops();
        tween.TweenProperty(sprite, "modulate:a", 0.5f, 0.5f);
        tween.TweenProperty(sprite, "modulate:a", 1.0f, 0.5f);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            GameManager.Instance.HasBossKey = true;
            GD.Print("Đã nhặt được Chìa Khóa!");
            QueueFree();
        }
    }
}
