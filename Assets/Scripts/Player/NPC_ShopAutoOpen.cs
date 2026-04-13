using UnityEngine;

public class NPC_ShopAutoOpen : MonoBehaviour
{
    [Header("Cài đặt Shop")]
    public PopupType shopType = PopupType.Shop;

    [Header("Tùy chọn")]
    [Tooltip("Tự động đóng Shop khi Player đi ra khỏi vòng")]
    public bool closeWhenExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player vào vòng → Mở Shop");
            UIManager.Instance.ShowPopupByType(shopType);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && closeWhenExit)
        {
            Debug.Log("Player ra khỏi vòng → Đóng Shop");
            UIManager.Instance.HidePopupByType(shopType);
        }
    }
}