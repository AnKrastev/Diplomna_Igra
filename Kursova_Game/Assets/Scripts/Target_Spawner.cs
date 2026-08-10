using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Target_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject target_prefab;
    [SerializeField] private Transform[] target_spawnPoints;
    private List<Transform> targets;

    void Start()
    { 
        
        foreach (Transform spawnPoint in target_spawnPoints.Take(4))
        {
            GameObject target = Instantiate(target_prefab, spawnPoint.position, spawnPoint.rotation);
            Target_Controller targetController = target.GetComponent<Target_Controller>();
            targetController.SetRespawnPoints(target_spawnPoints);
        }
    }
}