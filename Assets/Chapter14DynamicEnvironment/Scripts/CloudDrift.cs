using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    public class CloudDrift : MonoBehaviour
    {
        public Vector3 direction = new Vector3(1f, 0f, 0.25f);
        public float speed = 1.8f;
        public float wrapExtent = 75f;

        private void Update()
        {
            Vector3 moveDirection = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.right;

            transform.position += moveDirection * speed * Time.deltaTime;

            Vector3 position = transform.position;
            if (position.x > wrapExtent) position.x = -wrapExtent;
            if (position.x < -wrapExtent) position.x = wrapExtent;
            if (position.z > wrapExtent) position.z = -wrapExtent;
            if (position.z < -wrapExtent) position.z = wrapExtent;
            transform.position = position;
        }
    }
}
