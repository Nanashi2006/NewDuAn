using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment
{
    // Joint animation for the modeled knight and goblins, without external animation assets.
    public class CharacterPose : MonoBehaviour
    {
        public Transform leftArm, rightArm, leftLeg, rightLeg;
        public Transform body;
        private CharacterController controller;
        private NavMeshAgent agent;
        private EnemyHealth health;
        private Quaternion armRest;
        private Vector3 bodyRest;
        private float phase, swingUntil;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<EnemyHealth>();
            if (rightArm != null) armRest = rightArm.localRotation;
            if (body != null) bodyRest = body.localPosition;
        }

        public void Swing() { swingUntil = Time.time + 0.36f; }

        private void LateUpdate()
        {
            if (health != null && health.IsDead) return;
            float speed = controller != null ? new Vector2(controller.velocity.x, controller.velocity.z).magnitude
                : agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
            phase += Time.deltaTime * Mathf.Lerp(2f, 11f, Mathf.Clamp01(speed / 5f));
            float angle = Mathf.Sin(phase) * Mathf.Clamp(speed * 7f, 0f, 32f);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(angle, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-angle * 0.6f, 0f, 0f);
            if (rightArm != null)
            {
                float swing = Mathf.Clamp01((swingUntil - Time.time) / 0.36f);
                rightArm.localRotation = armRest * Quaternion.Euler(angle * 0.6f - Mathf.Sin(swing * Mathf.PI) * 110f, 0f, -swing * 25f);
            }
            if (body != null)
                body.localPosition = bodyRest + Vector3.up * (Mathf.Sin(phase * 2f) * Mathf.Clamp(speed * 0.008f, 0f, 0.04f));
        }
    }
}
