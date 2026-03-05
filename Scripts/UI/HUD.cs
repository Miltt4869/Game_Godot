using Godot;

public partial class HUD : CanvasLayer
{
    private ProgressBar _healthBar;
    private Label _scoreLabel;
    private Label _levelLabel;
    private Panel _pausePanel;
    private Player _player;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("MarginContainer/HBoxContainer/HealthBar");
        _scoreLabel = GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel");
        _levelLabel = GetNode<Label>("MarginContainer/HBoxContainer/LevelLabel");

        // Setup pause panel
        if (HasNode("PausePanel"))
        {
            _pausePanel = GetNode<Panel>("PausePanel");
            _pausePanel.Visible = false;
        }

        UpdateUI();
    }

    public override void _Process(double delta)
    {
        // Find player if not found
        if (_player == null)
        {
            var playerNode = GetTree().GetFirstNodeInGroup("player");
            if (playerNode is Player p)
            {
                _player = p;
                _player.HealthChanged += OnHealthChanged;
            }
        }

        UpdateUI();

        if (_pausePanel != null)
        {
            _pausePanel.Visible = GameManager.Instance.IsPaused;
        }
    }

    private void UpdateUI()
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = GameManager.Instance.MaxPlayerHealth;
            _healthBar.Value = GameManager.Instance.PlayerHealth;
        }

        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Điểm: {GameManager.Instance.Score}";
        }

        if (_levelLabel != null)
        {
            string levelName = GameManager.Instance.CurrentLevel switch
            {
                1 => "Đường Rừng Hiểm Trở",
                2 => "Hang Tối Hiểm Nguy",
                3 => "Đại Chiến Chằn Tinh",
                _ => $"Level {GameManager.Instance.CurrentLevel}"
            };
            _levelLabel.Text = levelName;
        }
    }

    private void OnHealthChanged(int newHealth, int maxHealth)
    {
        if (_healthBar != null)
        {
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = newHealth;
        }
    }
}
