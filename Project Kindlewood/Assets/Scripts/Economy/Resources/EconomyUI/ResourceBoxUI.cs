using System;
using System.Collections.Generic;
using UnityEngine;

namespace Economy.Resources.EconomyUI
{
    [Serializable]
    public struct ResourceIconMapping
    {
        public string resourceType; // For example "Tree"
        public Sprite icon;         // Tree Sprite icon
    }

    public class ResourceBoxUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ResourceBox resourceBox;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private ResourceSlotUI slotPrefab;

        [Header("Resources and icons to show")]
        [SerializeField] private List<ResourceIconMapping> resourceIcons;

        // Save created UI-slots into a dictionary for fast updating
        private readonly Dictionary<string, ResourceSlotUI> _spawnedSlots = new Dictionary<string, ResourceSlotUI>();

        private void Awake()
        {
            InitializeSlots();
        }

        private void OnEnable()
        {
            if (!resourceBox) return;
            resourceBox.OnResourcesChanged += RefreshUI;
            RefreshUI();
        }

        private void OnDisable()
        {
            if (resourceBox)
            {
                resourceBox.OnResourcesChanged -= RefreshUI;
            }
        }

        private void InitializeSlots()
        {
            // Empty examples
            foreach (Transform child in slotContainer)
            {
                Destroy(child.gameObject);
            }

            // Create a slot for all resources with default value = 0
            foreach (var mapping in resourceIcons)
            {
                if (string.IsNullOrEmpty(mapping.resourceType) || _spawnedSlots.ContainsKey(mapping.resourceType))
                    continue;

                var newSlot = Instantiate(slotPrefab, slotContainer);
                newSlot.SetData(mapping.resourceType, 0, mapping.icon);
                
                _spawnedSlots.Add(mapping.resourceType, newSlot);
            }
        }

        private void RefreshUI()
        {
            // Update all predetermined slots
            foreach (var (resourceType, slotUI) in _spawnedSlots)
            {
                var count = resourceBox.GetResourceCount(resourceType);
                slotUI.UpdateAmount(count);
            }

            // If resource is deposited into Resource Box, and it hasn't been defined
            // create a slot for it, so it doesn't remain hidden
            foreach (var resource in resourceBox.GetAllResources())
            {
                if (_spawnedSlots.ContainsKey(resource.Key)) continue;
                var newSlot = Instantiate(slotPrefab, slotContainer);
                newSlot.SetData(resource.Key, resource.Value);
                _spawnedSlots.Add(resource.Key, newSlot);
            }
        }
    }
}