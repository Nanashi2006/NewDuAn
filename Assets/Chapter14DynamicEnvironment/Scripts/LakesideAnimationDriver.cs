using UnityEngine;
using UnityEngine.AI;

// Drives the uploaded animation clips by state name. Asset demo controllers have no Speed/Run parameters.
public class LakesideAnimationDriver : MonoBehaviour
{
    public Animator animator;
    public string idleState = "Idle_Battle";
    public string walkState = "WalkForwardBattle";
    public string runState = "RunForwardBattle";
    public string attackState = "Attack01";
    public string dieState = "Die";
    private CharacterController character;
    private NavMeshAgent agent;
    private int idle, walk, run, attack, die, current;
    private float actionUntil;
    private bool dead;
    public float AttackDuration { get; private set; } = 0.65f;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        character = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        if (animator == null || animator.runtimeAnimatorController == null) return;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        idle = ValidState(idleState); walk = ValidState(walkState); run = ValidState(runState);
        attack = ValidState(attackState); die = ValidState(dieState);
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == attackState) AttackDuration = Mathf.Max(0.35f, clip.length);
        Play(idle, true);
    }

    private int ValidState(string state)
    {
        int hash = Animator.StringToHash("Base Layer." + state);
        return animator.HasState(0, hash) ? hash : 0;
    }

    private void LateUpdate()
    {
        if (dead || Time.time < actionUntil) return;
        float speed = character != null ? new Vector2(character.velocity.x, character.velocity.z).magnitude
            : agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
        int state = speed < 0.1f ? idle : speed > 4.5f && run != 0 ? run : walk;
        Play(state != 0 ? state : idle);
    }

    public void PlayAttack()
    {
        if (dead) return;
        actionUntil = Time.time + AttackDuration;
        Play(attack, true);
    }

    public bool PlayDeath()
    {
        dead = true;
        Play(die, true);
        return die != 0;
    }

    private void Play(int state, bool restart = false)
    {
        if (animator == null || state == 0 || (!restart && current == state)) return;
        current = state;
        animator.CrossFadeInFixedTime(state, 0.08f, 0, 0f);
    }
}
