using UnityEngine;
using UnityEngine.InputSystem;

namespace Chapter14DynamicEnvironment
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
        public float distance = 7f;
        public float minDistance = 3f;
        public float maxDistance = 11f;
        public float yaw = 35f;
        public float pitch = 22f;
        public float mouseSensitivity = 0.15f;
        public float smoothSpeed = 12f;

        public bool IsFirstPerson { get; private set; }
        public Vector3 firstPersonOffset = new Vector3(0f, 1.75f, 0.12f);
        private Renderer[] bodyRenderers;
        private CursorLockMode savedLock;
        private bool savedVisible;

        private void Start()
        {
            if (target != null) bodyRenderers = target.GetComponentsInChildren<Renderer>(true);
            savedLock = Cursor.lockState;
            savedVisible = Cursor.visible;
        }

        private void Update()
        {
            if (target == null || Keyboard.current == null) return;
            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                IsFirstPerson = !IsFirstPerson;
                if (bodyRenderers != null)
                    foreach (Renderer body in bodyRenderers) if (body != null) body.forceRenderingOff = IsFirstPerson;
                Cursor.lockState = IsFirstPerson ? CursorLockMode.Locked : savedLock;
                Cursor.visible = IsFirstPerson ? false : savedVisible;
                if (IsFirstPerson) yaw = target.eulerAngles.y;
                pitch = Mathf.Clamp(pitch, IsFirstPerson ? -80f : 8f, IsFirstPerson ? 80f : 65f);
            }
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnDisable()
        {
            if (bodyRenderers != null)
                foreach (Renderer body in bodyRenderers) if (body != null) body.forceRenderingOff = false;
            Cursor.lockState = savedLock;
            Cursor.visible = savedVisible;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed || (IsFirstPerson && Cursor.lockState == CursorLockMode.Locked))
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    yaw += delta.x * mouseSensitivity;
                    pitch -= delta.y * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch, IsFirstPerson ? -80f : 8f, IsFirstPerson ? 80f : 65f);
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance = Mathf.Clamp(distance - scroll * 0.01f, minDistance, maxDistance);
            }

            if (IsFirstPerson)
            {
                target.rotation = Quaternion.Euler(0f, yaw, 0f);
                transform.SetPositionAndRotation(target.TransformPoint(firstPersonOffset), Quaternion.Euler(pitch, yaw, 0f));
                return;
            }

            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + targetOffset;
            Vector3 desiredPosition = focusPoint - orbitRotation * Vector3.forward * distance;

            Vector3 toCamera = desiredPosition - focusPoint;
            if (Physics.SphereCast(focusPoint, 0.2f, toCamera.normalized, out RaycastHit hit,
                distance, ~(1 << 2), QueryTriggerInteraction.Ignore))
                desiredPosition = focusPoint + toCamera.normalized * Mathf.Max(0.5f, hit.distance - 0.2f);

            float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(focusPoint - transform.position, Vector3.up),
                t
            );
        }
    }
}
