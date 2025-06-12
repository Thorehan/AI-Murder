using UnityEngine;
using UnityEngine.UI;

public class NPCUIToggle : MonoBehaviour
{
    [Header("UI References")]
    public GameObject npcManagerPanel;
    public Button toggleButton;
    public KeyCode toggleKey = KeyCode.N;

    [Header("Button Settings")]
    public string showText = "Open NPC Manager";
    public string hideText = "Close NPC Manager";

    private bool isUIVisible = false;

    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleUI);
        }

        if (npcManagerPanel != null)
        {
            npcManagerPanel.SetActive(false);
            isUIVisible = false;
        }

        UpdateButtonText();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        if (npcManagerPanel != null)
        {
            isUIVisible = !isUIVisible;
            npcManagerPanel.SetActive(isUIVisible);
            UpdateButtonText();

            Debug.Log($"NPC Manager UI: {(isUIVisible ? "Opened" : "Closed")}");
        }
    }

    public void ShowUI()
    {
        if (npcManagerPanel != null)
        {
            isUIVisible = true;
            npcManagerPanel.SetActive(true);
            UpdateButtonText();
        }
    }

    public void HideUI()
    {
        if (npcManagerPanel != null)
        {
            isUIVisible = false;
            npcManagerPanel.SetActive(false);
            UpdateButtonText();
        }
    }

    void UpdateButtonText()
    {
        if (toggleButton != null)
        {
            var buttonText = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isUIVisible ? hideText : showText;
            }
        }
    }
}