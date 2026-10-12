using UnityEngine;
using UnityEngine.AI;

public class CandleKnightAnim : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
