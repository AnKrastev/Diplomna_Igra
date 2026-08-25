using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Enemy_Spawner : MonoBehaviour
{
    [Header("Enemy settings")]
    [SerializeField] private GameObject enemy_prefab;
    [SerializeField] private Transform[] spawn_points;
    [SerializeField] private GameManager game_manager;
    private List<Transform> targets;


    void Start()
    { 
        targets = GameObject.FindGameObjectsWithTag("Target").Select(go => go.transform).ToList();


        foreach (Transform spawnPoint in spawn_points)
        {
            GameObject enemy = Instantiate(enemy_prefab, spawnPoint.position, spawnPoint.rotation);
            Enemy_Controller enemyController = enemy.GetComponent<Enemy_Controller>();
            enemyController.SetGameManager(game_manager);

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
