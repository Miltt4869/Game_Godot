using Godot;

/// <summary>
/// Vùng chết - đặt dưới đáy hố sâu.
/// Khi Player rơi vào → chết ngay lập tức.
/// </summary>
public partial class KillZone : Area2D
{
    public override void _Ready()
    {
        // Kết nối signal khi body vào vùng
        BodyEntered += OnBodyEntered;

        // Đảm bảo collision mask bao gồm Player (layer 1)
        CollisionLayer = 0;
        CollisionMask = 1; // Detect Player
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            // Giết player ngay lập tức
            player.TakeDamage(9999);
        }
    }
}
