using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Economy.Resources.EconomyUI
{
    public class ResourceSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI amountText;

        public void SetData(string resourceType, int amount, Sprite icon = null)
        {
            if (nameText)
                nameText.text = resourceType;

            UpdateAmount(amount);

            if (!iconImage) return;
            iconImage.sprite = icon;
            // Hide image-element, if no icon
            iconImage.gameObject.SetActive(icon);
        }

        public void UpdateAmount(int amount)
        {
            if (amountText)
                amountText.text = amount.ToString();
        }
    }
}