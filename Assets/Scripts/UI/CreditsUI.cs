using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles everything to do with the credits screen...
/// </summary>
public class CreditsUI : UIComponent
{
    private const string FILE_PATH = "credits.txt";
    private const float SCROLL_SPEED = 50f;

    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI pauseHelpText;
    [SerializeField] private TextMeshProUGUI exitHelpText;
    [SerializeField] private float startYOffset = -500f;
    [SerializeField] private float endYOffset = 500f;

    private bool isPaused = false;
    private Coroutine scrollCoroutine;
    private float flashInterval = 0;
    private float staggerInterval = 0;
    private Coroutine pauseFlashCoroutine;
    private Coroutine exitFlashCoroutine;
    private MusicManager musicManager;

    public override void SetupUI()
    {
        LoadJsonSettings();
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        SetCanvasInteractivity(UICanvasGroup, showUI);

        if(!showUI) { return; }

        musicManager.PlayTrack("credits");
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();
        LoadCredits();
        if(pauseFlashCoroutine != null) { StopCoroutine(pauseFlashCoroutine); }
        if(exitFlashCoroutine != null) { StopCoroutine(exitFlashCoroutine); }
        pauseFlashCoroutine = StartCoroutine(PulseHelpText(pauseHelpText, 0));
        exitFlashCoroutine = StartCoroutine(PulseHelpText(exitHelpText, staggerInterval));
    }

    private void LoadCredits()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, FILE_PATH);

        if(File.Exists(filePath))
        {
            string creditsContent = File.ReadAllText(filePath);
            creditsText.text = creditsContent;
            if(scrollCoroutine != null) { StopCoroutine(scrollCoroutine); }
            scrollCoroutine = StartCoroutine(ScrollCredits());
        }
        else
        {
            Debug.LogError("Credits file not found at: " + filePath);
        }
    }

    private void Update()
    {
        if(UICanvasGroup.alpha > 0 && UICanvasGroup.interactable)
        {
            // Pause the scroll...
            if(Input.GetKeyDown(KeyCode.Space))
            {
                isPaused = !isPaused;
            }

            // Quit to main menu...
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                StopCoroutine(scrollCoroutine);

                FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
                fadeUI.FadeOut();

                FadeUI.OnFadeOutComplete += OnFadeOutComplete;
                SetCanvasInteractivity(UICanvasGroup, false);
            }
        }
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;
        BackToMainMenu();
    }

    private void LoadJsonSettings()
    {
        staggerInterval = (float)JsonData["staggerInterval"];
        flashInterval = (float)JsonData["flashInterval"];
        pauseHelpText.text = LocalizationManager.GetMessage("pauseHelpText", UIJsonIdentifier);
        exitHelpText.text = LocalizationManager.GetMessage("exitHelpText", UIJsonIdentifier);
    }

    private IEnumerator PulseHelpText(TextMeshProUGUI helpText, float delay)
    {
        yield return new WaitForSeconds(delay);

        Color originalColor = helpText.color;
        float alpha = 0;

        while(true)
        {
            while(alpha < 1)
            {
                alpha += Time.deltaTime / flashInterval;
                helpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
                yield return null;
            }

            while(alpha > 0)
            {
                alpha -= Time.deltaTime / flashInterval;
                helpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
                yield return null;
            }
        }
    }

    private IEnumerator ScrollCredits()
    {
        RectTransform rectTransform = creditsText.rectTransform;
        Vector2 startPosition = new Vector2(0, startYOffset);
        Vector2 endPosition = new Vector2(0, endYOffset);

        rectTransform.anchoredPosition = startPosition;

        while(rectTransform.anchoredPosition.y < endPosition.y)
        {
            if(!isPaused)
            {
                rectTransform.anchoredPosition += new Vector2(0, SCROLL_SPEED * Time.deltaTime);
            }

            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
        scrollCoroutine = StartCoroutine(ScrollCredits());
    }

    private void BackToMainMenu()
    {
        // Make main menu interactable again...
        MainMenuUI mainMenuUI = UIManager.GetUIComponent<MainMenuUI>();
        mainMenuUI.SetInteractable(true);

        // Fade into main menu...
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();
        Show(false);

        musicManager.PlayTrack("main_menu");
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying && creditsText != null && UICanvasGroup.alpha > 0)
        {
            RectTransform rectTransform = creditsText.rectTransform;
            Vector2 startPosition = new Vector2(rectTransform.position.x, rectTransform.position.y + startYOffset);
            Vector2 endPosition = new Vector2(rectTransform.position.x, rectTransform.position.y + endYOffset);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector2(startPosition.x - 1000, startPosition.y), new Vector2(startPosition.x + 1000, startPosition.y));
            Gizmos.DrawSphere(startPosition, 10f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(endPosition.x - 1000, endPosition.y), new Vector2(endPosition.x + 1000, endPosition.y));
            Gizmos.DrawSphere(endPosition, 10f);
        }
    }

    public override string UIJsonIdentifier => "credits_ui";
}
