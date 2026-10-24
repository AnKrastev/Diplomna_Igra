using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Target_Spawner : MonoBehaviour
{
    [Header("Target settings")]
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private Transform[] target_spawnPoints;
    [SerializeField] private GameManager game_manager;
    [SerializeField] private int spawned_targets = 4;

    void Start()
    {
        HashSet<Transform> occupied = new HashSet<Transform>();

        IEnumerable<Transform> start_points = target_spawnPoints.OrderBy(x => Random.value).Take(spawned_targets);

        foreach (Transform spawnPoint in start_points)
        {
            GameObject target = Instantiate(target_prefab, spawnPoint.position, spawnPoint.rotation);
            Target_Controller targetController = target.GetComponent<Target_Controller>();
            targetController.SetRespawnPoints(target_spawnPoints, occupied, spawnPoint);
            targetController.SetGameManager(game_manager);
        }
    }
}