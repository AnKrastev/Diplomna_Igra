using UnityEngine;

public class Target_Controller : MonoBehaviour
{
    [SerializeField] private float respawnDelay = 3f;

    private Transform[] respawnPoints;

    public void SetRespawnPoints(Transform[] points)
    {
        respawnPoints = points;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name}");
        gameObject.SetActive(false);
        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        if (respawnPoints != null && respawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, respawnPoints.Length);
            transform.position = respawnPoints[randomIndex].position;
        }

        gameObject.SetActive(true);
    }
}