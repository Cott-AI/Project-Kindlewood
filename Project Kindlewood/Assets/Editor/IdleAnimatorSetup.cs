using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
    public static class IdleAnimatorSetup
    {
        private const string ControllerFolder = "Assets/Animation/Player/Controllers";

        [MenuItem("Tools/Setup Idle Animator Controllers")]
        public static void SetupIdleAnimators()
        {
            EnsureFolderExists(ControllerFolder);

            CreateControllerForObject(
                "Player",
                "Assets/Animation/Player/1. Idle/Skins",
                "PlayerIdle.controller");

            CreateControllerForObject(
                "Accessory",
                "Assets/Animation/Player/1. Idle/Acc",
                "AccessoryIdle.controller");

            CreateControllerForObject(
                "Clothes",
                "Assets/Animation/Player/1. Idle/Clothers",
                "ClothesIdle.controller");

            CreateControllerForObject(
                "Eyes",
                "Assets/Animation/Player/1. Idle/Eyes",
                "EyesIdle.controller");

            CreateControllerForObject(
                "Hair",
                "Assets/Animation/Player/1. Idle/Hair's",
                "HairIdle.controller");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Idle Animator setup completed.");
        }

        private static void CreateControllerForObject(
            string objectName,
            string animationFolder,
            string controllerFileName)
        {
            GameObject target = GameObject.Find(objectName);

            if (target == null)
            {
                Debug.LogWarning($"GameObject '{objectName}' not found.");
                return;
            }

            Animator animator = target.GetComponent<Animator>();

            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            string controllerPath =
                $"{ControllerFolder}/{controllerFileName}";

            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            controller.AddParameter(
                "Direction",
                AnimatorControllerParameterType.Int);

            AnimatorStateMachine stateMachine =
                controller.layers[0].stateMachine;

            Dictionary<string, int> directions =
                new Dictionary<string, int>
                {
                    { "Down", 0 },
                    { "Left", 1 },
                    { "Right", 2 },
                    { "Up", 3 }
                };

            Dictionary<string, AnimatorState> createdStates =
                new Dictionary<string, AnimatorState>();

            foreach (var pair in directions)
            {
                AnimationClip clip =
                    FindAnimationClip(animationFolder, pair.Key);

                if (clip == null)
                    continue;

                AnimatorState state =
                    stateMachine.AddState(pair.Key);

                state.motion = clip;

                createdStates.Add(pair.Key, state);

                if (pair.Key == "Down")
                {
                    stateMachine.defaultState = state;
                }

                Debug.Log(
                    $"Assigned '{clip.name}' to {objectName} ({pair.Key})");
            }

            foreach (var fromState in createdStates)
            {
                foreach (var toState in createdStates)
                {
                    if (fromState.Key == toState.Key)
                        continue;

                    AnimatorStateTransition transition =
                        fromState.Value.AddTransition(toState.Value);

                    transition.hasExitTime = false;
                    transition.exitTime = 0f;
                    transition.duration = 0f;
                    transition.conditions = new AnimatorCondition[0];

                    transition.AddCondition(
                        AnimatorConditionMode.Equals,
                        directions[toState.Key],
                        "Direction");
                }
            }

            animator.runtimeAnimatorController = controller;

            EditorUtility.SetDirty(target);

            Debug.Log(
                $"Animator Controller assigned to '{objectName}'.");
        }

        private static AnimationClip FindAnimationClip(
            string folder,
            string direction)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"Folder not found: {folder}");
                return null;
            }

            string[] guids =
                AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });

            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(guid);

                AnimationClip clip =
                    AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip == null)
                    continue;

                string clipName = clip.name.ToLower();

                if (clipName.EndsWith("_" + direction.ToLower()))
                {
                    return clip;
                }
            }

            Debug.LogWarning(
                $"No {direction} animation found in: {folder}");

            return null;
        }

        private static void EnsureFolderExists(string assetFolder)
        {
            string[] parts = assetFolder.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        parts[i]);
                }

                current = next;
            }
        }
    }
}