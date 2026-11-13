using System;
using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(fileName = "NpcChatConfig", menuName = "RPG2D/Config/Game/Npc/NPC Chat Config")]
    public class NpcChatConfig : ScriptableObject
    {
        [SerializeField] private string npcId = "npc_ai_guide_001";
        [SerializeField] private string displayName = "小镇向导";
        [SerializeField, TextArea(2, 5)] private string welcomeMessage = "你好，我是小镇向导。你想了解小镇的什么事情？";

        public string NpcId => npcId;
        public string DisplayName => displayName;
        public string WelcomeMessage => welcomeMessage;

        public bool TryValidate(out string error)
        {
            error = string.Empty;
            if (!string.Equals(npcId, "npc_ai_guide_001", StringComparison.Ordinal))
            {
                error = "本版 AI 向导的 NPC ID 必须为 npc_ai_guide_001。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(welcomeMessage))
            {
                error = "AI 向导的显示名和欢迎语不能为空或全为空白。";
                return false;
            }

            return true;
        }
    }
}
