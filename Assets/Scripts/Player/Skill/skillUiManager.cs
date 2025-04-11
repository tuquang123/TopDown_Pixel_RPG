using UnityEngine;
using UnityEngine.UIElements;

public class SkillUIManager : MonoBehaviour
{
    private VisualElement root;
    private Button[] skillButtons;

    void OnEnable()
    {
        // Tải UXML và USS
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Lấy danh sách skill button
        skillButtons = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            skillButtons[i] = root.Q<Button>($"skill-slot-{i+1}");
            int index = i;
            skillButtons[i].clicked += () => AssignSkill(index);
        }
    }

    void AssignSkill(int slot)
    {
        Debug.Log($"Gán kỹ năng vào ô {slot + 1}");
        // Logic gán skill vào slot
    }
}