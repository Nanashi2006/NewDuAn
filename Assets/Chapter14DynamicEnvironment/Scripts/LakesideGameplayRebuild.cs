using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LakesideGameplayRebuild : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("Combat")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer = ~0;

    [Header("View")]
    [SerializeField] private GameObject thirdPersonBody;
    [SerializeField] private Transform firstPersonAnchor;
    [SerializeField] private Transform thirdPersonAnchor;

    private float verticalVelocity;
    private float pitch;
    private bool firstPerson;
    private bool attacking;
    private bool hasSpeed, hasRun, hasJump, hasAttack;
    private LakesideAnimationDriver animationDriver;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (playerCamera == null) playerCamera = Camera.main;
        animationDriver = GetComponent<LakesideAnimationDriver>();
        if (animator != null && animator.runtimeAnimatorController != null)
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == SpeedHash && parameter.type == AnimatorControllerParameterType.Float) hasSpeed = true;
                if (parameter.nameHash == RunHash && parameter.type == AnimatorControllerParameterType.Bool) hasRun = true;
                if (parameter.nameHash == JumpHash && parameter.type == AnimatorControllerParameterType.Trigger) hasJump = true;
                if (parameter.nameHash == AttackHash && parameter.type == AnimatorControllerParameterType.Trigger) hasAttack = true;
            }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleViewToggle();
        HandleLook();
        HandleMovement();
        HandleCombat();
    }

    private void HandleViewToggle()
    {
        if (Keyboard.current == null || !Keyboard.current.f5Key.wasPressedThisFrame) return;
        firstPerson = !firstPerson;
        if (thirdPersonBody != null)
            foreach (Renderer body in thirdPersonBody.GetComponentsInChildren<Renderer>(true)) body.forceRenderingOff = firstPerson;
        SnapCameraToActiveAnchor();
    }

    private void SnapCameraToActiveAnchor()
    {
        if (playerCamera == null) return;
        Transform anchor = firstPerson ? firstPersonAnchor : thirdPersonAnchor;
        if (anchor == null) return;
        playerCamera.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    private void HandleLook()
    {
        if (playerCamera == null || Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(Vector3.up * delta.x);
        pitch = Mathf.Clamp(pitch - delta.y, -80f, 80f);

        Transform anchor = firstPerson ? firstPersonAnchor : thirdPersonAnchor;
        if (anchor != null)
        {
            playerCamera.transform.position = anchor.position;
            playerCamera.transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
        }
    }

    private void HandleMovement()
    {
        if (controller == null || Keyboard.current == null) return;

        float x = 0f;
        float z = 0f;
        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;
        if (Keyboard.current.wKey.isPressed) z += 1f;

        Vector3 input = new Vector3(x, 0f, z).normalized;
        bool running = Keyboard.current.leftShiftKey.isPressed && input.sqrMagnitude > 0.01f;
        float speed = running ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * input.x + transform.forward * input.z) * speed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null && hasJump) animator.SetTrigger(JumpHash);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        if (animator != null)
        {
            if (hasSpeed) animator.SetFloat(SpeedHash, input.magnitude);
            if (hasRun) animator.SetBool(RunHash, running);
        }
    }

    private void HandleCombat()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || attacking) return;
        attacking = true;
        if (animator != null && hasAttack) animator.SetTrigger(AttackHash);
        if (animationDriver != null) animationDriver.PlayAttack();
        PerformAttack();
        Invoke(nameof(ResetAttack), animationDriver != null ? animationDriver.AttackDuration : 0.65f);
    }

    private void PerformAttack()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Vector3 center = origin.position + origin.forward * attackRange;
        Collider[] hits = Physics.OverlapSphere(center, attackRadius, enemyLayer, QueryTriggerInteraction.Collide);
        HashSet<LakesideEnemy> damaged = new HashSet<LakesideEnemy>();

        for (int i = 0; i < hits.Length; i++)
        {
            LakesideEnemy enemy = hits[i].GetComponentInParent<LakesideEnemy>();
            if (enemy != null && damaged.Add(enemy))
                enemy.TakeDamage(attackDamage);
        }
    }

    private void ResetAttack()
    {
        attacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Gizmos.DrawWireSphere(origin.position + origin.forward * attackRange, attackRadius);
    }
}
