using UnityEngine;
using UnityEngine.AI;

public class ParkourRunner : MonoBehaviour
{
    [SerializeField] private GameObject goal;
    [SerializeField] private NavMeshAgent agent;
    private Vector3 goalPos;
    // Start is called before the first frame update
    void Start()
    {
        goalPos = goal.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(goalPos);
    }
}
