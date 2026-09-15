using System;
using UnityEngine;

namespace RPG2D
{
    [Serializable]
    public class DialogueLine
    {
        [SerializeField] private string speakerName = "镇民";
        [SerializeField, TextArea(2, 4)] private string text = "这里还没有配置对话内容。";

        public string SpeakerName => speakerName;
        public string Text => text;

        //这里保留空构造函数，是为了让 Unity Inspector 里的序列化对象继续正常工作；
        //新增带参数构造函数，是为了运行时可以临时创建一句对话。
        public DialogueLine()
        {

        }

        public DialogueLine(string speakerName, string text)
        {
            this.speakerName = string.IsNullOrWhiteSpace(speakerName) ? "旁白" : speakerName;
            this.text = text;
        }
    }
}
