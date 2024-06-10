using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles core game related stuff and setup...
/// </summary>
public class GameManager : Singleton<GameManager>
{
    private const float GAME_TIMESCALE = 1.0f;
    public const string GAME_NAME = "POSTAL: Onslaught";

    public static GameState CurrentGameState { get; set; } = GameState.INACTIVE;
    public static float inGameTimer { get; private set; } = 0.0f;
    private static float previousTimeScale = 0;

    public enum GameState
    {
        INACTIVE,
        PLAYING,
        PAUSED,
        GAME_OVER
    }

    private void Start()
    {
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
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        levelManager.CreateLevel();

        CurrentGameState = GameState.PLAYING;
        Time.timeScale = GAME_TIMESCALE;
        inGameTimer = 0.0f;
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
        }
    }

    public static void ResumeGame()
    {
        if(previousTimeScale != 0 && Time.timeScale == 0)
        {
            Time.timeScale = previousTimeScale;
            previousTimeScale = 0;
            CurrentGameState = GameState.PLAYING;
        }
    }

    public static void GameOver()
    {
        
    }

    public static void BackToMenu()
    {

    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
