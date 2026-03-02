using Godot;
using System;

/// <summary>
/// Attach this script to any StaticBody2D that has a ColorRect child named "Sprite" or "GroundSprite".
/// It will automatically create a matching collision shape based on the ColorRect size.
/// </summary>
public partial class AutoCollision : StaticBody2D
{
    public override void _Ready()
    {
        // Find the ColorRect child
        ColorRect colorRect = null;
        foreach (var child in GetChildren())
        {
            if (child is ColorRect cr)
            {
                colorRect = cr;
                break;
            }
        }

        if (colorRect == null) return;

        // Calculate size from the ColorRect offsets
        float width = colorRect.OffsetRight - colorRect.OffsetLeft;
        float height = colorRect.OffsetBottom - colorRect.OffsetTop;
        float centerX = (colorRect.OffsetLeft + colorRect.OffsetRight) / 2.0f;
        float centerY = (colorRect.OffsetTop + colorRect.OffsetBottom) / 2.0f;

        // Create or update collision shape
        var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collisionShape != null)
        {
            var rectShape = new RectangleShape2D();
            rectShape.Size = new Vector2(width, height);
            collisionShape.Shape = rectShape;
            collisionShape.Position = new Vector2(centerX, centerY);
        }
    }
}
