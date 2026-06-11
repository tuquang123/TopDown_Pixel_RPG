using TMPro;
using UnityEngine;

public class PlayerLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerLevel playerLevel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    private void Start()
    {
        if (playerLevel != null)
        {
            var system = playerLevel.levelSystem;
            system.OnLevelUp += UpdateLevelText;
            system.OnLevelUp += OnLevelUp;
            system.OnExpChanged += UpdateExpText;

            UpdateLevelText(system.level);
            UpdateExpText(system.exp, system.ExpRequired);
        }
    }

    private void OnDestroy()
    {
        if (playerLevel != null)
        {
            var system = playerLevel.levelSystem;
            system.OnLevelUp -= UpdateLevelText;
            system.OnLevelUp -= OnLevelUp;
            system.OnExpChanged -= UpdateExpText;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        foreach (var btn in FindObjectsOfType<FeatureButtonUI>())
            btn.RefreshLockState();
    }

    public void RefreshUI()
    {
        if (playerLevel == null) return;
        var system = playerLevel.levelSystem;
        UpdateLevelText(system.level);
        UpdateExpText(system.exp, system.ExpRequired);
    }

    private void UpdateLevelText(int level)
    {
        levelText.text = $"{level}";
    }

    private void UpdateExpText(float current, float required)
    {
        expText.text = $"EXP: {Mathf.FloorToInt(current)} / {Mathf.FloorToInt(required)}";
    }
}