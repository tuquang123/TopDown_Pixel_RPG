using TMPro;
using UnityEngine;

public class PlayerLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerLevel playerLevel;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Start()
    {
        if (playerLevel != null)
        {
            playerLevel.levelSystem.OnLevelUp += UpdateLevelText;
            UpdateLevelText(playerLevel.levelSystem.level);
        }
    }

    private void OnDestroy()
    {
        if (playerLevel != null)
            playerLevel.levelSystem.OnLevelUp -= UpdateLevelText;
    }

    private void UpdateLevelText(int level)
    {
        levelText.text = $"Lv {level}";
    }
}