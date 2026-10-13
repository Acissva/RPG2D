using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPG2D.Learning
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(EnemyMotor2D))]
    public class LearningPathFollower2D : MonoBehaviour
    {
        private const float WaypointTolerance = 0.05f;
        private const float SampleDistance = 0.15f;

        [SerializeField] private NavMeshSurface surface;
        [SerializeField] private BoxCollider2D agentCollider;
        [SerializeField] private Transform target;
        [SerializeField, Min(0.05f)] private float replanInterval = 0.3f;

        private readonly List<Vector2> path = new List<Vector2>();
        private EnemyController enemyController;
        private EnemyMotor2D motor;
        private EnemyBrain brain;
        private NavMeshPath navPath;
        private NavMeshPathStatus? reportedStatus;
        private int waypointIndex;
        private float nextReplanTime;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            motor = GetComponent<EnemyMotor2D>();
            brain = GetComponent<EnemyBrain>();
            navPath = new NavMeshPath();
        }

        private void FixedUpdate()
        {
            if (brain != null && brain.enabled)
            {
                Debug.LogError("LearningPathFollower2D: 请关闭此学习实例的 EnemyBrain，避免两个驱动控制移动。", this);
                enabled = false;
                return;
            }

            if (surface == null || !surface.isActiveAndEnabled || surface.navMeshData == null || !surface.navMeshDataInstance.valid || agentCollider == null || !agentCollider.isActiveAndEnabled || target == null || !target.gameObject.activeInHierarchy || !motor.enabled || enemyController == null || enemyController.Stats == null || enemyController.Stats.IsDead)
            {
                ClearPath();
                nextReplanTime = 0f;
                return;
            }

            if (motor.IsKnockbackActive)
            {
                ClearPath();
                nextReplanTime = 0f;
                return;
            }

            if (Time.fixedTime >= nextReplanTime)
            {
                nextReplanTime = Time.fixedTime + replanInterval;
                Replan();
            }

            while (waypointIndex < path.Count && ((Vector2)transform.position - path[waypointIndex]).sqrMagnitude <= WaypointTolerance * WaypointTolerance)
            {
                waypointIndex++;
            }

            if (waypointIndex >= path.Count)
            {
                motor.Stop();
                return;
            }

            Vector2 waypoint = path[waypointIndex];
            float distance = Vector2.Distance(transform.position, waypoint);
            float speed = Mathf.Min(enemyController.Stats.ChaseSpeed, distance / Time.fixedDeltaTime);
            motor.MoveTo(waypoint, speed);
        }

        private void Replan()
        {
            ClearPath();

            // 导航使用实体中心；Transform 和 TargetMarker 仍表示脚底位置。
            Vector2 colliderOffset = (Vector2)agentCollider.bounds.center - (Vector2)transform.position;
            Vector2 startCenter = (Vector2)transform.position + colliderOffset;
            Vector2 goalCenter = (Vector2)target.position + colliderOffset;
            Vector3 startPoint = new Vector3(startCenter.x, startCenter.y, surface.transform.position.z);
            Vector3 goalPoint = new Vector3(goalCenter.x, goalCenter.y, surface.transform.position.z);
            NavMeshQueryFilter filter = new NavMeshQueryFilter { agentTypeID = surface.agentTypeID, areaMask = NavMesh.AllAreas };

            if (!NavMesh.SamplePosition(startPoint, out NavMeshHit startHit, SampleDistance, filter) || !NavMesh.SamplePosition(goalPoint, out NavMeshHit goalHit, SampleDistance, filter))
            {
                ReportStatus(NavMeshPathStatus.PathInvalid);
                return;
            }

            bool found = NavMesh.CalculatePath(startHit.position, goalHit.position, filter, navPath);
            NavMeshPathStatus status = found ? navPath.status : NavMeshPathStatus.PathInvalid;
            if (status != NavMeshPathStatus.PathComplete)
            {
                ReportStatus(status);
                return;
            }

            Vector3[] corners = navPath.corners; // 每次重规划只读取一次，不在每个物理帧读取数组。
            if (corners.Length == 0)
            {
                ReportStatus(NavMeshPathStatus.PathInvalid);
                return;
            }

            for (int i = 0; i < corners.Length; i++)
            {
                path.Add((Vector2)corners[i] - colliderOffset);
            }

            // 保留起点投影产生的微小修正；终点使用网格上的结果，不追加原始目标。
            ReportStatus(status);
        }

        private void ReportStatus(NavMeshPathStatus status)
        {
            if (reportedStatus == status)
            {
                return;
            }

            reportedStatus = status;
            Debug.Log($"LearningPathFollower2D: {status}，可跟随路径点 {path.Count}。", this);
        }

        private void ClearPath()
        {
            path.Clear();
            waypointIndex = 0;
            navPath?.ClearCorners();
            if (motor != null)
            {
                motor.Stop();
            }
        }

        private void OnDisable()
        {
            ClearPath();
            nextReplanTime = 0f;
            reportedStatus = null;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || waypointIndex >= path.Count)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Vector3 previous = transform.position;
            for (int i = waypointIndex; i < path.Count; i++)
            {
                Vector3 next = path[i];
                Gizmos.DrawLine(previous, next);
                Gizmos.DrawSphere(next, 0.06f);
                previous = next;
            }
        }
    }
}
