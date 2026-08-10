using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy_Spawner : MonoBehaviour
{
    [SerializeField] private GameObject enemy_prefab;
    [SerializeField] private Transform[] spawnPoints;
    private List<Transform> targets;

    void Start()
    { 
        targets = GameObject.FindGameObjectsWithTag("Target").Select(go => go.transform).ToList();
        Debug.Log($"Targets found: {targets.Count}");


        foreach (Transform spawnPoint in spawnPoints)
        {
            GameObject enemy = Instantiate(enemy_prefab, spawnPoint.position, spawnPoint.rotation);
            Enemy_Controller enemyController = enemy.GetComponent<Enemy_Controller>();

            if (enemyController != null && targets.Count > 0) 
            {
                int randomIndex = Random.Range(0, targets.Count);
                Transform randomTarget = targets[randomIndex];

                enemyController.SetTarget(randomTarget);
                targets.RemoveAt(randomIndex);
            }

        }
    }
}
