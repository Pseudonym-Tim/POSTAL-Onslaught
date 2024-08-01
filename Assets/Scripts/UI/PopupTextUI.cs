using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Screen-space UI flavor text...
/// </summary>
public class PopupTextUI : UIComponent
{
    private const float DESTROY_TIME = 5.0f;
    [SerializeField] private TextMeshProUGUI messageText;

    private void Awake() => SetupUI();

    public override void SetupUI()
    {
        Destroy(gameObject, DESTROY_TIME);
    }

    public static PopupTextUI Create(Vector2 spawnPos, string message, Transform parent, float scale = 1.0f)
    {
        if(Camera.main != null)
        {
            PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
            PopupTextUI popupTextPrefab = prefabDatabase?.GetPrefab<PopupTextUI>(nameof(PopupTextUI));
            spawnPos = Camera.main.WorldToScreenPoint(spawnPos);
            PopupTextUI popupTextUI = Instantiate(popupTextPrefab, spawnPos, Quaternion.identity);
            popupTextUI.transform.SetParent(parent, true);
            popupTextUI.name = nameof(PopupTextUI);
            popupTextUI.transform.localScale = Vector3.one * scale;
            if(message != null) { popupTextUI.messageText.text = message; }
            return popupTextUI;
        }

        return null;
    }
}
