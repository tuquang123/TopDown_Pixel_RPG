using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Create New Dialogue")]
public class Dialogue : ScriptableObject
{
    public DialogueLine[] lines;
    public string id;
}

[System.Serializable]
public class DialogueLine
{
    [Tooltip("Localization key for speakerName. Example: npc.blacksmith.name")]
    public string speakerKey;
    public string speakerName;

    [Tooltip("Localization key for sentence. Example: dialogue.blacksmith.intro.001")]
    public string sentenceKey;

    [TextArea(2, 5)]
    public string sentence;

    public string GetSpeakerText()
    {
        return Localize(speakerKey, speakerName);
    }

    public string GetSentenceText(string dialogueId, int lineIndex)
    {
        string key = sentenceKey;
        if (string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(dialogueId))
            key = $"dialogue.{dialogueId}.{lineIndex + 1:000}";

        return Localize(key, sentence);
    }

    private static string Localize(string key, string fallback)
    {
        return LanguageManager.Instance != null
            ? LanguageManager.Instance.GetTranslationOrFallback(key, fallback)
            : fallback;
    }
}
