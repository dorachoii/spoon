#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetNameFixer : EditorWindow
{
    [MenuItem("Tools/Fix Asset Names")]
    static void ShowWindow()
    {
        GetWindow<AssetNameFixer>("Fix Asset Names");
    }

    string folderPath = "Assets"; // 기본 경로

    void OnGUI()
    {
        GUILayout.Label("Fix Asset main object names to match filenames", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Fix Asset Names"))
        {
            FixAssetNames(folderPath);
        }
    }

    static void FixAssetNames(string folder)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { folder });

        int fixedCount = 0;

        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadMainAssetAtPath(path);

            if (asset == null)
                continue;

            string fileName = Path.GetFileNameWithoutExtension(path);

            if (asset.name != fileName)
            {
                Debug.Log($"Fixing asset name: {asset.name} -> {fileName} ({path})");
                asset.name = fileName;

                // 에셋 이름 변경 후 저장
                EditorUtility.SetDirty(asset);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Fix Asset Names 완료! 총 {fixedCount}개의 에셋 이름을 변경했습니다.");
    }
}
#endif
