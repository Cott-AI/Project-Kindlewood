using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Automatically generates directional character animations
    /// from sliced Unity Sprite Sheets.
    ///
    /// Supported layouts:
    /// 12 sprites: Down, Left, Right
    /// 16 sprites: Down, Left, Right, Up
    ///
    /// Each animation contains four frames at 60 FPS,
    /// with an 8-frame interval between each sprite change.
    /// </summary>
    public static class CharacterAnimationGenerator
    {
        // ---------------------------------------------------------
        // CONFIGURATION
        // ---------------------------------------------------------

        private const string SourceRoot =
            "Assets/Sprites/Character/Character/PNG";

        private const string OutputRoot =
            "Assets/Animation/Player";

        private const int FramesPerDirection = 4;

        private const int AnimationFrameRate = 60;

        private const int FramesBetweenKeys = 8;

        // Direction order MUST match the Sprite Sheet layout.
        private static readonly string[] Directions =
        {
            "Down",
            "Left",
            "Right",
            "Up"
        };

        // ---------------------------------------------------------
        // MENU ITEM
        // ---------------------------------------------------------

        [MenuItem("Tools/Generate Character Animations")]
        public static void GenerateCharacterAnimations()
        {
            Debug.Log("========================================");
            Debug.Log("Character Animation Generator started.");
            Debug.Log("========================================");

            int generatedCount = 0;
            int skippedCount = 0;
            int errorCount = 0;

            try
            {
                // Check that the source directory exists.
                if (!AssetDatabase.IsValidFolder(SourceRoot))
                {
                    Debug.LogError(
                        $"Source folder does not exist: {SourceRoot}"
                    );

                    return;
                }

                // Make sure the output directory exists.
                EnsureDirectoryExists(OutputRoot);

                // Find every PNG file recursively.
                string[] spriteSheetGuids =
                    AssetDatabase.FindAssets(
                        "t:Texture2D",
                        new[] { SourceRoot }
                    );

                List<string> spriteSheetPaths = new List<string>();

                foreach (string guid in spriteSheetGuids)
                {
                    string assetPath =
                        AssetDatabase.GUIDToAssetPath(guid);

                    if (string.Equals(
                            Path.GetExtension(assetPath),
                            ".png",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        spriteSheetPaths.Add(assetPath);
                    }
                }

                // Sort file paths for consistent processing.
                // This does NOT change the sprite order inside sheets.
                spriteSheetPaths.Sort(StringComparer.Ordinal);

                if (spriteSheetPaths.Count == 0)
                {
                    Debug.LogWarning(
                        $"No PNG files found under {SourceRoot}"
                    );

                    return;
                }

                Debug.Log(
                    $"Found {spriteSheetPaths.Count} PNG sprite sheets."
                );

                // Process every discovered sprite sheet.
                foreach (string spriteSheetPath in spriteSheetPaths)
                {
                    try
                    {
                        int generatedForSheet =
                            ProcessSpriteSheet(spriteSheetPath);

                        if (generatedForSheet > 0)
                        {
                            generatedCount += generatedForSheet;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                    catch (Exception exception)
                    {
                        errorCount++;

                        Debug.LogError(
                            $"Failed to process sprite sheet:\n" +
                            $"{spriteSheetPath}\n" +
                            $"{exception}"
                        );
                    }
                }

                // Save all generated assets.
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Print generation summary.
                Debug.Log("========================================");
                Debug.Log("CHARACTER ANIMATION GENERATION SUMMARY");
                Debug.Log("========================================");

                Debug.Log($"Processed sprite sheets: {spriteSheetPaths.Count}");
                Debug.Log($"Generated animation clips: {generatedCount}");
                Debug.Log($"Skipped sprite sheets: {skippedCount}");
                Debug.Log($"Errors: {errorCount}");

                Debug.Log("========================================");
                Debug.Log("Animation generation completed.");
                Debug.Log("========================================");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Character Animation Generator encountered " +
                    "an unexpected error:\n" + exception
                );
            }
        }

        // ---------------------------------------------------------
        // SPRITE SHEET PROCESSING
        // ---------------------------------------------------------

        /// <summary>
        /// Loads and processes one sprite sheet.
        ///
        /// Returns the number of animation clips generated.
        /// Returns zero if the sheet is invalid or skipped.
        /// </summary>
        private static int ProcessSpriteSheet(string spriteSheetPath)
        {
            Debug.Log($"Processing: {spriteSheetPath}");

            // Retrieve the texture importer.
            TextureImporter importer =
                AssetImporter.GetAtPath(spriteSheetPath)
                    as TextureImporter;

            if (importer == null)
            {
                Debug.LogWarning(
                    $"Could not retrieve TextureImporter: {spriteSheetPath}"
                );

                return 0;
            }

            // Only process sheets configured as Multiple.
            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                Debug.LogWarning(
                    $"Skipping sprite sheet because Sprite Mode is not " +
                    $"Multiple:\n{spriteSheetPath}"
                );

                return 0;
            }

            // Load all assets contained inside the sprite sheet.
            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);

            // IMPORTANT:
            // Preserve Unity's returned sub-asset order.
            // Do not sort sprites by their names.
            Sprite[] sprites = assets
                .OfType<Sprite>()
                .ToArray();

            // Detect the sprite sheet layout automatically.
            int spriteCount = sprites.Length;

            bool hasUpAnimation;

            if (spriteCount == 12)
            {
                hasUpAnimation = false;
            }
            else if (spriteCount == 16)
            {
                hasUpAnimation = true;
            }
            else
            {
                Debug.LogWarning(
                    $"Skipping invalid sprite sheet:\n" +
                    $"{spriteSheetPath}\n" +
                    $"Expected 12 or 16 sprites, found {spriteCount}."
                );

                return 0;
            }

            // Validate that every sprite reference is available.
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null)
                {
                    Debug.LogWarning(
                        $"Skipping sprite sheet because sprite index {i} " +
                        $"is missing:\n{spriteSheetPath}"
                    );

                    return 0;
                }
            }

            // Determine the number of available directions.
            int directionCount = hasUpAnimation ? 4 : 3;

            // Generate the destination folder while preserving
            // the original source directory hierarchy.
            string outputDirectory =
                GetOutputDirectory(spriteSheetPath);

            EnsureDirectoryExists(outputDirectory);

            // Get the source sprite sheet filename.
            string baseName =
                Path.GetFileNameWithoutExtension(spriteSheetPath);

            int generatedCount = 0;

            // Generate one animation clip per direction.
            for (int directionIndex = 0;
                 directionIndex < directionCount;
                 directionIndex++)
            {
                string direction = Directions[directionIndex];

                // Calculate the first sprite index for this direction.
                int firstSpriteIndex =
                    directionIndex * FramesPerDirection;

                // Extract exactly four sprites.
                Sprite[] directionSprites = new Sprite[FramesPerDirection];

                for (int frame = 0;
                     frame < FramesPerDirection;
                     frame++)
                {
                    int spriteIndex = firstSpriteIndex + frame;

                    directionSprites[frame] = sprites[spriteIndex];
                }

                // Construct the animation asset path.
                string animationPath =
                    $"{outputDirectory}/{baseName}_{direction}.anim";

                // Generate or overwrite the animation.
                CreateAnimationClip(
                    animationPath,
                    directionSprites
                );

                generatedCount++;

                Debug.Log(
                    $"Generated: {animationPath}"
                );
            }

            return generatedCount;
        }

        // ---------------------------------------------------------
        // ANIMATION CLIP CREATION
        // ---------------------------------------------------------

        /// <summary>
        /// Creates an AnimationClip using SpriteRenderer.m_Sprite.
        ///
        /// Existing AnimationClips are updated in place to preserve
        /// their asset GUIDs and existing Animator Controller references.
        /// </summary>
        private static void CreateAnimationClip(
            string animationPath,
            Sprite[] sprites)
        {
            if (sprites == null ||
                sprites.Length != FramesPerDirection)
            {
                throw new ArgumentException(
                    "An animation must contain exactly four sprites."
                );
            }

            // Validate sprite references.
            if (sprites.Any(sprite => sprite == null))
            {
                throw new ArgumentException(
                    $"Animation contains a missing sprite: {animationPath}"
                );
            }

            // Look for an existing animation asset.
            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    animationPath
                );

            if (clip == null)
            {
                // Do not silently replace an unrelated asset.
                UnityEngine.Object existingAsset =
                    AssetDatabase.LoadMainAssetAtPath(animationPath);

                if (existingAsset != null)
                {
                    throw new InvalidOperationException(
                        $"An asset already exists at {animationPath}, " +
                        "but it is not an AnimationClip."
                    );
                }

                // Create a new animation clip.
                clip = new AnimationClip();

                clip.name = Path.GetFileNameWithoutExtension(
                    animationPath
                );
            }
            else
            {
                // Clear old animation curves when overwriting.
                // The existing asset itself is retained.
                clip.ClearCurves();
            }

            // Configure animation frame rate.
            clip.frameRate = AnimationFrameRate;

            // Create the SpriteRenderer sprite property binding.
            //
            // Empty path means the SpriteRenderer is on the
            // GameObject containing the Animator component.
            EditorCurveBinding binding =
                EditorCurveBinding.PPtrCurve(
                    "",
                    typeof(SpriteRenderer),
                    "m_Sprite"
                );

            // Create object reference keyframes.
            ObjectReferenceKeyframe[] keyframes =
                new ObjectReferenceKeyframe[FramesPerDirection];

            for (int frame = 0;
                 frame < FramesPerDirection;
                 frame++)
            {
                // Frame positions:
                // 0, 8, 16, 24
                int animationFrame =
                    frame * FramesBetweenKeys;

                // Convert frame number into seconds.
                float keyTime =
                    (float)animationFrame / AnimationFrameRate;

                keyframes[frame] = new ObjectReferenceKeyframe
                {
                    time = keyTime,
                    value = sprites[frame]
                };
            }

            // Apply the sprite animation curve.
            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                keyframes
            );

            // Configure looping.
            AnimationClipSettings settings =
                AnimationUtility.GetAnimationClipSettings(clip);

            settings.loopTime = true;

            AnimationUtility.SetAnimationClipSettings(
                clip,
                settings
            );

            // Set wrap mode as an additional looping setting.
            clip.wrapMode = WrapMode.Loop;

            // Create the asset if it does not already exist.
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(
                    animationPath) == null)
            {
                AssetDatabase.CreateAsset(
                    clip,
                    animationPath
                );
            }

            // Mark the animation asset as modified.
            EditorUtility.SetDirty(clip);
        }

        // ---------------------------------------------------------
        // OUTPUT DIRECTORY HANDLING
        // ---------------------------------------------------------

        /// <summary>
        /// Calculates the output directory by removing the source root
        /// from the original sprite sheet path.
        ///
        /// Example:
        ///
        /// Source:
        /// Assets/Sprites/Character/Character/PNG/1. Idle/Beard/Black.png
        ///
        /// Output:
        /// Assets/Animation/Player/1. Idle/Beard
        /// </summary>
        private static string GetOutputDirectory(
            string spriteSheetPath)
        {
            string normalizedSourceRoot =
                SourceRoot.TrimEnd('/', '\\');

            string normalizedSpritePath =
                spriteSheetPath.Replace('\\', '/');

            string sourcePrefix =
                normalizedSourceRoot + "/";

            if (!normalizedSpritePath.StartsWith(
                    sourcePrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Sprite sheet is outside the source directory: " +
                    spriteSheetPath
                );
            }

            // Get the relative directory beneath PNG.
            string relativePath =
                normalizedSpritePath.Substring(
                    sourcePrefix.Length
                );

            string relativeDirectory =
                Path.GetDirectoryName(relativePath);

            if (string.IsNullOrEmpty(relativeDirectory))
            {
                return OutputRoot;
            }

            relativeDirectory =
                relativeDirectory.Replace('\\', '/');

            return $"{OutputRoot}/{relativeDirectory}";
        }

        /// <summary>
        /// Creates a directory and all missing parent directories
        /// using AssetDatabase APIs.
        /// </summary>
        private static void EnsureDirectoryExists(
            string directoryPath)
        {
            directoryPath =
                directoryPath.Replace('\\', '/').TrimEnd('/');

            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentException(
                    "Directory path cannot be empty."
                );
            }

            if (AssetDatabase.IsValidFolder(directoryPath))
            {
                return;
            }

            // Split the directory into individual components.
            string[] folders = directoryPath.Split('/');

            if (folders.Length == 0 || folders[0] != "Assets")
            {
                throw new InvalidOperationException(
                    $"Invalid Unity asset directory: {directoryPath}"
                );
            }

            string currentPath = "Assets";

            // Create every missing directory.
            for (int i = 1; i < folders.Length; i++)
            {
                string nextPath =
                    $"{currentPath}/{folders[i]}";

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    string guid =
                        AssetDatabase.CreateFolder(
                            currentPath,
                            folders[i]
                        );

                    if (string.IsNullOrEmpty(guid))
                    {
                        throw new IOException(
                            $"Could not create directory: {nextPath}"
                        );
                    }
                }

                currentPath = nextPath;
            }
        }
    }
}
