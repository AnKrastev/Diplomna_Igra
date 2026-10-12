using UnityEngine;
using UnityEngine.AI;

public class Enemy_Controller : MonoBehaviour
{
    [Header("Evade Settings")]
    [SerializeField] private float evade_radius = 8f;
    [SerializeField] private float evade_exit_multiplier = 1.25f;
    [SerializeField] private float evade_speed_multiplier = 1.3f;
    [SerializeField] private float evade_step_distance = 8f;
    [SerializeField] private int evade_candidates = 12;
    [SerializeField] private float target_bias = 0.5f;
    [SerializeField] private float hidden_bonus = 6f;
    [SerializeField] private float reevaluate_interval = 0.25f;
    [SerializeField] private float eye_height = 1.5f;
    [SerializeField] private LayerMask sight_blockers = ~0;

    private NavMeshAgent agent;
    private Transform target;
    private Transform player;
    private GameManager game_manager;

    private float base_speed;
    private bool is_evading = false;
    private float evade_timer = 0f;
    private bool caught = false;


    public void SetTarget(Transform new_target)
    {
        target = new_target;
    }

    public void SetPlayer(Transform new_player)
    {
        player = new_player;
    }

    public void SetGameManager(GameManager manager)
    {
        game_manager = manager;
    }

    public void Caught()
    {
        if (caught)
        {
            return;
        }

        caught = true;
        game_manager.EnemyIncrement();
        Destroy(gameObject);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.Warp(transform.position);
        base_speed = agent.speed;

        if (player == null)
        {
            GameObject player_object = GameObject.FindGameObjectWithTag("Player");
            if (player_object != null)
            {
                player = player_object.transform;
            }
        }
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        UpdateEvadeState();

        if (is_evading)
        {
            evade_timer -= Time.deltaTime;
            if (evade_timer <= 0f)
            {
                evade_timer = reevaluate_interval;
                agent.SetDestination(FindEvadePoint());
            }
        }
        else
        {
            agent.SetDestination(target.position);
        }
    }

    void UpdateEvadeState()
    {
        if (player == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (!is_evading && distance < evade_radius)
        {
            is_evading = true;
            evade_timer = 0f;
            agent.speed = base_speed * evade_speed_multiplier;
        }
        else if (is_evading && distance > evade_radius * evade_exit_multiplier)
        {
            is_evading = false;
            agent.speed = base_speed;
        }
    }


    Vector3 FindEvadePoint()
    {
        Vector3 my_position = transform.position;
        Vector3 player_position = player.position;
        Vector3 player_eye = player_position + Vector3.up * eye_height;
        float current_distance = Vector3.Distance(my_position, player_position);

        Vector3 best_point = target.position;
        float best_score = float.NegativeInfinity;
        bool found = false;

        for (int i = 0; i <= evade_candidates; i++)
        {
            Vector3 candidate;

            if (i == evade_candidates)
            {
                candidate = target.position;
            }
            else
            {
                float angle = i * (360f / evade_candidates);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                candidate = my_position + direction * evade_step_distance;
            }

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                continue;
            }

            candidate = hit.position;

            float player_distance = Vector3.Distance(candidate, player_position);

            if (player_distance < current_distance - 0.5f)
            {
                continue;
            }

            float score = player_distance;
            score -= target_bias * Vector3.Distance(candidate, target.position);

            if (Physics.Linecast(player_eye, candidate + Vector3.up * eye_height, sight_blockers, QueryTriggerInteraction.Ignore))
            {
                score += hidden_bonus;
            }

            if (score > best_score)
            {
                best_score = score;
                best_point = candidate;
                found = true;
            }
        }

        return found ? best_point : target.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, evade_radius);
    }
}