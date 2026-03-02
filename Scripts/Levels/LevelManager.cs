using Godot;

public partial class LevelManager : Node2D
{
    [Export] public int LevelNumber = 1;
    [Export] public PackedScene PlayerScene;

    private Node2D _spawnPoint;
    private HUD _hud;

    public override void _Ready()
    {
        // Set current level
        GameManager.Instance.CurrentLevel = LevelNumber;

        // Find spawn point
        if (HasNode("SpawnPoint"))
        {
            _spawnPoint = GetNode<Node2D>("SpawnPoint");
        }

        // Instantiate player at spawn point
        if (PlayerScene != null && _spawnPoint != null)
        {
            var player = PlayerScene.Instantiate<Player>();
            player.GlobalPosition = _spawnPoint.GlobalPosition;
            player.AddToGroup("player");
            AddChild(player);
        }
        else if (HasNode("Player"))
        {
            // Player already in scene
            var player = GetNode<Player>("Player");
            player.AddToGroup("player");
        }
    }
}
