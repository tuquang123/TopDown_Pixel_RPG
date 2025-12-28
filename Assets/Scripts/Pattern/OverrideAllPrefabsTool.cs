#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;

#if UNITY_EDITOR
public class OverrideAllPrefabsTool : EditorWindow
{
    [MenuItem("Tools/Prefab/Override All Prefabs In Scene")]
    static void Open()
    {
        GetWindow<OverrideAllPrefabsTool>("Override Prefabs");
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Override ALL prefab instances in current scene.\n" +
            "⚠ This will apply current instance values as overrides.",
            MessageType.Warning
        );

        GUILayout.Space(10);

        if (GUILayout.Button("OVERRIDE ALL PREFABS", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                    "Confirm Override",
                    "Are you sure you want to override ALL prefab instances in the scene?",
                    "Yes, Override",
                    "Cancel"))
            {
                OverrideAllPrefabs();
            }
        }
    }

    static void OverrideAllPrefabs()
    {
        int count = 0;

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in allObjects)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                continue;

            GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (root == null) continue;

            PrefabUtility.ApplyPrefabInstance(
                root,
                InteractionMode.AutomatedAction
            );

            count++;
        }

        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"✅ Overridden {count} prefab instances in scene.");
    }
}
#endif