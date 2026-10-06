using UnityEngine;

public class LakesideMudZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LakesideEnemy enemy = other.GetComponentInParent<LakesideEnemy>();
        if (enemy != null) enemy.SetMud(true);
    }

    private void OnTriggerExit(Collider other)
    {
        LakesideEnemy enemy = other.GetComponentInParent<LakesideEnemy>();
        if (enemy != null) enemy.SetMud(false);
    }
}
