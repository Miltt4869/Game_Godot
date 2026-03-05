using Godot;
using System.Collections.Generic;

public partial class Snake : BaseEnemy
{
    public override void _Ready()
    {
        // Set snake-specific stats before base _Ready
        MaxHealth = 40;
        AttackDamage = 15;
        MoveSpeed = 70.0f;
        ScoreValue = 100;
        PatrolDistance = 80.0f; // Vùng tuần tra vừa đủ để nó bò qua lại tự nhiên
        DetectRange = 200.0f;
        AttackRange = 50.0f;
        
        // Đẩy Thanh Máu vọt thẳng lên trời (-90 pixel) để vượt mảng Sprite của đầu Rắn
        HealthBarOffset = new Vector2(-20, -85);

        base._Ready();
    }

    protected override void CreatePlaceholderSprites()
    {
        AnimSprite.SpriteFrames = SpriteHelper.CreateSnakeSpriteFrames();
        AnimSprite.Play("walk");
    }
}
