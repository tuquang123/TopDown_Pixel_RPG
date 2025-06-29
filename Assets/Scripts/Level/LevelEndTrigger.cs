using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Đến điểm cuối màn!");
            LevelManager.Instance.NextLevel(); 
        }
    }
}