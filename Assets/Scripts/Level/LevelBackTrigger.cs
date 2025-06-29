using UnityEngine;

public class LevelBackTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Quay lại màn trước!");
            LevelManager.Instance.PreviousLevel();
        }
    }
}