using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Chapter14DynamicEnvironment
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Máu Enemy")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public bool destroyAfterDeath = true;
        public float destroyDelay = 2f;

        public bool IsDead { get; private set; }

        private Renderer[] renderers;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead)
                return;

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Abs(amount));
            Debug.Log($"[Enemy] Nhận {amount:0} damage. HP còn {currentHealth:0}/{maxHealth:0}");

            if (currentHealth <= 0f)
                StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            IsDead = true;

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
                col.enabled = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                foreach (Material material in renderer.materials)
                {
                    if (material != null && material.HasProperty("_BaseColor"))
                        material.SetColor("_BaseColor", new Color(0.2f, 0.2f, 0.2f, 1f));
                }
            }

            Transform visual = transform.Find("Visual");
            if (visual != null)
                visual.localRotation = Quaternion.Euler(0f, 0f, 75f);

            yield return new WaitForSeconds(destroyDelay);

            if (destroyAfterDeath)
                Destroy(gameObject);
        }

        private void OnGUI()
        {
            if (IsDead || Camera.main == null)
                return;

            Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.7f);
            if (screen.z <= 0f)
                return;

            float width = 90f;
            float height = 10f;
            float x = screen.x - width * 0.5f;
            float y = Screen.height - screen.y;

            Color previous = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(x - 2f, y - 2f, width + 4f, height + 4f), Texture2D.whiteTexture);

            GUI.color = new Color(0.55f, 0.05f, 0.05f, 1f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);

            GUI.color = new Color(0.1f, 0.85f, 0.2f, 1f);
            float ratio = maxHealth <= 0f ? 0f : currentHealth / maxHealth;
            GUI.DrawTexture(new Rect(x, y, width * ratio, height), Texture2D.whiteTexture);

            GUI.color = previous;
        }
    }
}
