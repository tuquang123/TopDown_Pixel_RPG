#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EnemyAnimationEventReceiverAutoFix
{
    private static readonly string[] PrefabRoots =
    {
        "Assets/Prefab/Enemy",
        "Assets/Prefab/IdleBattlers"
    };

    static EnemyAnimationEventReceiverAutoFix()
    {
        EditorApplication.delayCall += FixAllEnemyPrefabs;
    }

    [MenuItem("Tools/Enemy/Fix Animation Event Receivers")]
    public static void FixAllEnemyPrefabs()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", PrefabRoots))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FixPrefab(path);
        }
    }

    private static void FixPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null || prefab.GetComponentInChildren<EnemyAI>(true) == null)
            return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        bool changed = false;

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            if (animator == null || animator.GetComponent<PlayerAnimationEvents>() != null)
                continue;

            animator.gameObject.AddComponent<PlayerAnimationEvents>();
            changed = true;
        }

        if (changed)
            PrefabUtility.SaveAsPrefabAsset(root, path);

        PrefabUtility.UnloadPrefabContents(root);
    }
}

public class EnemyAnimationEventReceiverPrefabPostprocessor : AssetPostprocessor
{
    private static bool queued;

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (queued)
            return;

        if (!ContainsEnemyPrefab(importedAssets) && !ContainsEnemyPrefab(movedAssets))
            return;

        queued = true;
        EditorApplication.delayCall += () =>
        {
            queued = false;
            EnemyAnimationEventReceiverAutoFix.FixAllEnemyPrefabs();
        };
    }

    private static bool ContainsEnemyPrefab(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            if (!path.EndsWith(".prefab"))
                continue;

            if (path.StartsWith("Assets/Prefab/Enemy") || path.StartsWith("Assets/Prefab/IdleBattlers"))
                return true;
        }

        return false;
    }
}
#endif
