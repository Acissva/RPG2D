using UnityEngine;

namespace RPG2D
{
    public class OldInputReader : IInputReader
    {
        private readonly InputBuffer inputBuffer = new InputBuffer(3);

        public Vector2 MoveDir { get; private set; }
        public Vector2 PointerScreenPosition { get; private set; }

        public void OnUpdate()
        {
            PointerScreenPosition = Input.mousePosition;
            GetAction();
            inputBuffer.ClearExpired();
        }

        private void GetMoveDir()
        {
            //GetAxis自带缓冲，想立即停下用GetAxisRaw
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveDir = new Vector2(h, v).normalized;
        }

        public bool HasAction(InputActionType action)
        {
            return inputBuffer.Has(action);
        }

        public bool ConsumeAction(InputActionType action)
        {
            return inputBuffer.Consume(action);
        }

        private void GetAction()
        {
            GetMoveDir();

            if (Input.GetMouseButtonDown(0))
            {
                inputBuffer.Press(InputActionType.Attack);
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
            {
                inputBuffer.Press(InputActionType.Interact);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                inputBuffer.Press(InputActionType.UIBack);
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                inputBuffer.Press(InputActionType.UIDebugSnapshot);
            }
        }
    }
}
