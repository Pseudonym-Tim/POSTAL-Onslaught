using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Points towards the level exit...
/// </summary>
public class ExitIndicatorUI : UIComponent
{
    private const float BORDER_OFFSET = 250f;
    private const float BOB_FREQUENCY = 5f;
    private const float BOB_AMPLITUDE = 25 / 3f;

    [SerializeField] private Camera UICamera;
    private Vector3 currentExitOrigin;
    private Image indicatorImage;
    private TaskManager taskManager;
    private PlayerCamera playerCamera;
    private LevelManager levelManager;
    [SerializeField] private TextMeshProUGUI indicatorText; 
    private Vector3 originalIndicatorTextPos;

    private void Awake()
    {
        taskManager = FindFirstObjectByType<TaskManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
        indicatorImage = GetComponentInChildren<Image>();
        indicatorImage.enabled = false;
        indicatorText.enabled = false;
        originalIndicatorTextPos = indicatorText.rectTransform.localPosition;
    }

    public void Initialize(Vector3 exitOrigin)
    {
        currentExitOrigin = exitOrigin;
        indicatorText.text = LocalizationManager.GetMessage("indicatorText");
        indicatorImage.enabled = false;
        indicatorText.enabled = false;
        playerCamera = levelManager.GetEntity<Player>().PlayerCamera;
    }

    private void Update()
    {
        if(GameManager.CurrentGameState == GameManager.GameState.PLAYING)
        {
            if(taskManager.IsTaskComplete)
            {
                Vector3 origin = playerCamera.EntityPosition;
                origin.z = 0;
                Vector3 dir = (origin - currentExitOrigin).normalized;
                float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) % 360;
                indicatorImage.rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90f);

                Vector3 targetScreenPoint = playerCamera.Camera.WorldToScreenPoint(currentExitOrigin);

                bool isOffscreen = targetScreenPoint.x <= BORDER_OFFSET || targetScreenPoint.x >= Screen.width - BORDER_OFFSET || targetScreenPoint.y <= BORDER_OFFSET || targetScreenPoint.y >= Screen.height - BORDER_OFFSET;

                if(isOffscreen)
                {
                    Vector3 clampedScreenPos = targetScreenPoint;
                    if(clampedScreenPos.x <= BORDER_OFFSET) { clampedScreenPos.x = BORDER_OFFSET; }
                    if(clampedScreenPos.x >= Screen.width - BORDER_OFFSET) { clampedScreenPos.x = Screen.width - BORDER_OFFSET; }
                    if(clampedScreenPos.y <= BORDER_OFFSET) { clampedScreenPos.y = BORDER_OFFSET; }
                    if(clampedScreenPos.y >= Screen.height - BORDER_OFFSET) { clampedScreenPos.y = Screen.height - BORDER_OFFSET; }

                    Vector3 indicatorWorldPos = UICamera.ScreenToWorldPoint(clampedScreenPos);
                    indicatorImage.rectTransform.position = indicatorWorldPos;
                    Vector3 indicatorLocalPos = indicatorImage.rectTransform.localPosition;

                    // Bobbing effect...
                    float bobbingOffset = Mathf.Sin(Time.time * BOB_FREQUENCY) * BOB_AMPLITUDE;
                    indicatorImage.rectTransform.localPosition = new Vector3(indicatorLocalPos.x, indicatorLocalPos.y + bobbingOffset, 0);
                    indicatorText.rectTransform.localPosition = new Vector3(indicatorLocalPos.x, indicatorLocalPos.y + bobbingOffset + 55f, indicatorLocalPos.z);
                }

                indicatorImage.enabled = IsExitOffscreen;
                indicatorText.enabled = IsExitOffscreen && Mathf.PingPong(Time.time * 4, 1) > 0.5f;
            }
        }
        else
        {
            indicatorImage.enabled = false;
            indicatorText.enabled = false;
        }
    }

    public bool IsExitOffscreen
    {
        get
        {
            Vector3 viewportPoint = playerCamera.Camera.WorldToViewportPoint(currentExitOrigin);

            // Check if the exit origin is within the viewport bounds...
            bool isOffscreen = viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1;

            return isOffscreen;
        }
    }
}