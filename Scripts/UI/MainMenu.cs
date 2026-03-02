using Godot;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        var playButton = GetNode<Button>("VBoxContainer/PlayButton");
        var quitButton = GetNode<Button>("VBoxContainer/QuitButton");

        playButton.Pressed += OnPlayPressed;
        quitButton.Pressed += OnQuitPressed;
    }

    private void OnPlayPressed()
    {
        GameManager.Instance.StartGame();
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
