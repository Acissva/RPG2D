using UnityEngine;

namespace RPG2D
{
    public interface IInputReader
    {
        void OnUpdate();

        bool HasAction(InputActionType action);

        bool ConsumeAction(InputActionType action);

        Vector2 MoveDir { get; }

        Vector2 PointerScreenPosition { get; }
    }

    public enum InputActionType
    {
        Attack,
        Interact,
        UIBack,
        UIDebugSnapshot,
    }
}
