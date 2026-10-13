using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace RPG2D
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyMotor2D))]
    [RequireComponent(typeof(EnemyBrain))]
    public class EnemyNavigationAgent : MonoBehaviour
    {
        private const float WaypointTolerance = 0.05f;
        private const float GoalTolerance = 0.1f;
        private const float SampleDistance = 0.15f;

        [SerializeField] private bool useNavigation;
        [SerializeField] private NavMeshSurface surface;
        [SerializeField] private BoxCollider2D agentCollider;

        private readonly List<Vector2> path = new List<Vector2>();
        private EnemyController enemyController;
        private EnemyMotor2D motor;
        private EnemyBrain brain;
        private NavMeshPath navPath;
        private CombatTarget2D chaseTarget;
        private Vector2 destination;
        private Vector2 projectedDestination;
        private float moveSpeed;
        private float nextReplanTime;
        private int waypointIndex;
        private bool hasDestination;
        private bool isChasing;

        public bool UseNavigation => useNavigation;
        public Vector2 BodyCenter => agentCollider != null ? (Vector2)agentCollider.bounds.center : (Vector2)transform.position;
        public NavMeshPathStatus PathStatus { get; private set; } = NavMeshPathStatus.PathInvalid;
        public bool HasArrived => hasDestination && PathStatus == NavMeshPathStatus.PathComplete && waypointIndex >= path.Count && ((Vector2)transform.position - projectedDestination).sqrMagnitude <= WaypointTolerance * WaypointTolerance && ((Vector2)transform.position - GetDestination()).sqrMagnitude <= GoalTolerance * GoalTolerance;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            motor = GetComponent<EnemyMotor2D>();
            brain = GetComponent<EnemyBrain>();
        }

        public void ChaseTarget(CombatTarget2D target, float speed)
        {
            if (!useNavigation || !isActiveAndEnabled || target == null || !target.IsTargetable)
            {
                Stop();
                return;
            }

            if (!hasDestination || !isChasing || chaseTarget != target)
            {
                ClearWaypoints();
                nextReplanTime = 0f;
            }

            hasDestination = true;
            isChasing = true;
            chaseTarget = target;
            moveSpeed = Mathf.Max(0f, speed);
        }

        public void MoveToPoint(Vector2 position, float speed)
        {
            if (!useNavigation || !isActiveAndEnabled)
            {
                Stop();
                return;
            }

            if (!hasDestination || isChasing || (position - destination).sqrMagnitude > GoalTolerance * GoalTolerance)
            {
                ClearWaypoints();
                nextReplanTime = 0f;
            }

            hasDestination = true;
            isChasing = false;
            chaseTarget = null;
            destination = position;
            moveSpeed = Mathf.Max(0f, speed);
        }

        public void Stop()
        {
            hasDestination = false;
            isChasing = false;
            chaseTarget = null;
            nextReplanTime = 0f;
            ClearWaypoints();
        }

        private void FixedUpdate()
        {
            // 默认关闭时不接管 Motor，原追击 / 巡逻链路继续运行。
            if (!useNavigation)
            {
                if (hasDestination)
                {
                    Stop();
                }
                return;
            }

            if (!hasDestination)
            {
                motor.Stop();
                return;
            }

            if (brain == null || !brain.isActiveAndEnabled || enemyController.Stats == null || enemyController.Stats.IsDead || enemyController.Config == null || enemyController.Config.Movement == null || !motor.isActiveAndEnabled || surface == null || !surface.isActiveAndEnabled || surface.navMeshData == null || !surface.navMeshDataInstance.valid || agentCollider == null || !agentCollider.isActiveAndEnabled || (isChasing && (chaseTarget == null || !chaseTarget.IsTargetable)))
            {
                Stop();
                return;
            }

            if (motor.IsKnockbackActive)
            {
                ClearWaypoints();
                nextReplanTime = 0f;
                return;
            }

            if (Time.fixedTime >= nextReplanTime)
            {
                float interval = enemyController.Config.Movement.NavigationReplanInterval;
                if (float.IsNaN(interval) || float.IsInfinity(interval) || interval < 0.05f)
                {
                    Stop();
                    return;
                }
                nextReplanTime = Time.fixedTime + interval;
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
            motor.MoveTo(waypoint, Mathf.Min(moveSpeed, distance / Time.fixedDeltaTime));
        }

        private Vector2 GetDestination()
        {
            // TargetPosition 可用于瞄准；导航终点使用目标根节点的脚底。
            return isChasing && chaseTarget != null ? (Vector2)chaseTarget.transform.position : destination;
        }

        private void Replan()
        {
            ClearWaypoints();
            navPath ??= new NavMeshPath();

            Vector2 offset = BodyCenter - (Vector2)transform.position;
            Vector2 startCenter = (Vector2)transform.position + offset;
            Vector2 goalCenter = GetDestination() + offset;
            Vector3 startPoint = new Vector3(startCenter.x, startCenter.y, surface.transform.position.z);
            Vector3 goalPoint = new Vector3(goalCenter.x, goalCenter.y, surface.transform.position.z);
            NavMeshQueryFilter filter = new NavMeshQueryFilter { agentTypeID = surface.agentTypeID, areaMask = NavMesh.AllAreas };

            if (!NavMesh.SamplePosition(startPoint, out NavMeshHit startHit, SampleDistance, filter) || !NavMesh.SamplePosition(goalPoint, out NavMeshHit goalHit, SampleDistance, filter))
            {
                return;
            }

            bool found = NavMesh.CalculatePath(startHit.position, goalHit.position, filter, navPath);
            PathStatus = found ? navPath.status : NavMeshPathStatus.PathInvalid;
            if (PathStatus != NavMeshPathStatus.PathComplete)
            {
                return;
            }

            Vector3[] corners = navPath.corners;
            if (corners.Length == 0)
            {
                PathStatus = NavMeshPathStatus.PathInvalid;
                return;
            }

            projectedDestination = (Vector2)goalHit.position - offset;
            for (int i = 0; i < corners.Length; i++)
            {
                path.Add((Vector2)corners[i] - offset);
            }
        }

        private void ClearWaypoints()
        {
            path.Clear();
            waypointIndex = 0;
            PathStatus = NavMeshPathStatus.PathInvalid;
            navPath?.ClearCorners();
            if (motor != null)
            {
                motor.Stop();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !useNavigation || waypointIndex >= path.Count)
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
