using UnityEngine;

namespace Chapter14DynamicEnvironment
{
    [RequireComponent(typeof(Collider))]
    public class WaterVolume : MonoBehaviour
    {
        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            player.SetSwimming(true);
            Debug.Log("<color=blue>[Hệ thống] Player đã xuống nước! Tốc độ bị giảm.</color>");
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            player.SetSwimming(false);
            Debug.Log("<color=green>[Hệ thống] Player đã lên bờ! Tốc độ trở lại bình thường.</color>");
        }
    }
}
