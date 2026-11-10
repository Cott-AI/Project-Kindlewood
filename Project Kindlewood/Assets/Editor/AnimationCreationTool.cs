#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class FolderStructureCopier : EditorWindow
    {
        private DefaultAsset _sourceFolder;
        private DefaultAsset _targetParentFolder;

        [MenuItem("Tools/Kopioi kansiorakenne")]
        public static void ShowWindow()
        {
            GetWindow<FolderStructureCopier>("Kansiorakenteen kopioija");
        }

        private void OnGUI()
        {
            GUILayout.Label("Kopioi kansiorakenne toiseen paikkaan", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Lähdekansio (Mistä)", _sourceFolder, typeof(DefaultAsset), false);
            _targetParentFolder = (DefaultAsset)EditorGUILayout.ObjectField("Kohdekansio (Mihin)", _targetParentFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Kopioi kansiorakenne"))
            {
                CopyFolderStructure();
            }
        }

        private void CopyFolderStructure()
        {
            if (_sourceFolder == null || _targetParentFolder == null)
            {
                EditorUtility.DisplayDialog("Virhe", "Valitse sekä lähde- että kohdekansio!", "OK");return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(_sourceFolder);
            string targetParentPath = AssetDatabase.GetAssetPath(_targetParentFolder);

            if (!AssetDatabase.IsValidFolder(sourcePath) || !AssetDatabase.IsValidFolder(targetParentPath))
            {
                EditorUtility.DisplayDialog("Virhe", "Valittujen kohteiden täytyy olla kelvollisia kansioita Unityn projektissa.", "OK");
                return;
            }

            string sourceSystemPath = Application.dataPath.Replace("Assets", "") + sourcePath;
            string folderName = Path.GetFileName(sourcePath);
            string newRootTargetPath = targetParentPath + "/" + folderName;

            // Luodaan uusi juurikansio kohteeseen, jos sitä ei vielä ole
            if (!AssetDatabase.IsValidFolder(newRootTargetPath))
            {
                AssetDatabase.CreateFolder(targetParentPath, folderName);
            }

            // Kopioidaan alikansiot rekursiivisesti
            DirectoryInfo dirInfo = new DirectoryInfo(sourceSystemPath);
            foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
            {
                CopyDirectoryRecursive(subDir.FullName, newRootTargetPath);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Valmis!", "Kansiorakenne kopioitiin onnistuneesti.", "OK");
        }

        private void CopyDirectoryRecursive(string sourceDir, string targetAssetParentPath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            string folderName = dir.Name;
            string currentAssetPath = targetAssetParentPath + "/" + folderName;

            // Luodaan kansio, jos se puuttuu
            if (!AssetDatabase.IsValidFolder(currentAssetPath))
            {
                AssetDatabase.CreateFolder(targetAssetParentPath, folderName);
            }

            // Käydään läpi alikansiot
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                CopyDirectoryRecursive(subDir.FullName, currentAssetPath);
            }
        }
    }
}
#endif