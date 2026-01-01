using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class OverrideAllPrefabsTool : EditorWindow
{
    private Vector2 scroll;
    private List<PrefabItem> prefabItems = new();

    [MenuItem("Tools/Prefab/Override Prefabs In Scene")]
    public static void Open()
    {
        GetWindow<OverrideAllPrefabsTool>("Override Prefabs");
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        prefabItems.Clear();

        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (var go in allObjects)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                continue;

            var root = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (root == null)
                continue;

            if (prefabItems.Exists(p => p.instanceRoot == root))
                continue;

            if (!PrefabUtility.HasPrefabInstanceAnyOverrides(root, false))
                continue;

            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);

            prefabItems.Add(new PrefabItem
            {
                instanceRoot = root,
                prefabPath = path,
                isSelected = true
            });
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(5);

        if (GUILayout.Button("🔄 Refresh List", GUILayout.Height(30)))
        {
            Refresh();
        }

        GUILayout.Space(5);

        EditorGUILayout.LabelField($"Found {prefabItems.Count} prefab(s) with overrides", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var item in prefabItems)
        {
            EditorGUILayout.BeginHorizontal("box");

            item.isSelected = EditorGUILayout.Toggle(item.isSelected, GUILayout.Width(20));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(item.instanceRoot.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(item.prefabPath, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = item.instanceRoot;
                EditorGUIUtility.PingObject(item.instanceRoot);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        GUI.enabled = prefabItems.Exists(p => p.isSelected);

        if (GUILayout.Button("✅ Apply Selected Prefabs", GUILayout.Height(35)))
        {
            ApplySelected();
        }

        GUI.enabled = true;
    }

    private void ApplySelected()
    {
        int count = 0;

        foreach (var item in prefabItems)
        {
            if (!item.isSelected)
                continue;

            Object prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(item.instanceRoot);
            if (prefabAsset == null)
                continue;

            Undo.RegisterCompleteObjectUndo(prefabAsset, "Override Prefab");

            PrefabUtility.ApplyPrefabInstance(
                item.instanceRoot,
                InteractionMode.UserAction
            );

            count++;
        }

        Debug.Log($"[Override Tool] Applied {count} prefab(s)");
        Refresh();
    }

    private class PrefabItem
    {
        public GameObject instanceRoot;
        public string prefabPath;
        public bool isSelected;
    }
}
