#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WaveInfoUIPrefabCreator
{
    private const string PrefabPath = "Assets/Prefab/Ui/UiMain/WaveInfoUI.prefab";

    [MenuItem("Tools/Idle Auto Battle/Create Wave Info UI Prefab")]
    public static void CreateWaveInfoPrefab()
    {
        EnsureFolder("Assets/Prefab");
        EnsureFolder("Assets/Prefab/Ui");
        EnsureFolder("Assets/Prefab/Ui/UiMain");

        GameObject root = new GameObject("WaveInfoUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(460f, 140f);
        rootRect.anchoredPosition = new Vector2(0f, -20f);

        Image bg = root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.35f);

        WaveInfoUI ui = root.AddComponent<WaveInfoUI>();

        TextMeshProUGUI waveText = CreateText("WaveText", root.transform, new Vector2(0f, -18f), 38, FontStyles.Bold);
        waveText.text = "Wave: 1";

        TextMeshProUGUI stageText = CreateText("StageText", root.transform, new Vector2(0f, -62f), 30, FontStyles.Bold);
        stageText.text = "Stage: 1";

        TextMeshProUGUI bannerText = CreateText("WaveBannerText", root.transform, new Vector2(0f, -108f), 34, FontStyles.Bold);
        bannerText.text = "WAVE 1";
        bannerText.color = new Color(1f, 0.9f, 0.3f);
        bannerText.gameObject.SetActive(false);

        SerializedObject so = new SerializedObject(ui);
        so.FindProperty("waveText").objectReferenceValue = waveText;
        so.FindProperty("stageText").objectReferenceValue = stageText;
        so.FindProperty("waveBannerText").objectReferenceValue = bannerText;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = prefab;
        Debug.Log($"Created prefab at {PrefabPath}");
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPos, int size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(430f, 36f);
        rect.anchoredPosition = anchoredPos;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.color = Color.white;

        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;

        return tmp;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
