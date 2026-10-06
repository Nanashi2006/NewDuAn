using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Player")]
        public Transform playerTransform;

        [Header("AI")]
        public float chaseRange = 18f;
        public float returnDistance = 0.7f;
        public float repathInterval = 0.15f;

        [Header("Tầm nhìn")]
        public float viewDistance = 22f;
        [Range(1f, 360f)]
        public float viewAngle = 170f;
        public Vector3 eyeOffset = new Vector3(0f, 1.5f, 0f);
        public LayerMask obstacleMask = ~0;

        private NavMeshAgent agent;
        private EnemyHealth health;
        private Vector3 startPosition;
        private float nextRepathTime;

        public bool IsDead => health != null && health.IsDead;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<EnemyHealth>();
        }

        private void Start()
        {
            startPosition = transform.position;

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (agent == null || !agent.enabled || IsDead)
                return;

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
                return;
            }

            if (!agent.isOnNavMesh)
                return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool closeEnoughToNotice = distanceToPlayer <= 6f;
            bool shouldChase = distanceToPlayer <= chaseRange && (closeEnoughToNotice || CanSeePlayer());

            if (Time.time < nextRepathTime)
                return;

            nextRepathTime = Time.time + repathInterval;

            if (shouldChase)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                return;
            }

            if (Vector3.Distance(transform.position, startPosition) > returnDistance)
            {
                agent.isStopped = false;
                agent.SetDestination(startPosition);
            }
            else
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        public bool CanSeePlayer()
        {
            if (playerTransform == null || IsDead)
                return false;

            Vector3 enemyEye = transform.position + eyeOffset;
            Vector3 playerEye = playerTransform.position + eyeOffset;
            float distanceToPlayer = Vector3.Distance(enemyEye, playerEye);

            if (distanceToPlayer > viewDistance)
                return false;

            Vector3 directionToPlayer = (playerEye - enemyEye).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > viewAngle * 0.5f)
                return false;

            if (Physics.Raycast(
                    enemyEye,
                    directionToPlayer,
                    out RaycastHit hit,
                    distanceToPlayer + 0.3f,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore))
            {
                return hit.transform.root == playerTransform.root;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            Gizmos.color = Color.cyan;
            Vector3 eye = transform.position + eyeOffset;
            Gizmos.DrawWireSphere(eye, 0.08f);
        }
    }
}
