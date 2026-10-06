using UnityEngine;

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
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Combat")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private LayerMask enemyLayer;

    [Header("View")]
    [SerializeField] private GameObject thirdPersonBody;
    [SerializeField] private Transform firstPersonAnchor;
    [SerializeField] private Transform thirdPersonAnchor;

    private float verticalVelocity;
    private float pitch;
    private bool firstPerson;
    private bool attacking;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (playerCamera == null) playerCamera = Camera.main;
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
        if (!Input.GetKeyDown(KeyCode.F5)) return;
        firstPerson = !firstPerson;
        if (thirdPersonBody != null) thirdPersonBody.SetActive(!firstPerson);
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
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        pitch = Mathf.Clamp(pitch - mouseY, -80f, 80f);

        Transform anchor = firstPerson ? firstPersonAnchor : thirdPersonAnchor;
        if (anchor != null)
        {
            playerCamera.transform.position = anchor.position;
            playerCamera.transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
        }
    }

    private void HandleMovement()
    {
        if (controller == null) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z).normalized;

        bool running = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.01f;
        float speed = running ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * input.x + transform.forward * input.z) * speed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null) animator.SetTrigger(JumpHash);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, input.magnitude);
            animator.SetBool(RunHash, running);
        }
    }

    private void HandleCombat()
    {
        if (!Input.GetMouseButtonDown(0) || attacking) return;
        attacking = true;
        if (animator != null) animator.SetTrigger(AttackHash);
        PerformAttack();
        Invoke(nameof(ResetAttack), 0.35f);
    }

    private void PerformAttack()
    {
        Transform origin = attackOrigin != null ? attackOrigin : transform;
        Collider[] hits = Physics.OverlapSphere(origin.position + origin.forward * attackRange, attackRadius, enemyLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            LakesideEnemy enemy = hits[i].GetComponentInParent<LakesideEnemy>();
            if (enemy != null) enemy.TakeDamage(attackDamage);
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
