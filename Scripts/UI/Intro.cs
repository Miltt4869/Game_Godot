using Godot;
using System;
using System.Collections.Generic;

public partial class Intro : Control
{
    private Label _storyLabel;
    private Sprite2D _eagle;
    private Sprite2D _thachSanh;

    private int _currentLine = 0;
    private List<string> _storyLines = new List<string>
    {
        "Ngày xửa ngày xưa, tại vùng đất xa xôi,\ncó một chàng trai tiều phu hiền lành tên là Thạch Sanh...",
        "Chàng sống dưới gốc đa già, tuy nghèo khó\nnhưng lại sở hữu sức mạnh phi thường và lòng quả cảm.",
        "Một ngày nọ, một con Đại bàng tinh khổng lồ độc ác\nbất ngờ hiện ra từ mây đen...",
        "Nó đã bắt cóc Công chúa xinh đẹp\nvà mang nàng vào sâu trong hang đá tối tăm.",
        "Toàn vương quốc chìm trong u tối và sợ hãi,\nnhưng Thạch Sanh đã không hề do dự...",
        "Với chiếc rìu thần trên tay,\nchàng bước vào rừng sâu để giải cứu Công chúa!"
    };

    private float _timer = 0;
    private float _targetProgress = 0;
    private float _smoothProgress = 0;

    public override void _Ready()
    {
        _storyLabel = GetNode<Label>("StoryLabel");
        _eagle = GetNode<Sprite2D>("CharacterContainer/Eagle");
        _thachSanh = GetNode<Sprite2D>("CharacterContainer/ThachSanh");

        // Cả 2 cùng nhìn về bên phải
        _eagle.FlipH = true;
        _thachSanh.FlipH = false;

        ShowNextLine();
    }

    private float _totalDuration = 32f; // Tổng thời gian Intro chạy (xấp xỉ 5 câu x 6s)
    private float _introTimer = 0f;

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _timer += dt;
        _introTimer += dt;

        // 🟢 DI CHUYỂN SIÊU MƯỢT (LINEAR PROGRESS)
        // Không phụ thuộc vào dòng chữ, nhân vật trôi liên tục từ trái qua phải
        _smoothProgress = Mathf.Clamp(_introTimer / _totalDuration, 0f, 1f);

        // 🟢 HOẠNH ẢNH (FRAMES)
        _eagle.Frame = (int)(_timer * 11.0f) % 2;
        _thachSanh.Frame = (int)(_timer * 12.0f) % 3;

        // 🟢 VỊ TRÍ
        float screenW = GetViewportRect().Size.X;
        float startX = -150.0f; // Bắt đầu từ ngoài màn hình bên trái
        float endX = screenW + 150.0f; // Đi ra ngoài màn hình bên phải

        float currentX = Mathf.Lerp(startX, endX, _smoothProgress);

        // Hiệu ứng nhấp nhô
        float eagleLead = 280.0f + (Mathf.Sin(_timer * 3.0f) * 40.0f);
        float eagleBob = Mathf.Sin(_timer * 5.5f) * 18.0f;
        float thachSanhBob = Mathf.Abs(Mathf.Sin(_timer * 13.0f)) * -11.0f;

        _eagle.Position = new Vector2(currentX + eagleLead, eagleBob - 60);
        _thachSanh.Position = new Vector2(currentX, thachSanhBob);

        // Tự động vào game khi hết thời gian
        if (_introTimer >= _totalDuration)
        {
            StartGame();
        }
    }

    // Xóa bỏ Input để Intro tự chạy hoàn toàn
    public override void _Input(InputEvent @event) { }

    private void ShowNextLine()
    {
        if (_currentLine >= _storyLines.Count) return;

        string text = _storyLines[_currentLine];
        _currentLine++;

        var tw = CreateTween();
        tw.TweenProperty(_storyLabel, "modulate:a", 0.0f, 0.4f);
        tw.TweenCallback(Callable.From(() => _storyLabel.Text = text));
        tw.TweenProperty(_storyLabel, "modulate:a", 1.0f, 0.6f);

        tw.TweenInterval(6.0f);
        tw.TweenCallback(Callable.From(() => ShowNextLine()));
    }

    private void StartGame()
    {
        // Chống gọi đè nhiều lần
        SetProcess(false);
        GameManager.Instance.StartGame();
    }
}
