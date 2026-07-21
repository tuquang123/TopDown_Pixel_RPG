using UnityEngine;

public static class RewardGrantUtility
{
    public static bool HasAnyReward(QuestReward reward)
    {
        if (reward == null) return false;

        return reward.experienceReward > 0
            || reward.goldReward > 0
            || reward.gemReward > 0
            || (reward.itemIDs != null && reward.itemIDs.Count > 0)
            || (reward.rewardItem != null && reward.rewardItem.itemData != null);
    }

    public static void Grant(QuestReward reward, Transform source = null)
    {
        if (reward == null) return;

        int exp = reward.experienceReward;
        int gold = reward.goldReward;
        int gems = reward.gemReward;

        if (exp > 0 && PlayerStats.Instance != null)
        {
            var playerLevel = PlayerStats.Instance.GetComponent<PlayerLevel>();
            if (playerLevel != null)
            {
                playerLevel.levelSystem.AddExp(exp);
                if (FloatingTextSpawner.Instance != null)
                    FloatingTextSpawner.Instance.SpawnText("+ EXP :" + exp, GetRewardPosition(source), Color.magenta);
            }
        }

        if (gold > 0)
            CurrencyManager.Instance?.AddGold(gold);

        if (gems > 0)
            CurrencyManager.Instance?.AddGems(gems);

        if (exp > 0 && RewardPopupManager.Instance != null && CommonReferent.Instance != null)
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconExp, "EXP", exp);

        if (gold > 0 && RewardPopupManager.Instance != null && CommonReferent.Instance != null)
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconGold, "Gold", gold);

        if (gems > 0 && RewardPopupManager.Instance != null && CommonReferent.Instance != null)
            RewardPopupManager.Instance.ShowReward(CommonReferent.Instance.iconGem, "Gem", gems);

        if (reward.itemIDs != null)
        {
            foreach (var itemID in reward.itemIDs)
            {
                ItemData itemData = CommonReferent.Instance?.itemDatabase?.GetItemByID(itemID);
                if (itemData == null)
                {
                    Debug.LogWarning($"Item ID does not exist: {itemID}");
                    continue;
                }

                GrantItem(new ItemInstance(itemData));
            }
        }

        if (reward.rewardItem != null && reward.rewardItem.itemData != null)
        {
            GrantItem(new ItemInstance(
                reward.rewardItem.itemData,
                reward.rewardItem.upgradeLevel,
                locked: reward.rewardItem.isLocked
            ));
        }
    }

    private static Vector3 GetRewardPosition(Transform source)
    {
        if (source != null)
            return source.position;

        if (PlayerController.Instance != null)
            return PlayerController.Instance.transform.position;

        return Vector3.zero;
    }

    private static void GrantItem(ItemInstance itemInstance)
    {
        if (itemInstance?.itemData == null)
            return;

        Inventory.Instance?.AddItem(itemInstance);

        if (RewardPopupManager.Instance != null)
            RewardPopupManager.Instance.ShowReward(itemInstance.itemData.icon, itemInstance.itemData.itemName, 1);

        Debug.Log($"Received reward item: {itemInstance.itemData.itemName}");
    }
}
