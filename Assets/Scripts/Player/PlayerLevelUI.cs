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
            system.OnExpChanged -= UpdateExpText;
        }
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
        levelText.text = $"Lv {level}";
    }

    private void UpdateExpText(float current, float required)
    {
        expText.text = $"EXP: {Mathf.FloorToInt(current)} / {Mathf.FloorToInt(required)}";
    }
}
