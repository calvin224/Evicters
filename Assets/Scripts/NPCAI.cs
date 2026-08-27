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

    [Header("Current State")]
    public State currentState = State.Wandering;

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

        if (agent == null)
        {
            Debug.LogError("NPCAI requires a NavMeshAgent on " + gameObject.name);
            enabled = false;
            return;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "NPCAI could not find a GameObject tagged 'Player'. " +
                "Dave will still wander, but cannot detect the player."
            );
        }

        wanderTimer = 1f;
    }

    private void Update()
    {
        float distance = Mathf.Infinity;

        if (player != null)
        {
            distance = Vector3.Distance(
                transform.position,
                player.position
            );
        }

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

        if (wanderTimer <= 0f)
        {
            currentState = State.Wandering;
        }
    }

    private void HandleWandering(float distance)
    {
        if (distance <= detectionRange)
        {
            currentState = State.Suspicious;
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (!agent.hasPath || agent.remainingDistance <= 0.5f)
        {
            Wander();
        }
    }

    private void HandleSuspicious(float distance)
    {
        agent.isStopped = true;

        if (player != null)
        {
            LookAtPlayer();

            if (distance > detectionRange + 2f)
            {
                currentState = State.Wandering;
                wanderTimer = 0f;
            }
            else if (distance <= detectionRange / 2f)
            {
                currentState = State.Investigating;
            }
        }
        else
        {
            currentState = State.Wandering;
            wanderTimer = 0f;
        }
    }

    private void HandleInvestigating(float distance)
    {
        if (player == null)
        {
            currentState = State.Wandering;
            return;
        }

        agent.isStopped = false;

        agent.SetDestination(player.position);

        if (distance > detectionRange)
        {
            currentState = State.Wandering;
            wanderTimer = 0f;
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

            Debug.Log(
                gameObject.name +
                " wandering to " +
                hit.position
            );
        }
        else
        {
            Debug.LogWarning(
                gameObject.name +
                " could not find a position on the NavMesh."
            );
        }

        wanderTimer = wanderInterval;
    }

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

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