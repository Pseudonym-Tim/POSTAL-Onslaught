using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles core game related stuff and setup...
/// </summary>
public class GameManager : Singleton<GameManager>
{
    private const float GAME_TIMESCALE = 1.0f;
    public const string GAME_NAME = "POSTAL: Onslaught";

    public static GlobalStatistics GlobalStats { get; set; } = new GlobalStatistics();
    public static GameState CurrentGameState { get; set; } = GameState.INACTIVE;
    public static float InGameTimer { get; private set; } = 0.0f;
    public static ScoreManager scoreManager;
    public static LevelManager levelManager;
    public static LevelNavmesher levelNavmesher;
    private static float previousTimeScale = 0;

    public enum GameState
    {
        INACTIVE,
        PLAYING,
        PAUSED,
        GAME_OVER,
        LEVEL_CLEARED
    }

    private void Start()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
        levelNavmesher = FindFirstObjectByType<LevelNavmesher>();
        levelNavmesher.Setup();
        GlobalStats.LoadStats();
        StartGame();
    }

    private void Update()
    {
        if(CurrentGameState == GameState.PLAYING)
        {
            InGameTimer += Time.deltaTime;
        }
    }

    public static void StartGame()
    {
        scoreManager.Setup();
        LevelManager.CurrentLevel = 0;
        levelManager.RemoveEntities(true);

        // Force destroy player camera so we don't get any duplicates... ugh...
        PlayerCamera playerCamera = FindFirstObjectByType<PlayerCamera>();
        playerCamera?.DestroyEntity();

        levelManager.CreateLevel();
        UIManager.SetupUI();
        BeginPlaying();
        InGameTimer = 0.0f;
    }

    public static void BeginPlaying()
    {
        CurrentGameState = GameState.PLAYING;
        Time.timeScale = GAME_TIMESCALE;
    }

    public static string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(InGameTimer / 60F);
        int seconds = Mathf.FloorToInt(InGameTimer % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static void RestartGame()
    {
        StartGame();
    }

    public static void PauseGame()
    {
        if(Time.timeScale != 0)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0;
            CurrentGameState = GameState.PAUSED;
            PlayerInput.InputEnabled = false;
        }
    }

    public static bool IsLevelCleared
    {
        get { return CurrentGameState == GameState.LEVEL_CLEARED; }
    }

    public static bool IsGameOver
    {
        get { return CurrentGameState == GameState.GAME_OVER; }
    }

    public static void ResumeGame()
    {
        if(previousTimeScale != 0 && Time.timeScale == 0)
        {
            Time.timeScale = previousTimeScale;
            previousTimeScale = 0;
            CurrentGameState = GameState.PLAYING;
            PlayerInput.InputEnabled = true;
        }
    }

    public static void GameOver()
    {
        CurrentGameState = GameState.GAME_OVER;
        PlayerInput.InputEnabled = false;
        GameOverUI gameOverUI = UIManager.GetUIComponent<GameOverUI>();
        gameOverUI.Show();
        GlobalStats.Save();
    }

    public static void QuitToMainMenu()
    {
        // Save global statistics, load main menu scene...
        GlobalStats.Save();
        SceneManager.LoadScene(0);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}

[System.Serializable]
public class GlobalStatistics
{
    public int Kills { get; set; } = 0;
    public int ItemsCollected { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public float BestTime { get; set; } = 0;
    public int BestKillstreak { get; set; } = 0;
    public int Highscore { get; private set; } = 0;

    public void Save()
    {
        PlayerPrefs.SetInt("kills", Kills);
        PlayerPrefs.SetInt("itemsCollected", Kills);
        PlayerPrefs.SetInt("deaths", Deaths);
        PlayerPrefs.SetFloat("bestTime", BestTime);
        PlayerPrefs.SetInt("bestKillstreak", BestKillstreak);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        Kills = PlayerPrefs.GetInt("kills", 0);
        ItemsCollected = PlayerPrefs.GetInt("itemsCollected", 0);
        Deaths = PlayerPrefs.GetInt("deaths", 0);
        BestTime = PlayerPrefs.GetFloat("bestTime", 0.0f);
        BestKillstreak = PlayerPrefs.GetInt("bestKillstreak", 0);
        Highscore = PlayerPrefs.GetInt("highscore", 0);
    }
}