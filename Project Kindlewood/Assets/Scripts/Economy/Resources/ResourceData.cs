using UnityEngine;

namespace Economy.Resources
{
    [CreateAssetMenu(fileName = "NewResource", menuName = "Resources/Resource Data")]
    public class ResourceData : ScriptableObject
    {
        public string resourceName;
        public Sprite icon;
    }
}