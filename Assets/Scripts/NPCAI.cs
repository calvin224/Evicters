using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Wandering,
        Suspicious,
        Investigating,
        Evicted
    }

    [Header("Current State")]
    public State currentState = State.Wandering;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Wandering")]
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;

    [Header("Eviction")]
    public Transform exitPoint;
    public float exitDistance = 1f;

    private NavMeshAgent agent;
    private Transform player;
    private float wanderTimer;

    public event Action OnEvicted;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                "NPCAI requires a NavMeshAgent on " +
                gameObject.name
            );

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
                "NPCAI could not find a GameObject tagged Player."
            );
        }

        wanderTimer = 1f;
    }

    private void Update()
    {
        // Evicted is handled separately.
        if (currentState == State.Evicted)
        {
            HandleEvicted();
            return;
        }

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

        if (!agent.hasPath ||
            agent.remainingDistance <= 0.5f)
        {
            Wander();
        }
    }

    private void HandleSuspicious(float distance)
    {
        agent.isStopped = true;

        if (player == null)
        {
            currentState = State.Wandering;
            wanderTimer = 0f;
            return;
        }

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

    private void HandleEvicted()
    {
        if (exitPoint == null)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        // Make sure Dave is actually walking toward the exit.
        if (!agent.hasPath ||
            agent.destination != exitPoint.position)
        {
            agent.SetDestination(exitPoint.position);
        }

        // Check if Dave has reached the exit.
        if (!agent.pathPending &&
            agent.remainingDistance <= exitDistance)
        {
            agent.isStopped = true;

            Debug.Log(
                gameObject.name +
                " has reached the exit!"
            );

            OnEvicted?.Invoke();
        }
    }

    private void Wander()
    {
        agent.isStopped = false;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;

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
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion rotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotation,
                    Time.deltaTime * 5f
                );
        }
    }

    public void OnKnockedOn()
    {
        if (currentState == State.Evicted)
            return;

        currentState = State.Suspicious;

        agent.isStopped = true;

        Debug.Log(
            gameObject.name +
            " heard the knock."
        );
    }

    public void OnRefusedEviction()
    {
        if (currentState == State.Evicted)
            return;

        currentState = State.Investigating;
    }

    public void Evict()
    {
        if (exitPoint == null)
        {
            Debug.LogError(
                "Dave cannot be evicted because " +
                "Exit Point has not been assigned."
            );

            return;
        }

        currentState = State.Evicted;

        agent.isStopped = false;
        agent.ResetPath();

        bool destinationSet =
            agent.SetDestination(exitPoint.position);

        Debug.Log(
            "EVict called. Destination set: " +
            destinationSet
        );

        Debug.Log(
            "Dave is walking to: " +
            exitPoint.position
        );
    }
}
