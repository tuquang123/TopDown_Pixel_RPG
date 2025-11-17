#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class EnemyAutoSetupEditor : MonoBehaviour
{
    [Button("Auto Setup Rigidbody, Collider, Layer & UnitRoot")]
    private void AutoAddRigidbodyAndCollider()
    {
        // Add Rigidbody2D nếu chưa có
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            Debug.Log("Rigidbody2D added and configured.");
        }

        // Add BoxCollider2D nếu chưa có
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("BoxCollider2D added.");
        }

        // Gán tag
        gameObject.tag = "Enemy";

        // Gán layer nếu có
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            gameObject.layer = enemyLayer;
        }

        // Thêm CommonAnimationEvents vào UnitRoot
        Transform unitRoot = transform.Find("UnitRoot");
        if (unitRoot != null)
        {
            if (unitRoot.GetComponent<CommonAnimationEvents>() == null)
            {
                unitRoot.gameObject.AddComponent<CommonAnimationEvents>();
            }
        }
        else
        {
            Debug.LogWarning("UnitRoot not found.");
        }

        Debug.Log("Enemy Auto Setup: DONE.");
    }
}
#endif