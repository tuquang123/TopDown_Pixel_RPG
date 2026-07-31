using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class PlayerStatsPopupUI : BasePopup
{
    public StatDisplayComponent statDisplayComponent;

    [SerializeField] private PlayerStatsDataSO dataAsset;
    [SerializeField] private int unlockStep = 5;

    [Header("Cost Texts")]
    [SerializeField] private TextMeshProUGUI attackCostText, defenseCostText, speedCostText, critCostText,
                                             lifestealCostText, attackSpeedCostText, healthCostText, manaCostText;

    [Header("Preview Texts")]
    [SerializeField] private TextMeshProUGUI attackPreview, defensePreview, speedPreview, critPreview,
                                             lifestealPreview, attackSpeedPreview, healthPreview, manaPreview;

    [Header("Multiplier Buttons")]
    [SerializeField] private Button btnX1, btnX10, btnX100;

    [Header("Requirement Popups")]
    [SerializeField] private bool autoCreateRequirementPopups = true;
    [SerializeField] private GameObject critRequirementPopup;
    [SerializeField] private TextMeshProUGUI critRequirementText;
    [SerializeField] private GameObject lifestealRequirementPopup;
    [SerializeField] private TextMeshProUGUI lifestealRequirementText;
    [SerializeField] private GameObject attackSpeedRequirementPopup;
    [SerializeField] private TextMeshProUGUI attackSpeedRequirementText;
    [SerializeField] private GameObject speedRequirementPopup;
    [SerializeField] private TextMeshProUGUI speedRequirementText;

    [Header("Insufficient Gold Toast Limit")]
    [SerializeField] private int maxInsufficientGoldToasts = 3;
    [SerializeField] private float insufficientGoldStreakResetGap = 0.5f;

    private PlayerStatsDataContainer data => dataAsset.stats;
    private int currentMultiplier = 1;

    private int insufficientGoldStreak = 0;
    private float lastInsufficientGoldTime = -999f;

    public override void Show()
    {
        base.Show();
        dataAsset.Load();
        ApplyBaseStatsOnly();
        EnsureRequirementPopups();
        SetMultiplier(1);
        insufficientGoldStreak = 0;
    }

    private void SetMultiplier(int value)
    {
        currentMultiplier = value;

        btnX1.interactable   = value != 1;
        btnX10.interactable  = value != 10;
        btnX100.interactable = value != 100;

        RefreshUI();
    }

    private void RefreshUI()
    {
        statDisplayComponent.SetStats(PlayerStats.Instance);
        RefreshCostTexts();
        RefreshPreviewTexts();
        RefreshRequirementPopups();
        NormalizeStatsUiText();
        UpdateLocalizedStaticText();
    }

    private void RefreshCostTexts()
    {
        SetCostText(attackCostText,      data.attack,      currentMultiplier);
        SetCostText(defenseCostText,     data.defense,     currentMultiplier);
        SetCostText(speedCostText,       data.speed,       currentMultiplier);
        SetCostText(critCostText,        data.crit,        currentMultiplier);
        SetCostText(lifestealCostText,   data.lifesteal,   currentMultiplier);
        SetCostText(attackSpeedCostText, data.attackSpeed, currentMultiplier);
        SetCostText(healthCostText,      data.health,      currentMultiplier);
        SetCostText(manaCostText,        data.mana,        currentMultiplier);
    }

    private void RefreshPreviewTexts()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        SetPreviewText(attackPreview,      data.attack,      ps.attack.Value,      currentMultiplier, false);
        SetPreviewText(defensePreview,     data.defense,     ps.defense.Value,     currentMultiplier, false);
        SetPreviewText(speedPreview,       data.speed,       ps.speed.Value,       currentMultiplier, false);
        SetPreviewText(critPreview,        data.crit,        ps.critChance.Value,  currentMultiplier, true);
        SetPreviewText(lifestealPreview,   data.lifesteal,   ps.lifeSteal.Value,   currentMultiplier, true);
        SetPreviewText(attackSpeedPreview, data.attackSpeed, ps.attackSpeed.Value, currentMultiplier, true);
        SetPreviewText(healthPreview,      data.health,      ps.maxHealth.Value,   currentMultiplier, false);
        SetPreviewText(manaPreview,        data.mana,        ps.maxMana.Value,     currentMultiplier, false);
    }

    private void SetPreviewText(TextMeshProUGUI text, PlayerStatData stat, float currentValue, int times, bool isPercent)
    {
        if (text == null) return;
        PreparePreviewText(text);

        float nextValue  = currentValue + times * stat.increasePerLevel;
        int   currentLvl = stat.GetLevelFromValue(currentValue);
        int   nextLvl    = currentLvl + times;

        string current = FormatStatValue(currentValue, isPercent);
        string next    = FormatStatValue(nextValue,    isPercent);

        string levelLabel = T("lv", "Lv");
        string toLabel = IsVietnamese() ? "l\u00ean" : "to";
        text.text = $"<color=#AAAAAA>{levelLabel}.{currentLvl}</color>  {current} <color=#888888>>></color> <color=#00FF99>{next}</color>  <color=#AAAAAA>({toLabel} {levelLabel}.{nextLvl})</color>";
    }
    private string FormatStatValue(float value, bool isPercent)
    {
        if (isPercent)
            return $"{value:0.#}%";

        return $"{value:0.#}";
    }

    private string FormatGoldValue(long value)
    {
        if (value >= 1000000)
            return $"{value / 1000000f:0.#}M";

        if (value >= 1000)
            return $"{value / 1000f:0.#}K";

        return value.ToString();
    }

    private void SetCostText(TextMeshProUGUI text, PlayerStatData stat, int times)
    {
        if (text == null) return;
        PrepareCostText(text);

        if (!CanUpgradeStat(stat, times, out PlayerStatData requiredStat, out int requiredLevel, out int currentRequiredLevel))
        {
            string lockedText = T("locked", "Locked");
            text.text = $"<color=#151515>{lockedText}</color>";
            return;
        }

        long cost      = CalculateTotalCost(stat, times);
        int  safeCost  = cost > int.MaxValue ? int.MaxValue : (int)cost;
        bool canAfford = CurrencyManager.Instance.Gold >= safeCost;
        string display = CurrencyManager.FormatGold(safeCost);

        text.text = canAfford
            ? $"<color=#FFFFFF>{display}</color> <sprite name=\"gold_icon\">"
            : $"<color=#FF4444>{display}</color> <sprite name=\"gold_icon\">";
    }

    private void NormalizeStatsUiText()
    {
        NormalizeButtonTexts();
        PreparePreviewText(attackPreview);
        PreparePreviewText(defensePreview);
        PreparePreviewText(speedPreview);
        PreparePreviewText(critPreview);
        PreparePreviewText(lifestealPreview);
        PreparePreviewText(attackSpeedPreview);
        PreparePreviewText(healthPreview);
        PreparePreviewText(manaPreview);
        PrepareCostText(attackCostText);
        PrepareCostText(defenseCostText);
        PrepareCostText(speedCostText);
        PrepareCostText(critCostText);
        PrepareCostText(lifestealCostText);
        PrepareCostText(attackSpeedCostText);
        PrepareCostText(healthCostText);
        PrepareCostText(manaCostText);
    }

    private void NormalizeButtonTexts()
    {
        TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var label in labels)
        {
            if (label == null) continue;

            string raw = label.text.Trim();
            bool isButtonLabel = IsLevelUpText(raw) || IsLockedText(raw);
            if (!isButtonLabel) continue;

            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    private void UpdateLocalizedStaticText()
    {
        TextMeshProUGUI[] labels = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var label in labels)
        {
            if (label == null) continue;

            string raw = label.text.Trim();
            if (IsLevelUpText(raw))
            {
                label.text = T("level_up", "Level Up");
            }
            else if (IsLockedText(raw))
            {
                label.text = T("locked", "Locked");
            }
            else if (IsStatsTitleText(raw))
            {
                label.text = T("stats", "Stats");
            }
        }
    }

    private bool IsLevelUpText(string text)
    {
        return text == "Level Up"
            || text == "LEVEL UP"
            || text == "N\u00e2ng C\u1ea5p"
            || text == "N\u00c2NG C\u1ea4P";
    }

    private bool IsLockedText(string text)
    {
        return text == "Locked"
            || text == "LOCKED"
            || text == "\u0110\u00e3 Kh\u00f3a"
            || text == "\u0110\u00c3 KH\u00d3A";
    }

    private bool IsStatsTitleText(string text)
    {
        return text == "Stats"
            || text == "STATS"
            || text == "Ch\u1ec9 S\u1ed1"
            || text == "CH\u1ec8 S\u1ed0";
    }

    private void PreparePreviewText(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private void PrepareCostText(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Center;
    }

    private void ApplyBaseStatsOnly()
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return;

        ps.attack.SetBaseValue(data.attack.GetValueAtLevel(dataAsset.GetSavedLevel(data.attack)));
        ps.defense.SetBaseValue(data.defense.GetValueAtLevel(dataAsset.GetSavedLevel(data.defense)));
        ps.speed.SetBaseValue(data.speed.GetValueAtLevel(dataAsset.GetSavedLevel(data.speed)));
        ps.critChance.SetBaseValue(data.crit.GetValueAtLevel(dataAsset.GetSavedLevel(data.crit)));
        ps.lifeSteal.SetBaseValue(data.lifesteal.GetValueAtLevel(dataAsset.GetSavedLevel(data.lifesteal)));
        ps.attackSpeed.SetBaseValue(data.attackSpeed.GetValueAtLevel(dataAsset.GetSavedLevel(data.attackSpeed)));

        float newMaxHp = data.health.GetValueAtLevel(dataAsset.GetSavedLevel(data.health));
        ps.maxHealth.SetBaseValue(newMaxHp);
        ps.currentHealth = Mathf.Clamp(ps.currentHealth, 1, (int)ps.maxHealth.Value);
        ps.NotifyHealthChanged();

        float newMaxMana = data.mana.GetValueAtLevel(dataAsset.GetSavedLevel(data.mana));
        ps.maxMana.SetBaseValue(newMaxMana);
        ps.currentMana = Mathf.Clamp(ps.currentMana, 0, (int)ps.maxMana.Value);
        ps.NotifyManaChanged();

        ps.CalculatePower();
        ps.NotifyStatsChanged();
    }


    private long CalculateTotalCost(PlayerStatData stat, int times)
    {
        var ps = PlayerStats.Instance;
        if (ps == null) return 0;

        float runtimeValue = GetUpgradeableRuntimeValueForStat(ps, stat);
        int currentLevel = stat.GetLevelFromValue(runtimeValue);

        long total = 0;
        for (int i = 0; i < times; i++)
        {
            total += stat.GetUpgradeCost(currentLevel + i);
        }

        return total;
    }

    private float GetUpgradeableRuntimeValueForStat(PlayerStats ps, PlayerStatData stat)
    {
        if (stat == data.attack) return ps.attack.baseValue;
        if (stat == data.defense) return ps.defense.baseValue;
        if (stat == data.speed) return ps.speed.baseValue;
        if (stat == data.crit) return ps.critChance.baseValue;
        if (stat == data.lifesteal) return ps.lifeSteal.baseValue;
        if (stat == data.attackSpeed) return ps.attackSpeed.baseValue;
        if (stat == data.health) return ps.maxHealth.baseValue;
        if (stat == data.mana) return ps.maxMana.baseValue;

        return stat.GetValueAtLevel(dataAsset.GetSavedLevel(stat));
    }

    private void TryUpgrade(PlayerStatData stat, System.Func<PlayerStats, Stat> getter, int times, Transform animTarget = null)
    {
        if (!CanUpgradeStat(stat, times, out PlayerStatData requiredStat, out int requiredLevel, out int currentRequiredLevel))
        {
            GameEvents.OnShowToast.Raise(GetPreviousStatRequiredText(requiredStat, requiredLevel, currentRequiredLevel));
            return;
        }

        long totalCost = CalculateTotalCost(stat, times);
        int  safeCost  = totalCost > int.MaxValue ? int.MaxValue : (int)totalCost;

        if (!CurrencyManager.Instance.SpendGold(safeCost))
        {
            ShowInsufficientGoldToastLimited();
            return;
        }

        insufficientGoldStreak = 0;

        var ps           = PlayerStats.Instance;
        var playerStat   = getter(ps);
        int currentLevel = stat.GetLevelFromValue(playerStat.baseValue);
        int nextLevel    = currentLevel + times;

        float oldMax = playerStat.Value;
        playerStat.SetBaseValue(stat.GetValueAtLevel(nextLevel));
        float newMax = playerStat.Value;
        dataAsset.SetSavedLevel(stat, nextLevel);

        if (playerStat == ps.maxHealth && oldMax > 0)
        {
            ps.currentHealth = (int)newMax;
            ps.NotifyHealthChanged();
        }
        else if (playerStat == ps.maxMana && oldMax > 0)
        {
            ps.currentMana = (int)newMax;
            ps.NotifyManaChanged();
        }

        ps.CalculatePower();
        ps.NotifyStatsChanged();
        dataAsset.Save();
        QuestManager.Instance?.ReportProgressByObjectiveName("UpgradeStat", times);
        RefreshUI();

        if (animTarget != null)
        {
            animTarget.DOKill();
            animTarget.localScale = Vector3.one;
            animTarget.DOPunchScale(Vector3.one * 0.25f, 0.4f, 8, 0.5f);
        }
    }

    private void ShowInsufficientGoldToastLimited()
    {
        float now = Time.unscaledTime;

        if (now - lastInsufficientGoldTime > insufficientGoldStreakResetGap)
            insufficientGoldStreak = 0;

        lastInsufficientGoldTime = now;
        insufficientGoldStreak++;

        if (insufficientGoldStreak <= maxInsufficientGoldToasts)
            GameEvents.OnShowToast.Raise(T("not_enough_gold", "Not enough Gold"));
    }

    private bool CanUpgradeStat(
        PlayerStatData stat,
        int times,
        out PlayerStatData requiredStat,
        out int requiredLevel,
        out int currentRequiredLevel)
    {
        requiredLevel = 0;
        currentRequiredLevel = 0;
        requiredStat = GetRequiredPreviousStat(stat);
        if (requiredStat == null)
            return true;

        int nextLevel = dataAsset.GetSavedLevel(stat) + Mathf.Max(1, times);
        requiredLevel = GetRequiredStatValue(stat, nextLevel);
        currentRequiredLevel = Mathf.FloorToInt(GetCurrentRequirementValue(requiredStat));

        return currentRequiredLevel >= requiredLevel;
    }

    private PlayerStatData GetRequiredPreviousStat(PlayerStatData stat)
    {
        if (stat == data.crit)        return data.attack;
        if (stat == data.lifesteal)   return data.health;
        if (stat == data.attackSpeed) return data.attack;
        if (stat == data.speed)       return data.defense;

        return null;
    }

    private int GetRequiredStatValue(PlayerStatData stat, int nextLevel)
    {
        int safeLevel = Mathf.Max(1, nextLevel);

        if (stat == data.crit)        return safeLevel * 50;
        if (stat == data.lifesteal)   return safeLevel * 100;
        if (stat == data.attackSpeed) return safeLevel * 40;
        if (stat == data.speed)       return safeLevel * 10;

        return safeLevel * Mathf.Max(1, unlockStep);
    }

    private float GetCurrentRequirementValue(PlayerStatData requiredStat)
    {
        PlayerStats ps = PlayerStats.Instance;
        if (ps == null)
            return requiredStat.GetValueAtLevel(dataAsset.GetSavedLevel(requiredStat));

        if (requiredStat == data.attack)      return ps.attack.Value;
        if (requiredStat == data.defense)     return ps.defense.Value;
        if (requiredStat == data.health)      return ps.maxHealth.Value;
        if (requiredStat == data.mana)        return ps.maxMana.Value;
        if (requiredStat == data.speed)       return ps.speed.Value;
        if (requiredStat == data.attackSpeed) return ps.attackSpeed.Value;
        if (requiredStat == data.crit)        return ps.critChance.Value;
        if (requiredStat == data.lifesteal)   return ps.lifeSteal.Value;

        return requiredStat.GetValueAtLevel(dataAsset.GetSavedLevel(requiredStat));
    }

    private string GetPreviousStatRequiredText(PlayerStatData requiredStat, int requiredLevel, int currentRequiredLevel)
    {
        string requiredName = GetStatDisplayName(requiredStat);
        string message = IsVietnamese()
            ? "Y\u00eau c\u1ea7u ch\u1ec9 s\u1ed1"
            : "Requires stat";

        return string.IsNullOrEmpty(requiredName)
            ? message
            : $"{message}: {requiredName} {requiredLevel} ({currentRequiredLevel}/{requiredLevel})";
    }

    private string GetShortRequirementText(PlayerStatData requiredStat, int requiredLevel)
    {
        string requiredName = GetStatDisplayName(requiredStat);
        return string.IsNullOrEmpty(requiredName)
            ? T("locked", "Locked")
            : FormatRequiresText(requiredName, requiredLevel);
    }

    private void RefreshRequirementPopups()
    {
        RefreshRequirementPopup(data.crit, critRequirementPopup, critRequirementText);
        RefreshRequirementPopup(data.lifesteal, lifestealRequirementPopup, lifestealRequirementText);
        RefreshRequirementPopup(data.attackSpeed, attackSpeedRequirementPopup, attackSpeedRequirementText);
        RefreshRequirementPopup(data.speed, speedRequirementPopup, speedRequirementText);
    }

    private void RefreshRequirementPopup(PlayerStatData stat, GameObject popup, TextMeshProUGUI text)
    {
        if (popup == null) return;

        bool unlocked = CanUpgradeStat(stat, currentMultiplier, out PlayerStatData requiredStat, out int requiredLevel, out int currentRequiredLevel);
        popup.SetActive(!unlocked);

        if (!unlocked && text != null)
            text.text = GetButtonRequirementText(requiredStat, requiredLevel);
    }

    private void EnsureRequirementPopups()
    {
        if (!autoCreateRequirementPopups) return;

        EnsureRequirementPopup(ref critRequirementPopup, ref critRequirementText, critPreview, critCostText);
        EnsureRequirementPopup(ref lifestealRequirementPopup, ref lifestealRequirementText, lifestealPreview, lifestealCostText);
        EnsureRequirementPopup(ref attackSpeedRequirementPopup, ref attackSpeedRequirementText, attackSpeedPreview, attackSpeedCostText);
        EnsureRequirementPopup(ref speedRequirementPopup, ref speedRequirementText, speedPreview, speedCostText);
    }

    private void EnsureRequirementPopup(
        ref GameObject popup,
        ref TextMeshProUGUI popupText,
        TextMeshProUGUI previewText,
        TextMeshProUGUI costText)
    {
        if (popup != null) return;

        Transform buttonRoot = costText != null ? costText.transform.parent : previewText?.transform.parent;
        if (buttonRoot == null) return;

        popup = new GameObject("UpgradeLockBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(Shadow));
        popup.transform.SetParent(buttonRoot, false);
        popup.transform.SetAsLastSibling();

        RectTransform rect = popup.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = new Vector2(8f, 8f);
        rect.offsetMax = new Vector2(-8f, -8f);

        Image image = popup.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.96f);
        image.raycastTarget = false;

        Outline outline = popup.GetComponent<Outline>();
        outline.effectColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow[] shadows = popup.GetComponents<Shadow>();
        Shadow shadow = shadows[shadows.Length - 1];
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(0f, -6f);

        GameObject textObject = new GameObject("RequirementText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(popup.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.06f, 0.08f);
        textRect.anchorMax = new Vector2(0.94f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        popupText = textObject.GetComponent<TextMeshProUGUI>();
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.enableAutoSizing = true;
        popupText.fontSizeMin = 10f;
        popupText.fontSizeMax = 24f;
        popupText.color = new Color(0.95f, 0.08f, 0.06f, 1f);
        popupText.raycastTarget = false;

        popup.SetActive(false);
    }

    private string GetStatDisplayName(PlayerStatData stat)
    {
        if (stat == data.attack)      return T("attack", "Attack");
        if (stat == data.defense)     return T("defense", "Defense");
        if (stat == data.health)      return T("hp", "HP");
        if (stat == data.mana)        return T("mana", "Mana");
        if (stat == data.speed)       return T("speed", "Speed");
        if (stat == data.attackSpeed) return T("attack_speed", "Attack Speed");
        if (stat == data.crit)        return T("crit", "Critical");
        if (stat == data.lifesteal)   return T("life_steal", "Life Steal");

        return "";
    }

    private string GetButtonRequirementText(PlayerStatData requiredStat, int requiredLevel)
    {
        string requiredName = GetStatDisplayName(requiredStat);
        string lockedText = T("locked", "Locked");
        return string.IsNullOrEmpty(requiredName)
            ? lockedText
            : $"{lockedText.ToUpperInvariant()}\n{FormatRequiresText(requiredName, requiredLevel)}";
    }

    private string FormatRequiresText(string requiredName, int requiredLevel)
    {
        return IsVietnamese()
            ? $"Y\u00eau c\u1ea7u {requiredName} {requiredLevel}"
            : $"Requires {requiredName} {requiredLevel}";
    }

    private string T(string key, string fallback)
    {
        return LanguageManager.Instance != null ? LanguageManager.Instance.GetTranslation(key) : fallback;
    }

    private bool IsVietnamese()
    {
        return LanguageManager.Instance != null && LanguageManager.Instance.CurrentLanguage == "vi";
    }

    public void OnClickX1()   => SetMultiplier(1);
    public void OnClickX10()  => SetMultiplier(10);
    public void OnClickX100() => SetMultiplier(100);
    public void UpgradeAttack()      => TryUpgrade(data.attack,      ps => ps.attack,      currentMultiplier, attackPreview?.transform);
    public void UpgradeDefense()     => TryUpgrade(data.defense,     ps => ps.defense,     currentMultiplier, defensePreview?.transform);
    public void UpgradeSpeed()       => TryUpgrade(data.speed,       ps => ps.speed,       currentMultiplier, speedPreview?.transform);
    public void UpgradeCrit()        => TryUpgrade(data.crit,        ps => ps.critChance,  currentMultiplier, critPreview?.transform);
    public void UpgradeLifesteal()   => TryUpgrade(data.lifesteal,   ps => ps.lifeSteal,   currentMultiplier, lifestealPreview?.transform);
    public void UpgradeAttackSpeed() => TryUpgrade(data.attackSpeed, ps => ps.attackSpeed, currentMultiplier, attackSpeedPreview?.transform);
    public void UpgradeHealth()      => TryUpgrade(data.health,      ps => ps.maxHealth,   currentMultiplier, healthPreview?.transform);
    public void UpgradeMana()        => TryUpgrade(data.mana,        ps => ps.maxMana,     currentMultiplier, manaPreview?.transform);
    public void Close() => UIManager.Instance.HidePopupByType(PopupType.Stats);
}
