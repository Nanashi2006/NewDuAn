using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chapter14DynamicEnvironment
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Đánh cận chiến")]
        public float damage = 20f;
        public float attackRange = 1.5f;
        public float attackRadius = 1.25f;
        public float attackCooldown = 0.55f;

        private float nextAttackTime;

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
            {
                LakesideAnimationDriver animation = GetComponent<LakesideAnimationDriver>();
                nextAttackTime = Time.time + Mathf.Max(attackCooldown, animation != null ? animation.AttackDuration : 0f);
                Attack();
            }
        }

        private void Attack()
        {
            Vector3 center = transform.position + Vector3.up * 1f + transform.forward * attackRange;
            Collider[] hits = Physics.OverlapSphere(
                center,
                attackRadius,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
            foreach (Collider hit in hits)
            {
                EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth == null || !damagedEnemies.Add(enemyHealth))
                    continue;

                enemyHealth.TakeDamage(damage);
            }

            LakesideAnimationDriver animation = GetComponent<LakesideAnimationDriver>();
            if (animation != null) animation.PlayAttack();

            CharacterPose pose = GetComponent<CharacterPose>();
            if (pose != null) pose.Swing();
        }

    }
}
