using System.Collections;
using UnityEngine;

/// <summary>
/// Handles everything related to the scoring system.
/// </summary>
public class ScoreManager : Singleton<ScoreManager>
{
    private const int MAX_SCORE = int.MaxValue;
    private const float KILLSTREAK_TIME = 5f / 3;
    private PlayerHUD playerHUD;

    public int KillstreakAmount { get; private set; } = 0;
    public int CurrentScore { get; private set; } = 0;
    public int CurrentHighscore { get; private set; } = 0;

    public void Setup()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        CurrentScore = 0;
        playerHUD.UpdateScore(CurrentScore);
        LoadHighscore();
    }

    public void AddScore(int scoreToAdd, bool isNPCKill = false)
    {
        if(isNPCKill)
        {
            KillstreakAmount++;
            StopAllCoroutines();
            StartCoroutine(HandleScoreMultiplier(scoreToAdd * KillstreakAmount));

            // Update highest killstreak...
            if(KillstreakAmount > GameManager.GameStats.HighestKillstreak)
            {
                GameManager.GameStats.HighestKillstreak = KillstreakAmount;
            }
        }
        else
        {
            CurrentScore += scoreToAdd;

            if(CurrentScore > MAX_SCORE)
            {
                CurrentScore = MAX_SCORE;
            }
        }

        playerHUD.UpdateScore(CurrentScore);
        CheckUpdateHighscore();
    }

    private void CheckUpdateHighscore()
    {
        if(CurrentScore > CurrentHighscore)
        {
            CurrentHighscore = CurrentScore;
            PlayerPrefs.SetInt("highscore", CurrentHighscore);
            PlayerPrefs.Save();
            playerHUD.UpdateHighscoreText(CurrentHighscore);
        }
    }

    private void LoadHighscore()
    {
        CurrentHighscore = PlayerPrefs.GetInt("highscore", 0);
        playerHUD.UpdateHighscoreText(CurrentHighscore);
    }

    private void ResetKillstreak()
    {
        KillstreakAmount = 0;
        playerHUD.ShowScoreMultiplier(false);
    }

    private IEnumerator HandleScoreMultiplier(int scoreToAward)
    {
        const float FADE_TIME = 0.5f;
        const float TICK_SPEED = 12.5f;
        int initialScore = CurrentScore;
        int targetScore = initialScore + scoreToAward;
        float tickTimer = 0f;
        float fadeTimer = 0f;
        float animDuration = KILLSTREAK_TIME / TICK_SPEED;

        playerHUD.ShowScoreMultiplier();

        while(tickTimer < KILLSTREAK_TIME)
        {
            tickTimer += Time.deltaTime;
            float progressAmount = Mathf.Clamp01(tickTimer / animDuration);
            int currentScore = Mathf.RoundToInt(Mathf.Lerp(initialScore, targetScore, progressAmount));
            playerHUD.UpdateScoreMultiplier(currentScore - initialScore, KillstreakAmount, 1);
            yield return null;
        }

        // Ensure the score is set to targetScore before fading...
        playerHUD.UpdateScoreMultiplier(targetScore, KillstreakAmount, 1);

        while(fadeTimer < FADE_TIME)
        {
            fadeTimer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(1f, 0f, fadeTimer / FADE_TIME);
            playerHUD.UpdateScoreMultiplier(targetScore - initialScore, KillstreakAmount, currentAlpha);
            yield return null;
        }

        CurrentScore += scoreToAward * KillstreakAmount;

        if(CurrentScore > MAX_SCORE)
        {
            CurrentScore = MAX_SCORE;
        }

        playerHUD.UpdateScore(CurrentScore);

        ResetKillstreak();

        CheckUpdateHighscore();
    }

    public void AwardBonusScore()
    {
        int bonusScore = CalculateBonusScore();
        AddScore(bonusScore);
        playerHUD.UpdateScore(CurrentScore);
    }

    public int CalculateBonusScore()
    {
        // Using GameStatistics and inGameTimer to calculate bonus score
        GameManager.GameStatistics stats = GameManager.GameStats;
        float gameTime = GameManager.inGameTimer;

        // Formula for bonus score calculation...
        int killBonus = stats.CurrentKills * 10; // 10 points per kill
        int weaponBonus = stats.UniqueWeaponsUsed * 50; // 50 points per unique weapon used
        int streakBonus = stats.HighestKillstreak * 100; // 100 points per highest kill streak
        int distanceBonus = Mathf.FloorToInt(stats.DistanceCovered); // 1 point per unit distance

        // Calculate time bonus (e.g., max bonus of 1000 points, reducing with time)...
        const int maxTimeBonus = 1000;
        const float maxTime = 600f / 2; // Optimal completion time (5 minutes)...
        int timeBonus = Mathf.Clamp(Mathf.FloorToInt(maxTimeBonus * (1 - (gameTime / maxTime))), 0, maxTimeBonus);

        int totalBonus = killBonus + weaponBonus + streakBonus + distanceBonus + timeBonus;
        return totalBonus;
    }
}