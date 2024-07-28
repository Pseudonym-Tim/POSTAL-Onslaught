using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the main menu UI...
/// </summary>
public class MainMenuUI : UIComponent
{
    private const float BOB_FREQUENCY = 5.0f;
    private const float BOB_AMPLITUDE = 10.0f;

    public CanvasGroup UICanvasGroup;
    public Image logoImage;
    public List<Image> menuOptions;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private Vector3 initialLogoPosition;
    private FadeUI fadeUI;
    private OptionsUI optionsUI;
    private int selectedOptionIndex = 0;
    private MusicManager musicManager;
    private int? currentlyHoveredOption = null;

    private void Awake()
    {
        initialLogoPosition = logoImage.rectTransform.localPosition;
        musicManager = FindFirstObjectByType<MusicManager>();
        fadeUI = UIManager.GetUIComponent<FadeUI>();
        optionsUI = UIManager.GetUIComponent<OptionsUI>();
        SetCanvasInteractivity(UICanvasGroup, true);
        currentlyHoveredOption = null;
    }

    public override void SetupUI()
    {
        LoadJsonSettings();
        SetOptionsInactive();
        fadeUI.FadeIn();
        selectedOptionIndex = 0;
        musicManager.PlayTrack("Corpse Club");
    }

    private void LateUpdate()
    {
        UpdateLogoBob();
    }

    public void SetInteractable(bool interact)
    {
        SetCanvasInteractivity(UICanvasGroup, interact);
    }

    public void SelectOption(int optionIndex)
    {
        if(optionsUI.UICanvas.enabled) { return; }

        switch(optionIndex)
        {
            case 0: // Play...
                BeginFade();
                break;
            case 1: // Options...
                optionsUI.Show();
                break;
            case 2: // Stats...
                BeginFade();
                Debug.Log("STATS!");
                break;
            case 3: // Credits...
                BeginFade();
                Debug.Log("CREDITS!");
                break;
            case 4: // Quit game...
                BeginFade();
                break;
        }

        selectedOptionIndex = optionIndex;
    }

    private void BeginFade()
    {
        SetCanvasInteractivity(UICanvasGroup, false);
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        FadeUI.OnFadeOutComplete += OnFadeOutComplete;
        fadeUI.FadeOut();
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;

        switch(selectedOptionIndex)
        {
            case 0: // Play...
                SceneManager.LoadScene(1);
                break;
            case 2: // Stats...
                // TODO: Pull up stats menu...
                break;
            case 3: // Credits...
                CreditsUI creditsUI = UIManager.GetUIComponent<CreditsUI>();
                creditsUI.Show();
                break;
            case 4: // Quit game...
                Application.Quit();
                break;
        }
    }

    public void HoverOption(int optionIndex)
    {
        if(optionsUI.UICanvas.enabled) { return; }

        if(currentlyHoveredOption == optionIndex) { return; }

        if(currentlyHoveredOption.HasValue)
        {
            UnhoverOption(currentlyHoveredOption.Value);
        }

        currentlyHoveredOption = optionIndex;

        Image hoveredImage = menuOptions[optionIndex];
        Color hoverColor;
        ColorUtility.TryParseHtmlString(activeColor, out hoverColor);
        hoveredImage.color = hoverColor;
        hoveredImage.rectTransform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        if(currentlyHoveredOption != optionIndex) { return; }

        Image hoveredImage = menuOptions[optionIndex];
        Color unhoverColor;
        ColorUtility.TryParseHtmlString(inactiveColor, out unhoverColor);
        hoveredImage.color = unhoverColor;
        hoveredImage.rectTransform.localPosition -= new Vector3(0, hoverOffset, 0);

        currentlyHoveredOption = null;
    }

    private void SetOptionsInactive()
    {
        Color unhoveredColor;
        ColorUtility.TryParseHtmlString(inactiveColor, out unhoveredColor);

        for(int i = 0; i < menuOptions.Count; i++)
        {
            menuOptions[i].color = unhoveredColor;
        }
    }

    private void LoadJsonSettings()
    {
        inactiveColor = (string)JsonData["options"]["inactiveColor"];
        activeColor = (string)JsonData["options"]["activeColor"];
        hoverOffset = (float)JsonData["options"]["hoverOffset"];
    }

    private void UpdateLogoBob()
    {
        Vector3 bobbingPosition = initialLogoPosition;
        
        bobbingPosition.y += Mathf.Sin(Time.time * BOB_FREQUENCY) * BOB_AMPLITUDE;
        logoImage.rectTransform.localPosition = bobbingPosition;
    }

    public override string UIJsonIdentifier => "main_menu_ui";
}
