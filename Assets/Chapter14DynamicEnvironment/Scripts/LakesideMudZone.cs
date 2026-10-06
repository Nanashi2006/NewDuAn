using UnityEngine;

public class LakesideMudZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        LakesideEnemy enemy = other.GetComponentInParent<LakesideEnemy>();
        if (enemy != null) enemy.SetMud(true);
        Chapter14DynamicEnvironment.EnemyController villageEnemy = other.GetComponentInParent<Chapter14DynamicEnvironment.EnemyController>();
        if (villageEnemy != null) villageEnemy.SetMud(true);
    }

    private void OnTriggerExit(Collider other)
    {
        LakesideEnemy enemy = other.GetComponentInParent<LakesideEnemy>();
        if (enemy != null) enemy.SetMud(false);
        Chapter14DynamicEnvironment.EnemyController villageEnemy = other.GetComponentInParent<Chapter14DynamicEnvironment.EnemyController>();
        if (villageEnemy != null) villageEnemy.SetMud(false);
    }
}
