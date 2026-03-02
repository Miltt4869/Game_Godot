using Godot;
using System;

public partial class GameManager : Node
{
    // Singleton
    public static GameManager Instance { get; private set; }

    // Game State
    public int Score { get; set; } = 0;
    public int CurrentLevel { get; set; } = 1;
    public int PlayerHealth { get; set; } = 100;
    public int MaxPlayerHealth { get; set; } = 100;
    public bool IsGameOver { get; set; } = false;
    public bool IsPaused { get; set; } = false;

    // Level paths
    private readonly string[] _levelPaths = {
        "res://Scenes/Levels/Level1.tscn",
        "res://Scenes/Levels/Level2.tscn",
        "res://Scenes/Levels/Level3.tscn"
    };

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void StartGame()
    {
        Score = 0;
        CurrentLevel = 1;
        PlayerHealth = MaxPlayerHealth;
        IsGameOver = false;
        LoadLevel(CurrentLevel);
    }

    public void LoadLevel(int level)
    {
        CurrentLevel = level;
        if (level > _levelPaths.Length)
        {
            // Player won all levels!
            WinGame();
            return;
        }
        GetTree().ChangeSceneToFile(_levelPaths[level - 1]);
    }

    public void NextLevel()
    {
        CurrentLevel++;
        LoadLevel(CurrentLevel);
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void PlayerTakeDamage(int damage)
    {
        PlayerHealth -= damage;
        if (PlayerHealth <= 0)
        {
            PlayerHealth = 0;
            GameOver();
        }
    }

    public void HealPlayer(int amount)
    {
        PlayerHealth = Math.Min(PlayerHealth + amount, MaxPlayerHealth);
    }

    public void GameOver()
    {
        IsGameOver = true;
        GetTree().ChangeSceneToFile("res://Scenes/Main/GameOver.tscn");
    }

    public void WinGame()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Main/WinScreen.tscn");
    }

    public void GoToMainMenu()
    {
        IsGameOver = false;
        Score = 0;
        CurrentLevel = 1;
        PlayerHealth = MaxPlayerHealth;
        GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
    }

    public void RestartLevel()
    {
        PlayerHealth = MaxPlayerHealth;
        IsGameOver = false;
        LoadLevel(CurrentLevel);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        GetTree().Paused = IsPaused;
    }
}
