using Godot;

public partial class WinScreen : Control
{
    public override void _Ready()
    {
        var menuButton = GetNode<Button>("MainContainer/MenuButton");
        menuButton.Pressed += OnMenuPressed;

        var scoreLabel = GetNode<Label>("MainContainer/ScoreLabel");
        scoreLabel.Text = $"Tổng điểm: {GameManager.Instance.Score}";
        
        // Hiệu ứng Title bay lên nhẹ nhàng
        var title = GetNode<Label>("MainContainer/Title");
        title.Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(title, "modulate:a", 1.0f, 1.0f);
        tween.SetParallel();
        title.Position += new Vector2(0, 50);
        tween.TweenProperty(title, "position:y", title.Position.Y - 50, 1.0f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    private void OnMenuPressed()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
