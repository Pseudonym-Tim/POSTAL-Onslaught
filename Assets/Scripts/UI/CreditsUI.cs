using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything to do with the credits screen...
/// </summary>
public class CreditsUI : UIComponent
{
    private const string FILE_PATH = "credits.txt";
    private const float SCROLL_SPEED = 50f;

    public Canvas UICanvas;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private float startYOffset = -500f;
    [SerializeField] private float endYOffset = 500f;

    private bool isPaused = false;
    private Coroutine scrollCoroutine;

    public override void SetupUI()
    {
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        if(!showUI) { return; }
        LoadCredits();
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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            StopCoroutine(scrollCoroutine);
            BackToMainMenu();
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
        MainMenuUI mainMenuUI = UIManager.GetUIComponent<MainMenuUI>();
        mainMenuUI.SetInteractable(true);

        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();
        Show(false);
    }

    private void OnDrawGizmos()
    {
        if(creditsText != null)
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
}
