using UnityEngine;

public class LungeHitbox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Enemy_Controller enemy = other.GetComponentInParent<Enemy_Controller>();

        if (enemy != null)
        {
            enemy.Caught();
        }
    }
}