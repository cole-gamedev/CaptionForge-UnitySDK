using UnityEngine;
using UnityEditor;
using System.IO;

namespace CaptionForge
{
    public static class AssetHighlighter
    {
        public static void ShowFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogWarning("File path is invalid or does not exist: " + filePath);
                return;
            }

            // Check if the file is inside the Unity project
            var projectPath = Application.dataPath[..^"Assets".Length];
            if (filePath.StartsWith(projectPath))
            {
                // Convert full path to relative project path
                var relativePath = filePath[projectPath.Length..].Replace("\\", "/");

                // Load asset and highlight it
                var asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                    Debug.Log("Highlighted asset in Unity: " + relativePath);
                }
                else
                {
                    Debug.LogWarning("Could not load asset: " + relativePath);
                }
            }
            else
            {
                // Open file location in Explorer (Windows) or Finder (Mac)
                OpenInFileBrowser(filePath);
            }
        }

        private static void OpenInFileBrowser(string filePath)
        {
            var folderPath = Path.GetDirectoryName(filePath);

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "/select,\"" + filePath.Replace('/', '\\') + "\"",
                        UseShellExecute = true
                    });

                    break;
                case RuntimePlatform.OSXEditor:
                    System.Diagnostics.Process.Start("open", "-R \"" + filePath + "\"");
                    break;
                default:
                    Debug.LogWarning("Opening file location is not supported on this platform.");
                    break;
            }
        }
    }
}