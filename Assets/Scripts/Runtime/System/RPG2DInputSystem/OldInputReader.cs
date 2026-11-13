using UnityEngine;

namespace RPG2D
{
    public class OldInputReader : IInputReader
    {
        private readonly InputBuffer inputBuffer = new InputBuffer(3);
        private bool wasComposing;
        private int discardedFrame = -1;

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
            if (Time.frameCount == discardedFrame)
            {
                MoveDir = Vector2.zero;
                return;
            }
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

        public void DiscardActionsForCurrentFrame()
        {
            discardedFrame = Time.frameCount;
            MoveDir = Vector2.zero;
            inputBuffer.DiscardForCurrentFrame();
        }

        private bool CanQueueUiBack(string composition)
        {
            bool composing = !string.IsNullOrEmpty(composition);
            bool canQueue = !composing && !wasComposing;
            wasComposing = composing;
            if (!canQueue) inputBuffer.Consume(InputActionType.UIBack);
            return canQueue;
        }

        private void GetAction()
        {
            GetMoveDir();
            bool canQueueUiBack = CanQueueUiBack(Input.compositionString);

            if (Input.GetMouseButtonDown(0))
            {
                inputBuffer.Press(InputActionType.Attack);
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.E))
            {
                inputBuffer.Press(InputActionType.Interact);
            }

            if (Input.GetKeyDown(KeyCode.Escape) && canQueueUiBack)
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
