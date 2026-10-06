using System.Collections;
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
                nextAttackTime = Time.time + attackCooldown;
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

            StartCoroutine(ShowAttackFlash(center));
        }

        private IEnumerator ShowAttackFlash(Vector3 worldPosition)
        {
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.name = "AttackFlash";
            flash.transform.position = worldPosition;
            flash.transform.localScale = Vector3.one * attackRadius * 2f;

            Collider col = flash.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            Renderer renderer = flash.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                    shader = Shader.Find("Universal Render Pipeline/Lit");

                if (shader != null)
                {
                    Material material = new Material(shader);
                    Color flashColor = new Color(1f, 0.55f, 0.12f, 0.22f);
                    if (material.HasProperty("_BaseColor"))
                        material.SetColor("_BaseColor", flashColor);
                    renderer.material = material;
                }
            }

            yield return new WaitForSeconds(0.08f);
            Destroy(flash);
        }
    }
}
