using Godot;
using System.Collections.Generic;
using System;

public partial class TreasureChest : Area2D
{
    [Export] public bool RequireAllEnemiesDefeated = true;

    private AnimatedSprite2D _animSprite;
    private Label _messageLabel;
    private bool _isOpened = false;

    // Các thành phần đồ họa để làm hiệu ứng
    private ColorRect _portal;
    private ColorRect _keySprite;

    public override void _Ready()
    {
        _animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (_animSprite.SpriteFrames == null)
        {
            CreatePlaceholderSprites();
        }

        _messageLabel = new Label();
        _messageLabel.Text = "Hãy đánh bại hết quái vật để mở Rương!";
        _messageLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _messageLabel.Position = new Vector2(-120, -70);
        _messageLabel.Visible = false;
        _messageLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        _messageLabel.AddThemeFontSizeOverride("font_size", 14);
        AddChild(_messageLabel);

        BodyEntered += OnBodyEntered;
        _animSprite.Play("idle"); // Lão Hạc => Rương đóng
    }

    private void CreatePlaceholderSprites()
    {
        // Rương đóng (vàng đậm)
        var chestClosed = SpriteHelper.CreateColoredRect(40, 30, new Color(0.6f, 0.4f, 0.1f));
        // Rương mở (vàng sáng chói)
        var chestOpened = SpriteHelper.CreateColoredRect(40, 30, new Color(0.9f, 0.8f, 0.2f));

        var animations = new Dictionary<string, Texture2D[]>
        {
            { "idle", new Texture2D[] { chestClosed } },
            { "rescued", new Texture2D[] { chestOpened } }
        };
        _animSprite.SpriteFrames = SpriteHelper.BuildSpriteFrames(animations);
        _animSprite.Play("idle");
    }

    private void OnBodyEntered(Node2D body)
    {
        if (_isOpened) return;
        if (body is not Player player) return;

        if (RequireAllEnemiesDefeated)
        {
            var enemies = GetTree().GetNodesInGroup("enemies");
            if (enemies.Count > 0)
            {
                _messageLabel.Visible = true;
                var tweenHint = CreateTween();
                tweenHint.TweenInterval(2.0);
                tweenHint.TweenCallback(Callable.From(() => { _messageLabel.Visible = false; }));
                return;
            }
        }

        OpenChest(player);
    }

    private void OpenChest(Player player)
    {
        _isOpened = true;
        _animSprite.Play("rescued");
        
        GameManager.Instance.AddScore(500);

        // 1. Sinh ra Chìa khóa nảy lên xíu
        _keySprite = new ColorRect();
        _keySprite.Color = Colors.Gold; // Chìa khóa vàng
        _keySprite.Size = new Vector2(10, 20);
        _keySprite.Position = new Vector2(-5, -30); // Giữa rương
        AddChild(_keySprite);

        var tween = CreateTween();
        // Nhảy lên mượt mà
        tween.TweenProperty(_keySprite, "position:y", -70f, 0.5f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.TweenInterval(0.5f);
        // Biến mất
        tween.TweenProperty(_keySprite, "modulate:a", 0.0f, 0.5f);
        
        // 2. Mở Cổng Không Gian Huyền Ảo bên cạnh rương
        tween.TweenCallback(Callable.From(() => {
            _portal = new ColorRect();
            _portal.Color = new Color(0.5f, 0.1f, 0.8f, 0f); // Tím ma thuật, ban đầu trong suốt
            _portal.Size = new Vector2(60, 100);
            _portal.Position = new Vector2(50, -100); // Mở bên phải rương
            
            // Xoay tâm giữa
            _portal.PivotOffset = new Vector2(30, 50);
            AddChild(_portal);

            var portalTween = CreateTween();
            portalTween.SetParallel(true);
            portalTween.TweenProperty(_portal, "modulate:a", 1.0f, 1.0f);
            
            // Hiệu ứng phập phồng ánh sáng
            var pulseTween = CreateTween().SetLoops();
            pulseTween.TweenProperty(_portal, "scale", new Vector2(1.1f, 1.1f), 0.5f);
            pulseTween.TweenProperty(_portal, "scale", new Vector2(0.9f, 0.9f), 0.5f);

            // 3. Ép Nhân Vật tự đi vào cổng
            var walkTween = CreateTween();
            walkTween.TweenInterval(1.0f); // Đợi cổng mở đàng hoàng
            walkTween.TweenCallback(Callable.From(() => {
                // Tận dụng hàm đi vào hang có sẵn của Thạch Sanh (1.0f là đi bộ quay mặt qua phải vào cổng)
                player.WalkIntoCave(1.0f); 
            }));
            
            // 4. Qua màn sau 2 giây hút vào
            walkTween.TweenInterval(2.0f);
            walkTween.TweenCallback(Callable.From(() => {
                GameManager.Instance.NextLevel();
            }));
        }));
    }
}
