using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Skills.LevelUpSelection
{
    public sealed class LevelUpSkillPopupUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupRoot;
        [SerializeField] private List<SkillOptionSlotUI> slots = new();
        [SerializeField] private Button rerollButton;
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        private Action onReroll;
        private Action<PassiveSkillData> onSkillPicked;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            SetVisible(false, instant: true);
            rerollButton.gameObject.SetActive(false);
        }

        public void Show(IReadOnlyList<PassiveSkillData> options, Func<string, int> getCurrentLevel, Action<PassiveSkillData> onPick, Action onRerollPressed = null)
        {
            onSkillPicked = onPick;
            onReroll = onRerollPressed;

            for (var i = 0; i < slots.Count; i++)
            {
                var hasData = i < options.Count;
                slots[i].gameObject.SetActive(hasData);

                if (!hasData)
                {
                    continue;
                }

                var option = options[i];
                var currentLevel = getCurrentLevel(option.SkillId);
                slots[i].Bind(option, currentLevel, HandlePick);
            }

            rerollButton.gameObject.SetActive(onRerollPressed != null);
            rerollButton.onClick.RemoveAllListeners();
            rerollButton.onClick.AddListener(HandleReroll);

            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void HandlePick(PassiveSkillData data)
        {
            onSkillPicked?.Invoke(data);
        }

        private void HandleReroll()
        {
            onReroll?.Invoke();
        }

        private void SetVisible(bool visible, bool instant = false)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            if (instant || fadeDuration <= 0f)
            {
                ApplyVisibility(visible ? 1f : 0f, visible);
                return;
            }

            fadeRoutine = StartCoroutine(FadeRoutine(visible));
        }

        private IEnumerator FadeRoutine(bool visible)
        {
            if (visible)
            {
                gameObject.SetActive(true);
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            var start = canvasGroup.alpha;
            var target = visible ? 1f : 0f;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }

            ApplyVisibility(target, visible);
            fadeRoutine = null;
        }

        private void ApplyVisibility(float alpha, bool visible)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
            gameObject.SetActive(visible);

            if (popupRoot != null && visible)
            {
                popupRoot.anchoredPosition = Vector2.zero;
            }
        }
    }
}
