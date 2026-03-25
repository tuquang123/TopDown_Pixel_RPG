using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System;
using Sirenix.OdinInspector;

public class SODataManager : MonoBehaviour
{
    [Title("Scriptable Object Data Manager")]
    [InfoBox("Kéo thả tất cả ScriptableObject cần quản lý vào list bên dưới")]
    
    [SerializeField] 
    [ListDrawerSettings(ShowFoldout = true, Expanded = true)]
    private List<ScriptableObject> allScriptableObjects = new List<ScriptableObject>();

    [HorizontalGroup("Buttons", Width = 0.5f)]
    [Button("📤 Export to CSV", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void ExportToCSV()
    {
        if (allScriptableObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không có ScriptableObject nào để export!", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export to CSV", "", $"SOData_{DateTime.Now:yyyyMMdd_HHmmss}", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                var grouped = allScriptableObjects.GroupBy(so => so.GetType().Name);
                
                foreach (var group in grouped)
                {
                    // Ghi header cho từng loại
                    writer.WriteLine($"#TYPE:{group.Key}");
                    writer.WriteLine($"#COUNT:{group.Count()}");
                    
                    var entries = GetSerializableFieldEntries(group.First().GetType());
                    var allEntries = new List<FieldEntry> 
                    { 
                        new FieldEntry { fullName = "AssetName", fieldInfo = null, isSpecialField = true } 
                    };
                    allEntries.AddRange(entries);
                    
                    // Ghi header row
                    writer.WriteLine(string.Join(",", allEntries.Select(e => $"\"{e.fullName}\"")));
                    
                    // Ghi dữ liệu
                    foreach (var obj in group)
                    {
                        List<string> row = new List<string>();
                        foreach (var entry in allEntries)
                        {
                            if (entry.isSpecialField && entry.fullName == "AssetName")
                            {
                                row.Add(EscapeCSV(obj.name));
                            }
                            else
                            {
                                row.Add(EscapeCSV(ConvertValueToString(entry.GetValue(obj))));
                            }
                        }
                        writer.WriteLine(string.Join(",", row));
                    }
                    
                    writer.WriteLine(); // Dòng trống phân cách các type
                }
            }
            
            Debug.Log($"✅ Exported {allScriptableObjects.Count} objects to {path}");
            EditorUtility.DisplayDialog("Export Complete", $"Đã xuất {allScriptableObjects.Count} ScriptableObject\n\nFile: {path}\n\nCó thể mở bằng Excel để chỉnh sửa!", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Export failed: {ex.Message}");
            EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
        }
    }

    [HorizontalGroup("Buttons", Width = 0.5f)]
    [Button("📥 Import from CSV", ButtonSizes.Large)]
    [GUIColor(0.6f, 1f, 0.6f)]
    private void ImportFromCSV()
    {
        if (allScriptableObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không có ScriptableObject nào để import!", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel("Import from CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            int currentLine = 0;
            int importedCount = 0;
            
            while (currentLine < lines.Length)
            {
                string line = lines[currentLine].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    currentLine++;
                    continue;
                }
                
                // Kiểm tra header type
                if (line.StartsWith("#TYPE:"))
                {
                    string typeName = line.Substring(6);
                    currentLine++;
                    
                    // Đọc COUNT
                    if (currentLine < lines.Length && lines[currentLine].StartsWith("#COUNT:"))
                    {
                        currentLine++;
                    }
                    
                    // Đọc header row
                    if (currentLine >= lines.Length) break;
                    string[] headers = ParseCSVLine(lines[currentLine]);
                    currentLine++;
                    
                    var targetSOs = allScriptableObjects.Where(so => so.GetType().Name == typeName).ToList();
                    if (targetSOs.Count > 0)
                    {
                        var allEntries = GetSerializableFieldEntries(targetSOs[0].GetType());
                        var headerMap = new Dictionary<string, int>();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            string header = headers[i].Trim('"');
                            headerMap[header] = i;
                        }
                        
                        // Đọc data rows
                        while (currentLine < lines.Length && !string.IsNullOrEmpty(lines[currentLine].Trim()) && !lines[currentLine].StartsWith("#TYPE:"))
                        {
                            string[] values = ParseCSVLine(lines[currentLine]);
                            if (values.Length > 0)
                            {
                                string assetName = values[headerMap.GetValueOrDefault("AssetName", 0)].Trim('"');
                                var target = targetSOs.FirstOrDefault(so => so.name == assetName);
                                
                                if (target != null)
                                {
                                    foreach (var entry in allEntries)
                                    {
                                        if (headerMap.TryGetValue(entry.fullName, out int colIdx) && colIdx < values.Length)
                                        {
                                            string cellValue = values[colIdx].Trim('"');
                                            object parsedValue = ParseValueFromString(cellValue, entry.fieldInfo.FieldType);
                                            if (parsedValue != null)
                                            {
                                                entry.SetValue(target, parsedValue);
                                            }
                                        }
                                    }
                                    EditorUtility.SetDirty(target);
                                    importedCount++;
                                }
                            }
                            currentLine++;
                        }
                    }
                    else
                    {
                        // Skip to next type section
                        while (currentLine < lines.Length && !lines[currentLine].StartsWith("#TYPE:"))
                        {
                            currentLine++;
                        }
                    }
                }
                else
                {
                    currentLine++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Import completed! Updated {importedCount} objects from {path}");
            EditorUtility.DisplayDialog("Import Complete", $"Đã import và cập nhật {importedCount} ScriptableObject!", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Import failed: {ex.Message}");
            EditorUtility.DisplayDialog("Import Failed", ex.Message, "OK");
        }
    }

    [HorizontalGroup("Buttons", Width = 0.5f)]
    [Button("📁 Export to Folder (JSON)", ButtonSizes.Large)]
    [GUIColor(1f, 0.8f, 0.4f)]
    private void ExportToJSON()
    {
        if (allScriptableObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "Không có ScriptableObject nào để export!", "OK");
            return;
        }

        string folderPath = EditorUtility.SaveFolderPanel("Export to Folder", "", "SOData_Export");
        if (string.IsNullOrEmpty(folderPath)) return;

        try
        {
            int count = 0;
            foreach (var so in allScriptableObjects)
            {
                string json = JsonUtility.ToJson(so, true);
                string filePath = Path.Combine(folderPath, $"{so.GetType().Name}_{so.name}.json");
                File.WriteAllText(filePath, json, Encoding.UTF8);
                count++;
            }
            
            Debug.Log($"✅ Exported {count} objects to {folderPath}");
            EditorUtility.DisplayDialog("Export Complete", $"Đã xuất {count} ScriptableObject ra file JSON!\n\nFolder: {folderPath}", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Export failed: {ex.Message}");
            EditorUtility.DisplayDialog("Export Failed", ex.Message, "OK");
        }
    }

    [HorizontalGroup("Buttons", Width = 0.5f)]
    [Button("📁 Import from Folder (JSON)", ButtonSizes.Large)]
    [GUIColor(0.8f, 1f, 0.4f)]
    private void ImportFromJSON()
    {
        string folderPath = EditorUtility.OpenFolderPanel("Import from Folder", "", "");
        if (string.IsNullOrEmpty(folderPath)) return;

        try
        {
            var jsonFiles = Directory.GetFiles(folderPath, "*.json");
            int importedCount = 0;
            
            foreach (var filePath in jsonFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] nameParts = fileName.Split('_');
                if (nameParts.Length < 2) continue;
                
                string typeName = nameParts[0];
                string assetName = string.Join("_", nameParts.Skip(1));
                
                var target = allScriptableObjects.FirstOrDefault(so => so.GetType().Name == typeName && so.name == assetName);
                if (target != null)
                {
                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    JsonUtility.FromJsonOverwrite(json, target);
                    EditorUtility.SetDirty(target);
                    importedCount++;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"✅ Import completed! Updated {importedCount} objects");
            EditorUtility.DisplayDialog("Import Complete", $"Đã import và cập nhật {importedCount} ScriptableObject!", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Import failed: {ex.Message}");
            EditorUtility.DisplayDialog("Import Failed", ex.Message, "OK");
        }
    }

    // ============================================================
    // Helper Methods
    // ============================================================
    
    private class FieldEntry
    {
        public string fullName;
        public FieldInfo fieldInfo;
        public bool isSpecialField = false;
        public object GetValue(object obj) => isSpecialField ? null : fieldInfo?.GetValue(obj);
        public void SetValue(object obj, object val) { if (!isSpecialField) fieldInfo?.SetValue(obj, val); }
    }

    private List<FieldEntry> GetSerializableFieldEntries(Type type, string prefix = "")
    {
        var entries = new List<FieldEntry>();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null);
        
        foreach (var field in fields)
        {
            Type ft = field.FieldType;
            
            if (ft.IsPrimitive || ft == typeof(string) || ft.IsEnum || 
                ft == typeof(Vector2) || ft == typeof(Vector3) || ft == typeof(Vector4) ||
                ft == typeof(Color) || ft == typeof(Rect) || ft == typeof(Quaternion) ||
                typeof(UnityEngine.Object).IsAssignableFrom(ft))
            {
                entries.Add(new FieldEntry { fullName = prefix + field.Name, fieldInfo = field });
            }
            else if (ft.IsClass && !ft.IsArray)
            {
                entries.AddRange(GetSerializableFieldEntries(ft, prefix + field.Name + "_"));
            }
        }
        return entries;
    }

    private string ConvertValueToString(object value)
    {
        if (value == null) return "";
        
        Type t = value.GetType();
        if (t.IsEnum) return value.ToString();
        if (t == typeof(Vector2)) return $"{((Vector2)value).x},{((Vector2)value).y}";
        if (t == typeof(Vector3)) return $"{((Vector3)value).x},{((Vector3)value).y},{((Vector3)value).z}";
        if (t == typeof(Vector4)) return $"{((Vector4)value).x},{((Vector4)value).y},{((Vector4)value).z},{((Vector4)value).w}";
        if (t == typeof(Quaternion)) return $"{((Quaternion)value).x},{((Quaternion)value).y},{((Quaternion)value).z},{((Quaternion)value).w}";
        if (t == typeof(Color)) return ColorUtility.ToHtmlStringRGBA((Color)value);
        if (t == typeof(Rect)) return $"{((Rect)value).x},{((Rect)value).y},{((Rect)value).width},{((Rect)value).height}";
        if (typeof(UnityEngine.Object).IsAssignableFrom(t))
        {
            if (value == null) return "";
            string path = AssetDatabase.GetAssetPath((UnityEngine.Object)value);
            return string.IsNullOrEmpty(path) ? "" : AssetDatabase.AssetPathToGUID(path);
        }
        return value.ToString();
    }

    private object ParseValueFromString(string str, Type targetType)
    {
        if (string.IsNullOrEmpty(str)) return null;
        
        try
        {
            if (targetType == typeof(string)) return str;
            if (targetType == typeof(int)) return int.Parse(str);
            if (targetType == typeof(float)) return float.Parse(str);
            if (targetType == typeof(bool)) return bool.Parse(str);
            if (targetType.IsEnum) return Enum.Parse(targetType, str);
            
            if (targetType == typeof(Vector2))
            {
                var parts = str.Split(',');
                if (parts.Length == 2) return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            if (targetType == typeof(Vector3))
            {
                var parts = str.Split(',');
                if (parts.Length == 3) return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            }
            if (targetType == typeof(Vector4))
            {
                var parts = str.Split(',');
                if (parts.Length == 4) return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            if (targetType == typeof(Quaternion))
            {
                var parts = str.Split(',');
                if (parts.Length == 4) return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            if (targetType == typeof(Color))
            {
                if (ColorUtility.TryParseHtmlString("#" + str, out Color c)) return c;
            }
            if (targetType == typeof(Rect))
            {
                var parts = str.Split(',');
                if (parts.Length == 4) return new Rect(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                string path = AssetDatabase.GUIDToAssetPath(str);
                if (!string.IsNullOrEmpty(path)) return AssetDatabase.LoadAssetAtPath(path, targetType);
            }
        }
        catch { }
        return null;
    }

    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        return value;
    }

    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder current = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}