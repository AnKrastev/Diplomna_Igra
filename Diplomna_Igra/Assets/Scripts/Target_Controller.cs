using UnityEngine;
using System.Collections.Generic;

public class Target_Controller : MonoBehaviour
{
    [SerializeField] private float respawn_delay = 3f;

    private Transform[] respawn_points;
    private HashSet<Transform> occupied_points = new HashSet<Transform>();
    private Transform current_point;
    private Transform previous_point;
    private GameManager game_manager;

    public void SetRespawnPoints(Transform[] points, HashSet<Transform> occupied, Transform start_point)
    {
        respawn_points = points;
        occupied_points = occupied;
        current_point = start_point;

        if (current_point != null)
        {
            occupied_points.Add(current_point);
        }
    }

    public void SetGameManager(GameManager manager)
    {
        game_manager = manager;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Enemy_Controller>() == null)
        {
            return;
        }

        ReleasePoint();
        gameObject.SetActive(false);
        game_manager.TargetIncrement();
        Invoke(nameof(Respawn), respawn_delay);
    }

    void ReleasePoint()
    {
        if (current_point != null)
        {
            occupied_points.Remove(current_point);
            previous_point = current_point;
            current_point = null;
        }
    }

    void Respawn()
    {
        if (respawn_points != null && respawn_points.Length > 0)
        {
            List<Transform> free_points = new List<Transform>();

            foreach (Transform point in respawn_points)
            {
                if (!occupied_points.Contains(point) && point != previous_point)
                {
                    free_points.Add(point);
                }
            }

            if (free_points.Count == 0 && previous_point != null && !occupied_points.Contains(previous_point))
            {
                free_points.Add(previous_point);
            }

            if (free_points.Count == 0)
            {
                Invoke(nameof(Respawn), respawn_delay);
                return;
            }

            current_point = free_points[Random.Range(0, free_points.Count)];
            occupied_points.Add(current_point);
            transform.position = current_point.position;
        }

        gameObject.SetActive(true);
    }
}