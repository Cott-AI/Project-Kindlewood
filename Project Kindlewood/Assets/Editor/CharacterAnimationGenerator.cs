#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Unity Editor Tool to automatically generate directional character animation clips
    /// from sprite sheets based on a strict 12 or 16 frames layout.
    /// </summary>
    public class CharacterAnimationGenerator : EditorWindow
    {
        // Folder paths - update these if your main folder is "Animations" (plural)
        private const string SourceDirectory = "Assets/Sprites/Character/Character/PNG";
        private const string OutputDirectory = "Assets/Animations/Player";
    
        // Animation timing constants based on 60 FPS
        private const float FrameRate = 60f;
        private const int FramesPerSprite = 8;

        [MenuItem("Tools/Generate Character Animations")]
        public static void GenerateAnimations()
        {
            if (!AssetDatabase.IsValidFolder(SourceDirectory))
            {
                Debug.LogError($"[CharacterAnimationGenerator] Execution aborted. Source directory does not exist: {SourceDirectory}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(OutputDirectory))
            {
                Debug.LogError($"[CharacterAnimationGenerator] Execution aborted. Output directory does not exist: {OutputDirectory}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SourceDirectory });
            if (guids.Length == 0)
            {
                Debug.LogWarning($"[CharacterAnimationGenerator] No texture files found in {SourceDirectory}");
                return;
            }

            List<string> generatedFiles = new List<string>();
            HashSet<string> processedPaths = new HashSet<string>();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            
                if (!assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) 
                    continue;

                if (!processedPaths.Add(assetPath)) 
                    continue;

                ProcessSpriteSheet(assetPath, generatedFiles);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (generatedFiles.Count > 0)
            {
                StringBuilder summary = new StringBuilder();
                summary.AppendLine("Generated:");
                foreach (string file in generatedFiles)
                {
                    summary.AppendLine(file);
                }
                Debug.Log(summary.ToString());
            }

            Debug.Log("Animation generation completed.");
        }

        private static void ProcessSpriteSheet(string assetPath, List<string> generatedFiles)
        {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        
            Sprite[] sprites = allAssets.OfType<Sprite>()
                .OrderBy(GetSpriteIndex)
                .ToArray();

            if (sprites.Length != 12 && sprites.Length != 16)
            {
                Debug.LogWarning($"[CharacterAnimationGenerator] Skipped {assetPath}: Expected exactly 12 or 16 sprites, but found {sprites.Length}.");
                return;
            }

            // Dynamically assign directions using ternary operator
            string[] directions = sprites.Length == 16 
                ? new[] { "Down", "Up", "Right", "Left" } 
                : new[] { "Down", "Right", "Left" };

            string directoryName = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            string relativePath = "";

            if (directoryName != null && directoryName.Length > SourceDirectory.Length)
            {
                relativePath = directoryName.Substring(SourceDirectory.Length);
            
                if (relativePath.StartsWith("/")) 
                    relativePath = relativePath.Substring(1);
            }

            string targetDir = string.IsNullOrEmpty(relativePath) ? OutputDirectory : $"{OutputDirectory}/{relativePath}";
        
            // Strict validation: Ensure the user's manual folder structure matches the source
            if (!AssetDatabase.IsValidFolder(targetDir))
            {
                Debug.LogWarning($"[CharacterAnimationGenerator] Skipped {assetPath}: Target directory missing. Please manually create: {targetDir}");
                return;
            }

            string baseName = Path.GetFileNameWithoutExtension(assetPath);

            for (int d = 0; d < directions.Length; d++)
            {
                string direction = directions[d];
                string clipName = $"{baseName}_{direction}.anim";
                string clipPath = $"{targetDir}/{clipName}";

                CreateOrUpdateAnimationClip(clipPath, sprites, d * 4);

                string summaryPath = string.IsNullOrEmpty(relativePath) ? clipName : $"{relativePath}/{clipName}";
                generatedFiles.Add(summaryPath);
            }
        }

        private static void CreateOrUpdateAnimationClip(string clipPath, Sprite[] sprites, int startIndex)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            bool isNew = false;

            if (clip == null)
            {
                clip = new AnimationClip();
                isNew = true;
            }

            clip.frameRate = FrameRate;

            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[4];
            for (int i = 0; i < 4; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (i * FramesPerSprite) / FrameRate,
                    value = sprites[startIndex + i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (isNew)
            {
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }
        }

        private static int GetSpriteIndex(Sprite sprite)
        {
            Match match = Regex.Match(sprite.name, @"_(\d+)$");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0; 
        }
    }
}
#endif