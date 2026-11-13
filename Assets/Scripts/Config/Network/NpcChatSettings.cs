using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(fileName = "NpcChatSettings", menuName = "RPG2D/Config/Network/NPC Chat Settings")]
    public class NpcChatSettings : ScriptableObject
    {
        [SerializeField, Range(1, 120)] private int timeoutSeconds = 70;

        public int TimeoutSeconds => timeoutSeconds;

        public bool TryValidate(out string error)
        {
            error = string.Empty;
            if (timeoutSeconds < 1 || timeoutSeconds > 120)
            {
                error = "AI 交谈超时必须为 1～120 秒。";
                return false;
            }

            return true;
        }
    }
}
