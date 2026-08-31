using QFramework;
using UnityEngine;  //得留着

namespace RPG2D
{
    public class QuitGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
