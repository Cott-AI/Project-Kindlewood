using System;
using System.Collections.Generic;
using UnityEngine;

namespace Economy
{
    public class ResourceBox : MonoBehaviour
    {
        private readonly Dictionary<string, int> _storedResources = new Dictionary<string, int>();

        // Event, for UI to listen
        public event Action OnResourcesChanged;

        public void DepositResources(string resourceType, int amount)
        {
            if (!_storedResources.TryAdd(resourceType, amount))
            {
                _storedResources[resourceType] += amount;
            }
            
            // Let UI know resource amounts has changed
            OnResourcesChanged?.Invoke();
        }

        public int GetResourceCount(string resourceType)
        {
            return _storedResources.GetValueOrDefault(resourceType, 0);
        }

        // Lets UI show all stored resources
        public IReadOnlyDictionary<string, int> GetAllResources()
        {
            return _storedResources;
        }

        public bool TrySpendResources(string resourceType, int amount)
        {
            if (amount <= 0) return true;

            int currentAmount = _storedResources.GetValueOrDefault(resourceType, 0);
            if (currentAmount < amount)
            {
                return false;
            }
            
            _storedResources[resourceType] = currentAmount - amount;
            
            // Update the UI
            OnResourcesChanged?.Invoke();
            
            return true;
        }
    }
}