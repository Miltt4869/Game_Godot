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
        PatrolDistance = 120.0f;
        DetectRange = 200.0f;
        AttackRange = 60.0f;

        base._Ready();
    }

    protected override void CreatePlaceholderSprites()
    {
        AnimSprite.SpriteFrames = SpriteHelper.CreateSnakeSpriteFrames();
        AnimSprite.Play("walk");
    }
}
