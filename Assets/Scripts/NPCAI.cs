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
        Angry,
        WalkingToDoor,
        AtDoor,
        Evicted
    }

    private Transform doorPoint;
    private System.Action doorArrivalAction;

    [Header("Occupant")]
    public OccupantData occupantData;

    [Header("Current State")]
    public State currentState = State.Wandering;

    [Header("Eviction")]
    public Transform exitPoint;
    public float exitDistance = 1f;

    [Header("Physics")]
    public float pushForce = 5f;
    public float physicsRecoveryTime = 0.5f;

    private GameManager gameManager;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform player;

    private float wanderTimer;
    private float angerTimer;
    private float physicsTimer;

    private bool physicsActive;
    private bool evictionComplete;

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

        if (occupantData == null)
        {
            Debug.LogError(
                "No OccupantData assigned to " +
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
                "NPCAI could not find a GameObject " +
                "tagged Player."
            );
        }

        // NavMeshAgent controls the NPC normally.
        rb.isKinematic = true;

        wanderTimer = 1f;
    }

    private void Update()
    {
        // Physics temporarily controls the NPC.
        if (physicsActive)
        {
            physicsTimer -= Time.deltaTime;

            if (physicsTimer <= 0f)
            {
                RestoreNavMeshControl();
            }

            return;
        }

        // Evicted has its own behaviour.
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

            case State.WalkingToDoor:
                HandleWalkingToDoor();
                break;

            case State.AtDoor:
                HandleAtDoor();
                break;
        }
    }

    private void HandleWalkingToDoor()
    {
        if (doorPoint == null)
            return;

        agent.isStopped = false;

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.7f)
        {
            agent.isStopped = true;

            currentState = State.AtDoor;

            Debug.Log(
                gameObject.name +
                " reached the door."
            );

            if (doorArrivalAction != null)
            {
                System.Action action = doorArrivalAction;

                doorArrivalAction = null;

                action.Invoke();
            }
        }
    }

    private void HandleAtDoor()
    {
        agent.isStopped = true;
    }

    private void HandleIdle(float distance)
    {
        if (distance <= occupantData.detectionRange)
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
        if (distance <= occupantData.detectionRange)
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

        if (distance >
            occupantData.detectionRange + 2f)
        {
            currentState = State.Wandering;
            wanderTimer = 0f;
        }
        else if (distance <=
                 occupantData.detectionRange / 2f)
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

        if (distance > occupantData.detectionRange)
        {
            currentState = State.Wandering;
            wanderTimer = 0f;
        }
    }

    private void HandleAngry()
    {
        if (player == null)
            return;

        angerTimer -= Time.deltaTime;

        // Look away from the player.
        Vector3 directionAway =
            transform.position -
            player.position;

        directionAway.y = 0f;

        if (directionAway != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    directionAway
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 8f
                );
        }

        // Move away from the player.
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
                agent.SetDestination(
                    hit.position
                );
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

        angerTimer =
            occupantData.angerDuration;

        if (agent.enabled)
        {
            agent.isStopped = false;
        }

        Debug.Log(
            gameObject.name +
            " is angry!"
        );
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
            agent.SetDestination(
                exitPoint.position
            );
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
            occupantData.wanderRadius;

        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            occupantData.wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(
                hit.position
            );
        }

        wanderTimer =
            occupantData.wanderInterval;
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
                Quaternion.LookRotation(
                    direction
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    rotation,
                    Time.deltaTime * 5f
                );
        }
    }

    private void OnCollisionEnter(
        Collision collision)
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

        Debug.Log(
            gameObject.name +
            " was pushed!"
        );

        NPC npc =
            GetComponent<NPC>();

        if (npc != null)
        {
            npc.ReactToPush();
        }
    }

    private void StartPhysicsMode()
    {
        physicsActive = true;
        physicsTimer = physicsRecoveryTime;

        agent.isStopped = true;
        agent.enabled = false;

        rb.isKinematic = false;
    }

    private void RestoreNavMeshControl()
    {
        physicsActive = false;

        if (NavMesh.SamplePosition(
            transform.position,
            out NavMeshHit hit,
            2f,
            NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        agent.enabled = true;

        agent.Warp(
            transform.position
        );

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        agent.isStopped = false;

        Debug.Log(
            gameObject.name +
            " returned to NavMesh control."
        );
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
                gameObject.name +
                " cannot be evicted because " +
                "Exit Point has not been assigned."
            );

            return;
        }

        currentState = State.Evicted;

        evictionComplete = false;

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
            gameObject.name +
            " is walking to the exit."
        );
    }

    public void GoToDoor(
    Transform target,
    System.Action onArrival)
    {
        if (currentState == State.Evicted)
            return;

        doorPoint = target;
        doorArrivalAction = onArrival;

        currentState = State.WalkingToDoor;

        agent.isStopped = false;
        agent.SetDestination(doorPoint.position);

        Debug.Log(
            gameObject.name +
            " is walking to the door."
        );
    }

    public void LeaveDoor() { if (currentState != State.AtDoor) return; doorPoint = null; doorArrivalAction = null; currentState = State.Wandering; wanderTimer = 0f; agent.isStopped = false; Debug.Log(gameObject.name + " is leaving the door."); }
}
