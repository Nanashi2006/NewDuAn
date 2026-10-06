using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LakesideEnemy : MonoBehaviour
{
    [Header("AI")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float returnDistance = 0.25f;
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private float mudSpeed = 1.5f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject healthCanvas;
    [SerializeField] private float disappearDelay = 2f;

    private int currentHealth;
    private Vector3 startPosition;
    private bool dead;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (player == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null) player = taggedPlayer.transform;
        }

        startPosition = transform.position;
        currentHealth = maxHealth;
        if (agent != null) agent.speed = normalSpeed;
        RefreshHealthUI();
    }

    private void Update()
    {
        if (dead || agent == null || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRange)
            agent.SetDestination(player.position);
        else if (Vector3.Distance(transform.position, startPosition) > returnDistance)
            agent.SetDestination(startPosition);
        else
            agent.ResetPath();

        if (animator != null)
            animator.SetFloat(SpeedHash, agent.velocity.magnitude);
    }

    public void TakeDamage(int amount)
    {
        if (dead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        RefreshHealthUI();
        if (currentHealth <= 0) Die();
    }

    public void SetMud(bool insideMud)
    {
        if (agent != null && !dead)
            agent.speed = insideMud ? mudSpeed : normalSpeed;
    }

    private void RefreshHealthUI()
    {
        if (healthSlider == null) return;
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    private void Die()
    {
        dead = true;
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        Canvas worldHealth = GetComponentInChildren<Canvas>(true);
        if (worldHealth != null) worldHealth.gameObject.SetActive(false);
        else if (healthCanvas != null && healthCanvas != gameObject) healthCanvas.SetActive(false);

        if (animator != null) animator.SetTrigger(DieHash);

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;

        Destroy(gameObject, disappearDelay);
    }
}
