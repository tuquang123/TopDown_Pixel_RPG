// LocalUIManager.cs
using UnityEngine;

public class LocalUIManager : MonoBehaviour
{
    [Header("Joystick Prefab")]
    public UltimateJoystick joystickPrefab; // assign prefab trên inspector

    [HideInInspector] public UltimateJoystick joystickInstance;

    private void Awake()
    {
        // Chỉ spawn 1 joystick cho local player
        if (joystickPrefab != null && joystickInstance == null)
        {
            joystickInstance = Instantiate(joystickPrefab, transform);
            joystickInstance.name = "Joystick_Local";
        }
        else if (joystickPrefab == null)
        {
            Debug.LogWarning("Joystick Prefab chưa gán trên LocalUIManager!");
        }
    }
}
