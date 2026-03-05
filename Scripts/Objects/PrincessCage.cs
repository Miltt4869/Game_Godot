using Godot;

public partial class PrincessCage : StaticBody2D
{
    private Area2D _interactArea;
    private ColorRect _visual;
    private bool _isOpened = false;

    public override void _Ready()
    {
        // Thân lồng (Vật cản)
        _visual = new ColorRect();
        _visual.Color = new Color(0.4f, 0.4f, 0.45f, 0.8f);
        _visual.Size = new Vector2(80, 100);
        _visual.Position = new Vector2(-40, -100);
        AddChild(_visual);

        // Các thanh sắt
        for (int i = 0; i < 5; i++)
        {
            var bar = new ColorRect();
            bar.Color = new Color(0.2f, 0.2f, 0.25f);
            bar.Size = new Vector2(4, 100);
            bar.Position = new Vector2(-40 + i * 20 - 2, -100);
            AddChild(bar);
        }

        // Vùng tương tác
        _interactArea = new Area2D();
        _interactArea.CollisionLayer = 0;
        _interactArea.CollisionMask = 1;
        _interactArea.BodyEntered += OnPlayerEntered;
        
        var shape = new CollisionShape2D();
        shape.Shape = new CircleShape2D { Radius = 100f };
        _interactArea.AddChild(shape);
        AddChild(_interactArea);

        // Label thông báo
        var label = new Label();
        label.Name = "Hint";
        label.Text = "Cần chìa khóa!";
        label.Position = new Vector2(-50, -130);
        label.Visible = false;
        AddChild(label);
    }

    private void OnPlayerEntered(Node2D body)
    {
        if (_isOpened) return;

        if (body is Player)
        {
            if (GameManager.Instance.HasBossKey)
            {
                OpenCage();
            }
            else
            {
                GetNode<Label>("Hint").Visible = true;
                var timer = GetTree().CreateTimer(2.0);
                timer.Timeout += () => GetNode<Label>("Hint").Visible = false;
            }
        }
    }

    private void OpenCage()
    {
        _isOpened = true;
        GD.Print("Lồng đã mở! Cứu được Công Chúa!");
        
        // Hiệu ứng mở lồng
        var tween = CreateTween();
        tween.TweenProperty(_visual, "modulate:a", 0.0f, 1.5f);
        tween.TweenCallback(Callable.From(() => {
            // Giải phóng va chạm để cứu công chúa
            GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
            _visual.Visible = false;
        }));
        
        // Xóa các thanh sắt (bất cứ ColorRect nào không phải _visual)
        foreach (var child in GetChildren())
        {
            if (child is ColorRect cr && cr != _visual)
            {
                CreateTween().TweenProperty(cr, "position:y", 50f, 1.0f);
            }
        }
    }
}
