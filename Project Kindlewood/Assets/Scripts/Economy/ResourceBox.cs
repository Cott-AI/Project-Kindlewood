using System;
using System.Collections.Generic;
using UnityEngine;

namespace Economy
{
    public class ResourceBox : MonoBehaviour
    {
        private readonly Dictionary<string, int> _storedResources = new Dictionary<string, int>();

        // Tapahtuma, jota UI kuuntelee
        public event Action OnResourcesChanged;

        public void DepositResources(string resourceType, int amount)
        {
            if (!_storedResources.TryAdd(resourceType, amount))
            {
                _storedResources[resourceType] += amount;
            }

            Debug.Log($"Varastoon palautettu {amount} kpl [{resourceType}]. Yhteensä varastossa: {_storedResources[resourceType]}");

            // Ilmoitetaan UI:lle, että varaston sisältö on muuttunut
            OnResourcesChanged?.Invoke();
        }

        public int GetResourceCount(string resourceType)
        {
            return _storedResources.GetValueOrDefault(resourceType, 0);
        }

        // Antaa UI:lle lukuoikeuden kaikkiin resursseihin kerralla
        public IReadOnlyDictionary<string, int> GetAllResources()
        {
            return _storedResources;
        }
    }
}