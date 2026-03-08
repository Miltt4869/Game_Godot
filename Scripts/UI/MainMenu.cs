using Godot;

/// <summary>
/// Màn Hình Chính của Game
/// Được nạp khi mới mở game, hoặc khi người chơi chọn "Về Menu" từ các màn khác
/// </summary>
public partial class MainMenu : Control
{
    public override void _Ready()
    {
        // Liên kết node nút bấm (không cần qua VBoxContainer nữa)
        var playButton = GetNode<Button>("PlayButton");
        var quitButton = GetNode<Button>("QuitButton");
        var titleLabel = GetNode<Label>("TitleLabel");

        // Gắn sự kiện chuyển cảnh
        playButton.Pressed += OnPlayPressed;
        quitButton.Pressed += OnQuitPressed;

        // Mặc định trò chơi bật lên sẽ Focus nút Play 
        playButton.GrabFocus();

        // [Tùy chọn] Animation cho TitleLabel (trôi từ trên xuống nhẹ)
        titleLabel.Modulate = new Color(1, 1, 1, 0);
        titleLabel.Position += new Vector2(0, -30);
        var tw = CreateTween();
        tw.SetParallel(true); // Chạy 2 tween cùng lúc
        tw.TweenProperty(titleLabel, "modulate:a", 1.0f, 0.8f).SetTrans(Tween.TransitionType.Quad);
        tw.TweenProperty(titleLabel, "position:y", titleLabel.Position.Y + 30, 0.8f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
    }

    private void OnPlayPressed()
    {
        // Khi nhấn Bắt đầu chơi: Cần chạy Intro trước khi vào Level 1
        GameManager.Instance.StartIntro();
    }

    private void OnQuitPressed()
    {
        // Thoát khỏi trò chơi xuống màn hình desktop
        GetTree().Quit();
    }
}
