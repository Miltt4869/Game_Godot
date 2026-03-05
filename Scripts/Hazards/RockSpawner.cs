using Godot;

/// <summary>
/// Spawner tự động tạo đá lăn theo timer.
/// Đặt ở điểm cao, đá sẽ rơi xuống và lăn theo hướng đã đặt.
/// </summary>
public partial class RockSpawner : Node2D
{
    [Export] public PackedScene RockScene;
    [Export] public float SpawnInterval = 4.0f; // Giây giữa mỗi lượt spawn
    [Export] public float RockDirection = -1f;  // -1 = lăn trái, 1 = lăn phải
    [Export] public bool ActiveOnStart = true;

    private Timer _spawnTimer;
    private bool _active = false;

    public override void _Ready()
    {
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = SpawnInterval;
        _spawnTimer.OneShot = false;
        _spawnTimer.Timeout += SpawnRock;
        AddChild(_spawnTimer);

        if (ActiveOnStart)
        {
            // Delay nhỏ trước khi bắt đầu spawn
            var startDelay = GetTree().CreateTimer(1.0);
            startDelay.Timeout += () =>
            {
                _active = true;
                _spawnTimer.Start();
                SpawnRock(); // Spawn ngay lần đầu
            };
        }
    }

    public void StartSpawning()
    {
        _active = true;
        _spawnTimer.Start();
    }

    public void StopSpawning()
    {
        _active = false;
        _spawnTimer.Stop();
    }

    private void SpawnRock()
    {
        if (!_active || RockScene == null) return;

        // Kiểm tra nếu player đang gần spawner (trong khoảng 600px)
        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (player == null) return;

        float distToPlayer = Mathf.Abs(player.GlobalPosition.X - GlobalPosition.X);
        if (distToPlayer > 800f) return; // Không spawn nếu player quá xa

        var rock = RockScene.Instantiate<RollingRock>();
        rock.GlobalPosition = GlobalPosition;
        rock.SetDirection(RockDirection);

        // Thêm vào scene cha
        GetParent().AddChild(rock);
    }
}
