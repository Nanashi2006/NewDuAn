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

        private void LateUpdate()
        {
            if (target == null)
                return;

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                {
                    Vector2 delta = mouse.delta.ReadValue();
                    yaw += delta.x * mouseSensitivity;
                    pitch -= delta.y * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch, 8f, 65f);
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance = Mathf.Clamp(distance - scroll * 0.01f, minDistance, maxDistance);
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
