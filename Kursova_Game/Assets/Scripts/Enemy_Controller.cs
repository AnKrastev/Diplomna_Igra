using UnityEngine;
using UnityEngine.AI;

public class Enemy_Controller : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform target;
    private GameManager game_manager;


    public void SetTarget(Transform new_target)
    {
        target = new_target;
    }

    public void SetGameManager(GameManager manager)
    {
        game_manager = manager;
    }

    void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))    
        {
            game_manager.EnemyIncrement();
            Destroy(gameObject);
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.Warp(transform.position);

    }

    void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
}