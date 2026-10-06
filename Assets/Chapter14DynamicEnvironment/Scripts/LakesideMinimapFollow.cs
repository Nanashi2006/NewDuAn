using UnityEngine;

public class LakesideMinimapFollow : MonoBehaviour
{
    public Transform target;
    [SerializeField] private float height = 30f;

    private void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + Vector3.up * height;
        transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}
