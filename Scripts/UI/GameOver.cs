using Godot;

public partial class GameOver : Control
{
    public override void _Ready()
    {
        var retryButton = GetNode<Button>("VBoxContainer/RetryButton");
        var menuButton = GetNode<Button>("VBoxContainer/MenuButton");

        retryButton.Pressed += OnRetryPressed;
        menuButton.Pressed += OnMenuPressed;

        // Show final score
        var scoreLabel = GetNode<Label>("VBoxContainer/ScoreLabel");
        scoreLabel.Text = $"Điểm: {GameManager.Instance.Score}";
    }

    private void OnRetryPressed()
    {
        GameManager.Instance.RestartLevel();
    }

    private void OnMenuPressed()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
