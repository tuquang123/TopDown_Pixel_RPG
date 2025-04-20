using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Create New Dialogue")]
public class Dialogue : ScriptableObject {
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine {
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;
}
