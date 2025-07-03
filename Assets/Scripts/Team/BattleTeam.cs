using System;
using UnityEngine;

[Serializable]
public class BattleTeam
{
    public Hero[] teamSlots = new Hero[4]; // 4 Slot đội hình
    
    public void ResetTeam()
    {
        for (int i = 0; i < teamSlots.Length; i++)
        {
            teamSlots[i] = null;
        }
    }

    // Thêm Hero vào Slot chỉ định
    public void AddHeroToSlot(Hero hero, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= teamSlots.Length)
        {
            Debug.LogWarning("Slot không hợp lệ");
            return;
        }

        teamSlots[slotIndex] = hero;
    }

    // Xóa Hero khỏi Slot
    public void RemoveHeroFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= teamSlots.Length)
        {
            Debug.LogWarning("Slot không hợp lệ");
            return;
        }

        teamSlots[slotIndex] = null;
    }

    // Kiểm tra Hero đã có trong team chưa
    public bool IsHeroInTeam(Hero hero)
    {
        foreach (var h in teamSlots)
        {
            if (h == hero)
                return true;
        }
        return false;
    }

    // Lấy slot trống đầu tiên
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < teamSlots.Length; i++)
        {
            if (teamSlots[i] == null)
                return i;
        }
        return -1; // Full slot
    }
}