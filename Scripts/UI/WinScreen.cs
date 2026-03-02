using Godot;

public partial class WinScreen : Control
{
    public override void _Ready()
    {
        var menuButton = GetNode<Button>("VBoxContainer/MenuButton");
        menuButton.Pressed += OnMenuPressed;

        var scoreLabel = GetNode<Label>("VBoxContainer/ScoreLabel");
        scoreLabel.Text = $"Tổng điểm: {GameManager.Instance.Score}";
    }

    private void OnMenuPressed()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
