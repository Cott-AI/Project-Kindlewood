using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Economy.Resources.EconomyUI
{
    [RequireComponent(typeof(Collider2D))]
    public class ResourceBoxTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject resourceBoxUIPanel;
        [SerializeField] private GameObject interactPrompt;

        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool openAutomatically; // Activate to open box when approaching without key input

        private bool _isPlayerInRange;

        private void Start()
        {
            // Hide the UI on default
            if (resourceBoxUIPanel)
                resourceBoxUIPanel.SetActive(false);

            if (interactPrompt)
                interactPrompt.SetActive(false);
        }

        private void Update()
        {
            if (!_isPlayerInRange || openAutomatically) return;

            if (WasInteractPressed())
            {
                ToggleResourceBoxUI();
            }
        }

        private bool WasInteractPressed()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                return true;
            return false;
        }


        private void ToggleResourceBoxUI()
        {
            if (!resourceBoxUIPanel) return;

            bool isOpening = !resourceBoxUIPanel.activeSelf;
            resourceBoxUIPanel.SetActive(isOpening);

            if (interactPrompt)
            {
                interactPrompt.SetActive(!isOpening);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            _isPlayerInRange = true;

            if (openAutomatically)
            {
                if (resourceBoxUIPanel)
                    resourceBoxUIPanel.SetActive(true);
            }
            else
            {
                if (interactPrompt && (!resourceBoxUIPanel || !resourceBoxUIPanel.activeSelf))
                {
                    interactPrompt.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            _isPlayerInRange = false;

            if (interactPrompt)
                interactPrompt.SetActive(false);

            if (resourceBoxUIPanel)
                resourceBoxUIPanel.SetActive(false);
        }
    }
}