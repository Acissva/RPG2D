using UnityEngine;

namespace RPG2D
{
    public class CameraController : MonoBehaviour
    {
        public bool follow;
        public Transform followTarget;
        public float followSpeed = 0.1f;

        private void Update()
        {
            Follow();
        }

        private void Follow()
        {
            if (follow)
            {
                if (followTarget == null) return;

                Vector2 currentTargetPos = followTarget.position;
                Vector3 targetPos = new Vector3(currentTargetPos.x, currentTargetPos.y, transform.position.z);

                if ((transform.position - targetPos).sqrMagnitude >= 0.0001f)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
                }
            }
        }

    }
}

