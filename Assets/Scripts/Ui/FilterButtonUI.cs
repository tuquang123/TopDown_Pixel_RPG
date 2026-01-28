using UnityEngine;

public class FilterButtonUI : MonoBehaviour
{
    [Header("BG")]
     // BG thường (có thể bỏ nếu không cần)
    public GameObject selectedBG;  // BG highlight

    private void Awake()
    {
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
      

        if (selectedBG != null)
            selectedBG.SetActive(selected);
    }
}