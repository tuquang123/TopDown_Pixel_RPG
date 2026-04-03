using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SODataManager : MonoBehaviour
{
    [Title("Scriptable Object Data Manager")]
    [InfoBox("Kéo thả ScriptableObject vào đây để quản lý")]

    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, Expanded = true)]
    private List<ScriptableObject> allScriptableObjects = new List<ScriptableObject>();

    // =========================================================
    // EXPORT CSV
    // =========================================================
    [Button("📤 Export CSV", ButtonSizes.Large)]
    private void ExportCSV()
    {
#if UNITY_EDITOR
        if (allScriptableObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không có dữ liệu!", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export CSV", "", "SOData", "csv");
        if (string.IsNullOrEmpty(path)) return;

        using (var writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            var grouped = allScriptableObjects.GroupBy(x => x.GetType().Name);

            foreach (var group in grouped)
            {
                writer.WriteLine($"#TYPE:{group.Key}");
                writer.WriteLine($"#COUNT:{group.Count()}");

                var entries = GetFields(group.First().GetType());
                entries.Insert(0, new FieldEntry { fullName = "AssetName", isSpecial = true });

                writer.WriteLine(string.Join(",", entries.Select(e => $"\"{e.fullName}\"")));

                foreach (var obj in group)
                {
                    List<string> row = new List<string>();

                    foreach (var e in entries)
                    {
                        if (e.isSpecial)
                            row.Add(Escape(obj.name));
                        else
                            row.Add(Escape(ValueToString(e.GetValue(obj), e.isList)));
                    }

                    writer.WriteLine(string.Join(",", row));
                }

                writer.WriteLine();
            }
        }

        EditorUtility.DisplayDialog("Done", "Export CSV thành công!", "OK");
#endif
    }

    // =========================================================
    // IMPORT CSV
    // =========================================================
    [Button("📥 Import CSV", ButtonSizes.Large)]
    private void ImportCSV()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        var lines = File.ReadAllLines(path, Encoding.UTF8);
        int index = 0;
        int count = 0;

        while (index < lines.Length)
        {
            string line = lines[index];

            if (!line.StartsWith("#TYPE:"))
            {
                index++;
                continue;
            }

            string typeName = line.Replace("#TYPE:", "");
            index++;

            if (lines[index].StartsWith("#COUNT:")) index++;

            var headers = ParseLine(lines[index]);
            index++;

            var targets = allScriptableObjects.Where(x => x.GetType().Name == typeName).ToList();
            if (targets.Count == 0) continue;

            var entries = GetFields(targets[0].GetType());

            var headerMap = new Dictionary<string, int>();
            for (int i = 0; i < headers.Length; i++)
                headerMap[headers[i]] = i;

            while (index < lines.Length && !string.IsNullOrEmpty(lines[index]))
            {
                var values = ParseLine(lines[index]);

                string assetName = values[headerMap["AssetName"]];
                var target = targets.FirstOrDefault(x => x.name == assetName);

                if (target != null)
                {
                    foreach (var e in entries)
                    {
                        if (headerMap.TryGetValue(e.fullName, out int col))
                        {
                            var val = Parse(values[col], e.fieldInfo.FieldType, e.isList);
                            if (val != null)
                                e.SetValue(target, val);
                        }
                    }

                    EditorUtility.SetDirty(target);
                    count++;
                }

                index++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Import xong {count} object", "OK");
#endif
    }

    // =========================================================
    // EXPORT JSON
    // =========================================================
    [Button("📁 Export JSON", ButtonSizes.Large)]
    private void ExportJSON()
    {
#if UNITY_EDITOR
        string folder = EditorUtility.SaveFolderPanel("Export JSON", "", "SOData");
        if (string.IsNullOrEmpty(folder)) return;

        foreach (var so in allScriptableObjects)
        {
            string json = JsonUtility.ToJson(so, true);
            string path = Path.Combine(folder, $"{so.GetType().Name}_{so.name}.json");
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        EditorUtility.DisplayDialog("Done", "Export JSON xong!", "OK");
#endif
    }

    // =========================================================
    // IMPORT JSON
    // =========================================================
    [Button("📁 Import JSON", ButtonSizes.Large)]
    private void ImportJSON()
    {
#if UNITY_EDITOR
        string folder = EditorUtility.OpenFolderPanel("Import JSON", "", "");
        if (string.IsNullOrEmpty(folder)) return;

        var files = Directory.GetFiles(folder, "*.json");

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            var parts = name.Split('_');

            if (parts.Length < 2) continue;

            string type = parts[0];
            string assetName = string.Join("_", parts.Skip(1));

            var target = allScriptableObjects.FirstOrDefault(x =>
                x.GetType().Name == type && x.name == assetName);

            if (target != null)
            {
                string json = File.ReadAllText(file);
                JsonUtility.FromJsonOverwrite(json, target);
                EditorUtility.SetDirty(target);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", "Import JSON xong!", "OK");
#endif
    }

    // =========================================================
    // DEBUG
    // =========================================================
    [Button("🔍 Debug Fields", ButtonSizes.Medium)]
    private void DebugFields()
    {
#if UNITY_EDITOR
        if (allScriptableObjects.Count == 0)
        {
            Debug.Log("No objects");
            return;
        }

        var obj = allScriptableObjects[0];
        var type = obj.GetType();

        Debug.Log($"=== Debugging: {type.Name} ===");

        var allFields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Debug.Log($"Total fields found: {allFields.Length}");

        foreach (var f in allFields)
        {
            bool isPublic = f.IsPublic;
            bool hasSerializeField = f.GetCustomAttribute<SerializeField>() != null;
            bool isSimple = IsSimple(f.FieldType);
            bool isList = IsListOrArray(f.FieldType);

            Debug.Log($"  • {f.Name} ({f.FieldType.Name}) - Public:{isPublic} Serialize:{hasSerializeField} Simple:{isSimple} List:{isList}");
        }

        var detected = GetFields(type);
        Debug.Log($"\nDetected by GetFields: {detected.Count}");
        foreach (var e in detected)
        {
            Debug.Log($"  ✓ {e.fullName} (List:{e.isList})");
        }
#endif
    }

    // =========================================================
    // HELPER CLASSES
    // =========================================================

    private class FieldEntry
    {
        public string fullName;
        public FieldInfo fieldInfo;
        public bool isSpecial;
        public bool isList;

        public object GetValue(object obj) => isSpecial ? null : fieldInfo?.GetValue(obj);
        public void SetValue(object obj, object val)
        {
            if (!isSpecial) fieldInfo?.SetValue(obj, val);
        }
    }

    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> items;
    }

    [Serializable]
    private class ArrayWrapper<T>
    {
        public T[] items;
    }

    // =========================================================
    // FIELD DETECTION
    // =========================================================

    private List<FieldEntry> GetFields(Type type, string prefix = "")
    {
        var list = new List<FieldEntry>();

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);

        foreach (var f in fields)
        {
            Type t = f.FieldType;

            // Check if List<T> or Array
            if (IsListOrArray(t))
            {
                list.Add(new FieldEntry
                {
                    fullName = prefix + f.Name,
                    fieldInfo = f,
                    isList = true
                });
            }
            // Simple types
            else if (IsSimple(t))
            {
                list.Add(new FieldEntry
                {
                    fullName = prefix + f.Name,
                    fieldInfo = f,
                    isList = false
                });
            }
            // Nested serializable class
            else if (t.IsClass && !t.IsArray && t.GetCustomAttribute<SerializableAttribute>() != null)
            {
                list.AddRange(GetFields(t, prefix + f.Name + "_"));
            }
        }

        return list;
    }

    private bool IsListOrArray(Type t)
    {
        if (t.IsArray) return true;
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) return true;
        return false;
    }

    private bool IsSimple(Type t)
    {
        return t.IsPrimitive || t == typeof(string) || t.IsEnum ||
               t == typeof(Vector2) || t == typeof(Vector3) ||
               t == typeof(Vector4) || t == typeof(Color) ||
               t == typeof(Rect) || t == typeof(Quaternion) ||
               typeof(UnityEngine.Object).IsAssignableFrom(t);
    }

    // =========================================================
    // VALUE CONVERSION
    // =========================================================

    private string ValueToString(object val, bool isList)
    {
#if UNITY_EDITOR
        if (val == null) return "";

        // Handle List/Array as JSON
        if (isList)
        {
            string json = JsonUtility.ToJson(new { items = val }, false);
            // Remove wrapper: {"items":[...]} -> [...]
            int start = json.IndexOf('[');
            int end = json.LastIndexOf(']');
            if (start >= 0 && end > start)
                return json.Substring(start, end - start + 1);
            return json;
        }

        if (val is UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return AssetDatabase.AssetPathToGUID(path);
        }

        if (val is Vector3 v3) return $"{v3.x},{v3.y},{v3.z}";
        if (val is Vector2 v2) return $"{v2.x},{v2.y}";
        if (val is Color c) return ColorUtility.ToHtmlStringRGBA(c);

        return val.ToString();
#else
        return "";
#endif
    }

    private object Parse(string str, Type t, bool isList)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(str)) return null;

        // Handle List/Array from JSON
        if (isList)
        {
            try
            {
                // Wrap: [...] -> {"items":[...]}
                string wrapped = $"{{\"items\":{str}}}";
                
                if (t.IsArray)
                {
                    Type elemType = t.GetElementType();
                    Type wrapperType = typeof(ArrayWrapper<>).MakeGenericType(elemType);
                    var wrapper = JsonUtility.FromJson(wrapped, wrapperType);
                    var itemsField = wrapperType.GetField("items");
                    return itemsField.GetValue(wrapper);
                }
                else if (t.IsGenericType)
                {
                    Type elemType = t.GetGenericArguments()[0];
                    Type wrapperType = typeof(ListWrapper<>).MakeGenericType(elemType);
                    var wrapper = JsonUtility.FromJson(wrapped, wrapperType);
                    var itemsField = wrapperType.GetField("items");
                    return itemsField.GetValue(wrapper);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Parse List/Array failed: {e.Message}");
                return null;
            }
        }

        // Simple types
        if (t == typeof(string)) return str;
        if (t == typeof(int)) return int.Parse(str);
        if (t == typeof(float)) return float.Parse(str);
        if (t == typeof(bool)) return bool.Parse(str);
        if (t.IsEnum) return Enum.Parse(t, str);

        if (t == typeof(Vector3))
        {
            var p = str.Split(',');
            return new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
        }

        if (t == typeof(Vector2))
        {
            var p = str.Split(',');
            return new Vector2(float.Parse(p[0]), float.Parse(p[1]));
        }

        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
        {
            string path = AssetDatabase.GUIDToAssetPath(str);
            return AssetDatabase.LoadAssetAtPath(path, t);
        }
#endif
        return null;
    }

    // =========================================================
    // CSV PARSING
    // =========================================================

    private string Escape(string v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        if (v.Contains(",") || v.Contains("\"") || v.Contains("\n"))
        {
            v = v.Replace("\"", "\"\"");
            return $"\"{v}\"";
        }
        return v;
    }

    private string[] ParseLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuote = false;
        StringBuilder cur = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Check for escaped quote ""
                if (inQuote && i + 1 < line.Length && line[i + 1] == '"')
                {
                    cur.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuote = !inQuote;
                }
            }
            else if (c == ',' && !inQuote)
            {
                result.Add(cur.ToString());
                cur.Clear();
            }
            else
            {
                cur.Append(c);
            }
        }

        result.Add(cur.ToString());
        return result.ToArray();
    }
}