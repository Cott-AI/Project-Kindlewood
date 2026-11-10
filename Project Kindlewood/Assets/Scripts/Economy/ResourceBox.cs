using System.Collections.Generic;
using UnityEngine;

namespace Economy
{
    public class ResourceBox : MonoBehaviour
    {
        private readonly Dictionary<string, int> _storedResources = new Dictionary<string, int>();

        public void DepositResources(string resourceType, int amount)
        {
            if (_storedResources.ContainsKey(resourceType))
            {
                _storedResources[resourceType] += amount;
            }
            else
            {
                _storedResources[resourceType] = amount;
            }

            Debug.Log($"Varastoon palautettu {amount} kpl [{resourceType}]. Yhteensä varastossa: {_storedResources[resourceType]}");
        }

        public int GetResourceCount(string resourceType)
        {
            return _storedResources.TryGetValue(resourceType, out int count) ? count : 0;
        }
    }
}