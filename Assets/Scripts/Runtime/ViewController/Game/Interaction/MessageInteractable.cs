using UnityEngine;

namespace RPG2D
{
    public class MessageInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionId = "message_gt_001";
        [SerializeField] private string promptText = "查看";
        [SerializeField, TextArea] private string message = "这里还没有配置内容";

        public string InteractionId => interactionId;

        public string PromptText => promptText;

        public bool CanInteract(GameObject interactor)
        {
            return isActiveAndEnabled;
        }

        public void Interact(GameObject interactor)
        {
            Debug.Log($"{message}：{InteractionId}");
        }
    }
}
