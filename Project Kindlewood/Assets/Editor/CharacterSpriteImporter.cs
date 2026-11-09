using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Debug;

namespace Editor
{
    /// <summary>
    /// Character Sprite Importer for Unity 6.
    ///
    /// Automatically validates and fixes PNG import settings
    /// for the Farm RPG character sprite assets.
    ///
    /// Features:
    /// - Recursively scans all PNG files.
    /// - Configures Sprite (2D and UI).
    /// - Uses Multiple sprite mode.
    /// - Sets Pixels Per Unit to 16.
    /// - Configures RGBA32 uncompressed textures.
    /// - Preserves existing sprite slicing.
    /// - Validates individual sprite dimensions.
    /// - Reports missing or invalid sprite slices.
    /// - Supports dry-run validation.
    /// - Handles platform-specific texture overrides.
    ///
    /// IMPORTANT:
    /// This tool NEVER automatically slices or deletes sprites.
    /// Existing Sprite Editor slicing information is preserved.
    /// </summary>
    public static class CharacterSpriteImporter
    {
        // =========================================================
        // CONFIGURATION
        // =========================================================

        private const string SourceFolder =
            "Assets/Sprites/Character/Character/PNG";

        private const int ExpectedSpriteWidth = 16;
        private const int ExpectedSpriteHeight = 16;

        private const float PixelsPerUnit = 16f;

        // Known platform names used by Unity texture import settings.
        private static readonly string[] KnownPlatforms =
        {
            "Standalone",
            "Android",
            "iPhone",
            "WebGL",
            "Windows Store Apps",
            "tvOS"
        };

        // =========================================================
        // GENERATION STATISTICS
        // =========================================================

        private class ImportStatistics
        {
            public int TotalFiles;
            public int FixedFiles;
            public int UnchangedFiles;
            public int InvalidFiles;
            public int MissingSlices;
            public int DimensionWarnings;
            public int Errors;
            public int TotalSprites;
        }

        // =========================================================
        // MENU ITEMS
        // =========================================================

        [MenuItem("Tools/Character Sprites/Validate & Fix PNGs")]
        private static void ValidateAndFix()
        {
            ProcessSprites(false);
        }

        [MenuItem("Tools/Character Sprites/Validate PNGs (Dry Run)")]
        private static void ValidateOnly()
        {
            ProcessSprites(true);
        }

        // =========================================================
        // MAIN PROCESS
        // =========================================================

        private static void ProcessSprites(bool dryRun)
        {
            ImportStatistics statistics = new ImportStatistics();

            string mode = dryRun ? "DRY RUN" : "FIX MODE";

            Log(
                "==================================================\n" +
                $"CHARACTER SPRITE IMPORTER - {mode}\n" +
                "=================================================="
            );

            try
            {
                // Verify source directory.
                if (!AssetDatabase.IsValidFolder(SourceFolder))
                {
                    LogError(
                        $"Source directory does not exist:\n{SourceFolder}"
                    );

                    return;
                }

                // Find every texture recursively.
                string[] guids = AssetDatabase.FindAssets(
                    "t:Texture2D",
                    new[] { SourceFolder }
                );

                List<string> pngFiles = guids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path =>
                        string.Equals(
                            Path.GetExtension(path),
                            ".png",
                            StringComparison.OrdinalIgnoreCase
                        ))
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToList();

                statistics.TotalFiles = pngFiles.Count;

                if (pngFiles.Count == 0)
                {
                    LogWarning(
                        $"No PNG files found in {SourceFolder}"
                    );

                    return;
                }

                Log(
                    $"Found {pngFiles.Count} PNG files."
                );

                // Process files individually.
                for (int i = 0; i < pngFiles.Count; i++)
                {
                    string path = pngFiles[i];

                    try
                    {
                        EditorUtility.DisplayProgressBar(
                            "Character Sprite Importer",
                            $"Processing {Path.GetFileName(path)}",
                            (float)(i + 1) / pngFiles.Count
                        );

                        ProcessSprite(path, dryRun, statistics);
                    }
                    catch (Exception exception)
                    {
                        statistics.Errors++;

                        LogError(
                            $"Error processing:\n{path}\n{exception}"
                        );
                    }
                }

                // Save and refresh only when changes were applied.
                if (!dryRun)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception exception)
            {
                LogError(
                    $"Unexpected importer error:\n{exception}"
                );
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            PrintSummary(statistics, dryRun);
        }

        // =========================================================
        // PROCESS INDIVIDUAL SPRITE
        // =========================================================

        private static void ProcessSprite(
            string assetPath,
            bool dryRun,
            ImportStatistics statistics)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null)
            {
                statistics.Errors++;

                LogWarning(
                    $"Could not retrieve TextureImporter: {assetPath}"
                );

                return;
            }

