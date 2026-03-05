using Godot;

/// <summary>
/// Hiệu ứng Fade In/Out khi chuyển cảnh.
/// Đặt như CanvasLayer với ColorRect full màn hình.
/// </summary>
public partial class FadeTransition : CanvasLayer
{
    private ColorRect _fadeRect;
    private bool _isFading = false;

    [Signal] public delegate void FadeCompletedEventHandler();

    public override void _Ready()
    {
        // Layer cao nhất để phủ lên tất cả
        Layer = 100;

        // Tạo ColorRect full screen màu đen
        _fadeRect = new ColorRect();
        _fadeRect.Color = new Color(0, 0, 0, 0); // Bắt đầu trong suốt
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        AddChild(_fadeRect);
    }

    /// <summary>
    /// Fade out (tối dần) → chuyển scene → Fade in (sáng dần)
    /// </summary>
    public void TransitionToScene(string scenePath, float duration = 0.8f)
    {
        if (_isFading) return;
        _isFading = true;

        // Fade out - màn hình tối dần
        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            // Chuyển scene khi đã tối hoàn toàn
            GetTree().ChangeSceneToFile(scenePath);
        }));
        tween.TweenInterval(0.3f); // Đợi scene load
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            _isFading = false;
            EmitSignal(SignalName.FadeCompleted);
        }));
    }

    /// <summary>
    /// Chỉ Fade out (tối dần), không chuyển scene
    /// </summary>
    public void FadeOut(float duration = 0.8f)
    {
        if (_isFading) return;
        _isFading = true;

        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            _isFading = false;
            EmitSignal(SignalName.FadeCompleted);
        }));
    }

    /// <summary>
    /// Chỉ Fade in (sáng dần), dùng khi bắt đầu level mới
    /// </summary>
    public void FadeIn(float duration = 0.8f)
    {
        _fadeRect.Color = new Color(0, 0, 0, 1); // Bắt đầu tối
        _isFading = true;

        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            _isFading = false;
            EmitSignal(SignalName.FadeCompleted);
        }));
    }
}
