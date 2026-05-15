using System.Collections.Generic;
using UnityEngine;

namespace RPG.Skills.LevelUpSelection
{
    public sealed class LevelUpSkillSelectionController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerLevel playerLevel;
        [SerializeField] private CodexManager codexManager;
        [SerializeField] private LevelUpSkillPopupUI popupUI;

        [Header("Config")]
        [SerializeField, Min(1)] private int optionsPerPopup = 3;
        [SerializeField] private bool allowReroll;
        [SerializeField, Min(0)] private int maxRerolls = 1;

        private int remainingRerolls;
        private bool popupOpen;

        private void Awake()
        {
            remainingRerolls = maxRerolls;
        }

        private void OnEnable()
        {
            if (playerLevel != null)
            {
                playerLevel.levelSystem.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (playerLevel != null)
            {
                playerLevel.levelSystem.OnLevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int _)
        {
            if (popupOpen)
            {
                return;
            }

            OpenPopup();
        }

        private void OpenPopup()
        {
            popupOpen = true;
            Time.timeScale = 0f;
            ShowRandomOptions();
        }

        private void ShowRandomOptions()
        {
            List<PassiveSkillData> options = codexManager.GetRandomSelection(optionsPerPopup);
            popupUI.Show(
                options,
                codexManager.GetCurrentLevel,
                HandleSkillSelected,
                allowReroll && remainingRerolls > 0 ? HandleReroll : null);
        }

        private void HandleReroll()
        {
            if (remainingRerolls <= 0)
            {
                return;
            }

            remainingRerolls--;
            ShowRandomOptions();
        }

        private void HandleSkillSelected(PassiveSkillData selectedSkill)
        {
            codexManager.AddOrUpgrade(selectedSkill);
            popupUI.Hide();
            popupOpen = false;
            remainingRerolls = maxRerolls;
            Time.timeScale = 1f;
        }
    }
}
