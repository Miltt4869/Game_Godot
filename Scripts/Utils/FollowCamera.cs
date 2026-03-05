using Godot;

/// <summary>
/// Camera that follows the player smoothly.
/// Attach to a Camera2D node that is a child of the Level.
/// </summary>
public partial class FollowCamera : Camera2D
{
    [Export] public float SmoothSpeed = 5.0f;
    [Export] public Vector2 FollowOffset = new Vector2(0, -50);
    [Export] public float MinX = 0;
    [Export] public float MaxX = 3000;
    [Export] public float MinY = 0;
    [Export] public float MaxY = 648;

    private Node2D _target;

    public override void _Ready()
    {
        // Make this the current camera
        MakeCurrent();
        AddToGroup("MainCamera");

        // Khóa hẳn giới hạn hiển thị của Camera để không quay lố ra vùng không có Map (màu xám)
        LimitLeft = (int)MinX;
        LimitRight = (int)MaxX;
        LimitTop = (int)MinY;
        LimitBottom = (int)MaxY;
    }

    public override void _Process(double delta)
    {
        if (_target == null || _target.IsQueuedForDeletion())
        {
            // Find player
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player is Node2D p)
            {
                _target = p;
            }
            return;
        }

        // Smooth follow
        Vector2 targetPos = _target.GlobalPosition + FollowOffset;

        // Clamp to level bounds
        targetPos.X = Mathf.Clamp(targetPos.X, MinX, MaxX);
        targetPos.Y = Mathf.Clamp(targetPos.Y, MinY, MaxY);

        GlobalPosition = GlobalPosition.Lerp(targetPos, SmoothSpeed * (float)delta);
    }
}
