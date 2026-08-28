using UnityEngine;
using UnityEngine.AI;

public class NPCAI : MonoBehaviour
{
    public enum State { Idle, Wandering, Suspicious, Investigating, Angry, Evicted }

    private GameManager gameManager;

    private bool evictionComplete;

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

    [Header("Physics")]
    public float pushForce = 5f;
    public float physicsRecoveryTime = 0.5f;

    [Header("Anger")]
    public float angerDuration = 3f;

    private float angerTimer;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform player;

    private float wanderTimer;

    private bool angryDestinationSet;

    private float physicsTimer;

    private bool physicsActive;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        gameManager =
    FindFirstObjectByType<GameManager>();

        if (agent == null)
        {
            Debug.LogError(
                "NPCAI requires a NavMeshAgent on " +
                gameObject.name
            );

            enabled = false;
            return;
        }

        if (rb == null)
        {
            Debug.LogError(
                "NPCAI requires a Rigidbody on " +
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

        // NavMeshAgent controls Dave normally.
        rb.isKinematic = true;

        wanderTimer = 1f;
    }

    private void Update()
    {
        // If physics currently controls Dave,
        // wait until the recovery timer expires.
        if (physicsActive)
        {
            physicsTimer -= Time.deltaTime;

            if (physicsTimer <= 0f)
            {
                RestoreNavMeshControl();
            }

            return;
        }

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

            case State.Angry:
                HandleAngry();
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


private void HandleAngry()
    {
        if (player == null)
            return;

        angerTimer -= Time.deltaTime;

        // Look away from the player
        Vector3 directionAway =
            transform.position - player.position;

        directionAway.y = 0f;

        if (directionAway != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(directionAway);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }

        // Move away from the player
        if (agent.enabled)
        {
            agent.isStopped = false;

            Vector3 targetPosition =
                transform.position +
                directionAway.normalized * 3f;

            if (NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit hit,
                3f,
                NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        if (angerTimer <= 0f)
        {
            currentState = State.Wandering;
            wanderTimer = 0f;

            if (agent.enabled)
            {
                agent.isStopped = false;
            }
        }
    }

public void BecomeAngry()
    {
        if (currentState == State.Evicted)
            return;

        currentState = State.Angry;
        angerTimer = angerDuration;

        angryDestinationSet = false;

        if (agent.enabled)
        {
            agent.isStopped = false;
        }

        Debug.Log(
            gameObject.name +
            " is angry!"
        );
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

        agent.SetDestination(
            player.position
        );

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

        if (evictionComplete)
            return;

        agent.isStopped = false;

        if (!agent.hasPath)
        {
            agent.SetDestination(exitPoint.position);
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= exitDistance)
        {
            agent.isStopped = true;

            evictionComplete = true;

            Debug.Log(
                gameObject.name +
                " has reached the exit!"
            );

            if (gameManager != null)
            {
                gameManager.EvictionSuccessful();
            }
        }
    }
    private void Wander()
    {
        agent.isStopped = false;

        Vector3 randomDirection =
            UnityEngine.Random.insideUnitSphere *
            wanderRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(
                hit.position
            );
        }

        wanderTimer = wanderInterval;
    }

    private void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (physicsActive)
            return;

        if (currentState == State.Evicted)
            return;

        Vector3 pushDirection =
            transform.position -
            collision.transform.position;

        pushDirection.y = 0f;

        if (pushDirection.sqrMagnitude < 0.01f)
            return;

        pushDirection.Normalize();

        StartPhysicsMode();

        rb.AddForce(
            pushDirection * pushForce,
            ForceMode.Impulse
        );

        Debug.Log("Dave was pushed!");

        NPC npc = GetComponent<NPC>();

        if (npc != null)
        {
            npc.ReactToPush();
        }
    }

    private void StartPhysicsMode()
    {
        physicsActive = true;
        physicsTimer = physicsRecoveryTime;

        // Stop the NavMeshAgent.
        agent.isStopped = true;

        // Disable the agent while physics controls Dave.
        agent.enabled = false;

        // Allow Rigidbody physics.
        rb.isKinematic = false;
    }

    private void RestoreNavMeshControl()
    {
        physicsActive = false;

        // Find the closest valid point on the NavMesh.
        if (NavMesh.SamplePosition(
            transform.position,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        // Turn the NavMeshAgent back on.
        agent.enabled = true;

        agent.Warp(transform.position);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        agent.isStopped = false;

        Debug.Log("Dave returned to NavMesh control.");
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

        // Make sure the NavMeshAgent is active.
        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        rb.isKinematic = true;

        agent.isStopped = false;

        agent.ResetPath();

        agent.SetDestination(
            exitPoint.position
        );

        Debug.Log(
            "Dave is walking to the exit."
        );
    }
}
