using UnityEngine;
using UnityEngine.AI;

public class NPCAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Wandering,
        Suspicious,
        Investigating
    }

    public State currentState = State.Idle;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Wandering")]
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;

    private NavMeshAgent agent;
    private Transform player;

    private float wanderTimer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        wanderTimer = 1f;
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(distance);
                break;

            case State.Wandering:
                HandleWandering(distance);
                break;

            case State.Suspicious:
                HandleSuspicious(distance);
                break;

            case State.Investigating:
                HandleInvestigating(distance);
                break;
        }
    }

    private void HandleIdle(float distance)
    {
        if (distance <= detectionRange)
        {
            currentState = State.Suspicious;
            return;
        }

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0)
        {
            currentState = State.Wandering;
        }
    }

    private void HandleWandering(float distance)
    {
        if (distance <= detectionRange)
        {
            currentState = State.Suspicious;
            return;
        }

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Wander();
        }
    }

    private void HandleSuspicious(float distance)
    {
        agent.isStopped = true;

        LookAtPlayer();

        if (distance > detectionRange + 2f)
        {
            currentState = State.Wandering;
        }
        else if (distance <= detectionRange / 2f)
        {
            currentState = State.Investigating;
        }
    }

    private void HandleInvestigating(float distance)
    {
        agent.isStopped = false;

        agent.SetDestination(player.position);

        if (distance > detectionRange)
        {
            currentState = State.Wandering;
        }
    }

    private void Wander()
    {
        agent.isStopped = false;

        Vector3 randomDirection =
            Random.insideUnitSphere * wanderRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        wanderTimer = wanderInterval;
    }

    private void LookAtPlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion rotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                Time.deltaTime * 5f
            );
        }
    }
}