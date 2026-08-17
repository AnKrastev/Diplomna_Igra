using UnityEngine;

public class Target_Controller : MonoBehaviour
{
    [SerializeField] private float respawn_delay = 3f;

    private Transform[] respawn_points;
    private GameManager game_manager;

    public void SetRespawnPoints(Transform[] points)
    {
        respawn_points = points;
    }

    public void SetGameManager(GameManager manager)
    {
        game_manager = manager;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))
        {
            Debug.Log($"Trigger entered by: {other.name}");
            gameObject.SetActive(false);
            game_manager.TargetIncrement();
            Invoke(nameof(Respawn), respawn_delay);
        }
    }

    void Respawn()
    {
        if (respawn_points != null && respawn_points.Length > 0)
        {
            int randomIndex = Random.Range(0, respawn_points.Length);
            transform.position = respawn_points[randomIndex].position;
        }

        gameObject.SetActive(true);
    }
}