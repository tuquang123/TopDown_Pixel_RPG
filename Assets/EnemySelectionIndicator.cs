using UnityEngine;

public class EnemySelectionIndicator : MonoBehaviour
{
    public void Show(bool value)
    {
        gameObject.SetActive(value);
    }
}
