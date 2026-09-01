using System;
using UnityEngine;

namespace RPG2D
{
    [Serializable]
    public class DialogueLine
    {
        [SerializeField] private string speakerName = "镇民";
        [SerializeField, TextArea(2, 4)] private string text = "这里还没有配置对话内容。";

        [SerializeField] private string speakerNameKey;
        [SerializeField] private string textKey;

        public string SpeakerName => speakerName;
        public string Text => text;
        public string SpeakerNameKey => speakerNameKey;
        public string TextKey => textKey;
    }
}
