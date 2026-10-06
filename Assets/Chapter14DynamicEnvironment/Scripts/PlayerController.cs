using UnityEngine;
using UnityEngine.InputSystem;

namespace Chapter14DynamicEnvironment
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Di chuyển")]
        public float walkSpeed = 5f;
        public float runSpeed = 8f;
        public float rotationSpeed = 12f;

        [Header("Nhảy / trọng lực")]
        public float jumpHeight = 1.8f;
        public float gravity = -20f;

        [Header("Bơi lội")]
        [Range(0.1f, 1f)]
        public float swimmingSpeedMultiplier = 0.45f;
        [Range(0.1f, 1f)]
        public float swimmingGravityMultiplier = 0.35f;

        public bool IsSwimming { get; private set; }

        private CharacterController controller;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || controller == null)
                return;

            Vector2 input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                forward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
            }

            Vector3 moveDirection = forward * input.y + right * input.x;
            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            bool running = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            float moveSpeed = running ? runSpeed : walkSpeed;
            if (IsSwimming)
                moveSpeed *= swimmingSpeedMultiplier;

            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (keyboard.spaceKey.wasPressedThisFrame && (controller.isGrounded || IsSwimming))
            {
                float jumpMultiplier = IsSwimming ? 0.65f : 1f;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity) * jumpMultiplier;
            }

            float gravityMultiplier = IsSwimming ? swimmingGravityMultiplier : 1f;
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        public void SetSwimming(bool swimming)
        {
            IsSwimming = swimming;
        }
    }
}
