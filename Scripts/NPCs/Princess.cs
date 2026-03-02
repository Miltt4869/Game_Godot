using Godot;
using System.Collections.Generic;

public partial class Princess : Area2D
{
    [Export] public bool RequireAllEnemiesDefeated = true;

    private AnimatedSprite2D _animSprite;
    private Label _messageLabel;
    private bool _isRescued = false;

    [Signal] public delegate void PrincessRescuedEventHandler();

    public override void _Ready()
    {
        _animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        // Auto-create sprites if none assigned
        if (_animSprite.SpriteFrames == null)
        {
            CreatePlaceholderSprites();
        }

        // Create message label
        _messageLabel = new Label();
        _messageLabel.Text = "Hãy đánh bại hết quái vật!";
        _messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _messageLabel.Position = new Vector2(-100, -70);
        _messageLabel.Visible = false;
        _messageLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        _messageLabel.AddThemeFontSizeOverride("font_size", 14);
        AddChild(_messageLabel);

        BodyEntered += OnBodyEntered;
        _animSprite.Play("idle");
    }

    private void CreatePlaceholderSprites()
    {
        _animSprite.SpriteFrames = SpriteHelper.CreatePrincessSpriteFrames();
        _animSprite.Play("idle");
    }

    public override void _Process(double delta)
    {
        if (!_isRescued)
        {
            _animSprite.Play("idle");
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isRescued) return;
        if (body is not Player) return;

        if (RequireAllEnemiesDefeated)
        {
            // Check if all enemies are defeated
            var enemies = GetTree().GetNodesInGroup("enemies");
            if (enemies.Count > 0)
            {
                // Show message
                _messageLabel.Visible = true;
                var tween = CreateTween();
                tween.TweenInterval(2.0);
                tween.TweenCallback(Callable.From(() => { _messageLabel.Visible = false; }));
                return;
            }
        }

        RescuePrincess();
    }

    private void RescuePrincess()
    {
        _isRescued = true;
        _animSprite.Play("rescued");
        _messageLabel.Text = "Cảm ơn Thạch Sanh! ❤️";
        _messageLabel.Visible = true;

        EmitSignal(SignalName.PrincessRescued);

        // Transition to next level after delay
        var timer = GetTree().CreateTimer(3.0);
        timer.Timeout += () =>
        {
            GameManager.Instance.AddScore(500);
            GameManager.Instance.NextLevel();
        };
    }
}
