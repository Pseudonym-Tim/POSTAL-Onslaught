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

    public static GameState CurrentGameState { get; set; } = GameState.INACTIVE;
    public static ScoreManager scoreManager;
    public static LevelManager levelManager;
    public static LevelNavmesher levelNavmesher;
    public static float inGameTimer { get; private set; } = 0.0f;
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
        StartGame();
    }

    private void Update()
    {
        if(CurrentGameState == GameState.PLAYING)
        {
            inGameTimer += Time.deltaTime;
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
        inGameTimer = 0.0f;
    }

    public static void BeginPlaying()
    {
        CurrentGameState = GameState.PLAYING;
        Time.timeScale = GAME_TIMESCALE;
    }

    public static string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(inGameTimer / 60F);
        int seconds = Mathf.FloorToInt(inGameTimer % 60F);
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
        GameOverUI gameOverUI = UIManager.GetUIComponent<GameOverUI>();
        gameOverUI.Show();
    }

    public static void QuitToMainMenu()
    {
        // Load main menu scene...
        SceneManager.LoadScene(0);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