            // Check whether any import settings need modification.
            bool needsFix = NeedsSettingsUpdate(importer);

            if (needsFix)
            {
                if (dryRun)
                {
                    Log(
                        $"[WOULD FIX] {assetPath}"
                    );
                }
                else
                {
                    ApplyImportSettings(importer);

                    // Reimport only files whose settings changed.
                    importer.SaveAndReimport();

                    statistics.FixedFiles++;

                    Log(
                        $"[FIXED] {assetPath}"
                    );
                }
            }
            else
            {
                statistics.UnchangedFiles++;

                Log(
                    $"[OK] {assetPath}"
                );
            }

            // Load imported sprite assets after applying settings.
            // In dry-run mode, this reads the existing imported data.
            ValidateSpriteSlices(assetPath, statistics);
        }

        // =========================================================
        // CHECK IMPORT SETTINGS
        // =========================================================

        private static bool NeedsSettingsUpdate(
            TextureImporter importer)
        {
            // Basic texture settings.
            if (importer.textureType != TextureImporterType.Sprite)
                return true;

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
                return true;

            if (!Mathf.Approximately(
                    importer.spritePixelsPerUnit,
                    PixelsPerUnit))
                return true;

            if (importer.filterMode != FilterMode.Point)
                return true;

            if (importer.mipmapEnabled)
                return true;

            if (importer.isReadable)
                return true;

            if (importer.wrapMode != TextureWrapMode.Clamp)
                return true;

            if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
                return true;

            if (!importer.alphaIsTransparency)
                return true;

            // Check default platform texture format.
            if (PlatformSettingsNeedFix(
                    importer.GetDefaultPlatformTextureSettings()))
                return true;

            // Check existing platform-specific overrides.
            foreach (string platform in GetPlatformsToCheck())
            {
                TextureImporterPlatformSettings settings =
                    importer.GetPlatformTextureSettings(platform);

                if (settings.overridden &&
                    PlatformSettingsNeedFix(settings))
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================================
        // PLATFORM VALIDATION
        // =========================================================

        private static bool PlatformSettingsNeedFix(
            TextureImporterPlatformSettings settings)
        {
            return
                settings.format != TextureImporterFormat.RGBA32 ||
                settings.textureCompression !=
                TextureImporterCompression.Uncompressed ||
                settings.crunchedCompression;
        }

        /// <summary>
        /// Returns known platforms and the currently selected
        /// build platform, avoiding duplicate entries.
        /// </summary>
        private static IEnumerable<string> GetPlatformsToCheck()
        {
            HashSet<string> platforms =
                new HashSet<string>(KnownPlatforms);

            string activePlatform = GetActiveTexturePlatform();

            if (!string.IsNullOrEmpty(activePlatform))
                platforms.Add(activePlatform);

            return platforms;
        }

        /// <summary>
        /// Unity uses texture-platform names that can differ
        /// from the BuildTarget enum names.
        /// </summary>
        private static string GetActiveTexturePlatform()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneOSX:
                    return "Standalone";

                case BuildTarget.Android:
                    return "Android";

                case BuildTarget.iOS:
                    return "iPhone";

                case BuildTarget.WebGL:
                    return "WebGL";

                default:
                    return BuildPipeline.GetBuildTargetName(
                        EditorUserBuildSettings.activeBuildTarget
                    );
            }
        }

        // =========================================================
        // APPLY IMPORT SETTINGS
        // =========================================================

        private static void ApplyImportSettings(
            TextureImporter importer)
        {
            // -----------------------------------------------------
            // SPRITE SETTINGS
            // -----------------------------------------------------

            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Multiple;

            importer.spritePixelsPerUnit =
                PixelsPerUnit;

            SerializedObject serializedImporter =
                new SerializedObject(importer);

            SerializedProperty meshType =
                serializedImporter.FindProperty("m_SpriteMeshType");

            if (meshType != null)
            {
                meshType.intValue = (int)SpriteMeshType.FullRect;
                serializedImporter.ApplyModifiedProperties();
            }

            // -----------------------------------------------------
            // IMAGE FILTERING
            // -----------------------------------------------------

            importer.filterMode =
                FilterMode.Point;

            importer.wrapMode =
                TextureWrapMode.Clamp;

            // -----------------------------------------------------
            // ALPHA SETTINGS
            // -----------------------------------------------------

            importer.alphaSource =
                TextureImporterAlphaSource.FromInput;

            importer.alphaIsTransparency = true;

            // -----------------------------------------------------
            // PERFORMANCE SETTINGS
            // -----------------------------------------------------

            importer.mipmapEnabled = false;

            importer.isReadable = false;

            // -----------------------------------------------------
            // DEFAULT TEXTURE PLATFORM
            // -----------------------------------------------------

            TextureImporterPlatformSettings defaultSettings =
                importer.GetDefaultPlatformTextureSettings();

            ConfigureTextureFormat(defaultSettings);

            importer.SetPlatformTextureSettings(defaultSettings);

            // -----------------------------------------------------
            // EXISTING PLATFORM OVERRIDES
            // -----------------------------------------------------

            foreach (string platform in GetPlatformsToCheck())
            {
                TextureImporterPlatformSettings settings =
                    importer.GetPlatformTextureSettings(platform);

                // Only modify existing overrides.
                // Platforms without overrides inherit default settings.
                if (!settings.overridden)
                    continue;

                ConfigureTextureFormat(settings);

                importer.SetPlatformTextureSettings(settings);
            }
        }

        /// <summary>
        /// Configures a platform to use uncompressed RGBA32.
        /// Other platform settings are left unchanged.
        /// </summary>
        private static void ConfigureTextureFormat(
            TextureImporterPlatformSettings settings)
        {
            settings.format =
                TextureImporterFormat.RGBA32;

            settings.textureCompression =
                TextureImporterCompression.Uncompressed;

            settings.crunchedCompression = false;
        }

        // =========================================================
        // SPRITE SLICE VALIDATION
        // =========================================================

        private static void ValidateSpriteSlices(
            string assetPath,
            ImportStatistics statistics)
        {
            UnityEngine.Object[] assets =
                AssetDatabase.LoadAllAssetsAtPath(assetPath);

            Sprite[] sprites = assets
                .OfType<Sprite>()
                .ToArray();

            // No sliced sprites found.
            if (sprites.Length == 0)
            {
                statistics.MissingSlices++;

                LogWarning(
                    $"[NO SPRITES FOUND]\n" +
                    $"{assetPath}\n" +
                    "The texture is configured as Multiple, " +
                    "but no sliced Sprite assets were found.\n" +
                    "Open Sprite Editor and create the slices manually."
                );

                return;
            }

            statistics.TotalSprites += sprites.Length;

            int invalidDimensions = 0;

            foreach (Sprite sprite in sprites)
            {
                if (sprite == null)
                {
                    invalidDimensions++;

                    LogWarning(
                        $"Missing Sprite reference in {assetPath}"
                    );

                    continue;
                }

                int width = Mathf.RoundToInt(sprite.rect.width);
                int height = Mathf.RoundToInt(sprite.rect.height);

                if (width != ExpectedSpriteWidth ||
                    height != ExpectedSpriteHeight)
                {
                    invalidDimensions++;

                    LogWarning(
                        $"[INVALID SPRITE DIMENSIONS]\n" +
                        $"File: {assetPath}\n" +
                        $"Sprite: {sprite.name}\n" +
                        $"Expected: {ExpectedSpriteWidth}x" +
                        $"{ExpectedSpriteHeight}\n" +
                        $"Found: {width}x{height}"
                    );
                }
            }

            if (invalidDimensions > 0)
            {
                statistics.DimensionWarnings++;

                statistics.InvalidFiles++;

                LogWarning(
                    $"{invalidDimensions} invalid sprite(s) " +
                    $"found in {assetPath}"
                );
            }
            else
            {
                Log(
                    $"[VALID SLICING] {assetPath} " +
                    $"({sprites.Length} sprites, " +
                    $"{ExpectedSpriteWidth}x{ExpectedSpriteHeight})"
                );
            }
        }

        // =========================================================
        // SUMMARY
        // =========================================================

        private static void PrintSummary(
            ImportStatistics statistics,
            bool dryRun)
        {
            Log(
                "\n" +
                "==================================================\n" +
                "CHARACTER SPRITE IMPORT SUMMARY\n" +
                "=================================================="
            );

            Log($"Mode: {(dryRun ? "DRY RUN" : "FIX MODE")}");

            Log($"PNG files found: {statistics.TotalFiles}");

            Log($"Files fixed: {statistics.FixedFiles}");

            Log($"Files already correct: {statistics.UnchangedFiles}");

            Log($"Files with invalid dimensions: {statistics.InvalidFiles}");

            Log($"Missing sprite slicing: {statistics.MissingSlices}");

            Log($"Dimension warnings: {statistics.DimensionWarnings}");

            Log($"Total imported sprites: {statistics.TotalSprites}");

            Log($"Errors: {statistics.Errors}");

            Log(
                "=================================================="
            );

            if (dryRun)
            {
                Log(
                    "Dry run completed. No import settings were modified."
                );
            }
            else
            {
                Log(
                    "PNG import validation and fixing completed."
                );
            }

            Log(
                "=================================================="
            );
        }
    }
}
