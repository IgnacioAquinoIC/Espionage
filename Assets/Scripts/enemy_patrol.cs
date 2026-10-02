using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Detecting,
        Alerted,
        Pursuing,
        Returning
    }

    public EnemyState currentState = EnemyState.Patrol;
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private int currentPoint = 0;

    [Header("Detection")]
    public Transform player;
    public float detectionRange = 15f;

    [Header("Pursuit")]
    public float pursuitSpeed = 4f;
    public float alertDistance = 5f;

    private float alertTimer = 0f;
    private EnemyState previousState;
    private Vector3 lastSeenPosition;
    public bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;

        if (directionToPlayer.magnitude > detectionRange)
            return false;

        float angle = Vector3.Angle(
            transform.forward,
            directionToPlayer
        );

        if (angle > FOV / 2f)
            return false;

        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            directionToPlayer.normalized,
            out hit,
            detectionRange))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }
    public new Rigidbody rigidbody;
    public float FOV = 90f;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;

        previousState = currentState;
    }

    void Update()
    {
        Debug.Log("Current state: " + currentState);
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                CheckForPlayer();
                break;

            case EnemyState.Detecting:
                Detecting();
                break;

            case EnemyState.Alerted:
                Alerted();
                break;

            case EnemyState.Pursuing:
                Pursuing();
                break;
            case EnemyState.Returning:
                Returning();
                break;
        }

        if (currentState != previousState)
        {
            Debug.Log("Enemy state changed: " + previousState + " → " + currentState);
            previousState = currentState;
        }
    }

    // =========================
    // PATROL
    // =========================

    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPoint];

        MoveTowards(targetPoint.position, patrolSpeed);

        Vector3 direction = targetPoint.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            currentPoint++;

            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }
        }
    }

    // =========================
    // DETECTION
    // =========================

    void CheckForPlayer()
    {
        if (CanSeePlayer())
        {
            lastSeenPosition = player.position;
            currentState = EnemyState.Detecting;
        }
    }

    // =========================
    // DETECTING
    // =========================

    void Detecting()
    {
        // Move toward where the player was last seen.
        MoveTowards(lastSeenPosition, patrolSpeed);

        // Look toward the last known position.
        Vector3 direction = lastSeenPosition - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Check whether the player is currently close enough to trigger an alert.
        if (Vector3.Distance(transform.position, player.position) <= alertDistance)
        {
            currentState = EnemyState.Alerted;
            alertTimer = 1f;
            return;
        }

        // Reached the location where the player was last seen.
        if (Vector3.Distance(transform.position, lastSeenPosition) < 0.05f)
        {
            currentState = EnemyState.Returning;
        }
    }

    // =========================
    // ALERTED
    // =========================

    void Alerted()
    {
        LookAtPlayer();

        alertTimer -= Time.deltaTime;

        if (alertTimer <= 0f)
        {
            currentState = EnemyState.Pursuing;
        }
    }

    // =========================
    // PURSUING
    // =========================

    void Pursuing()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        RaycastHit hit;
        bool canSeePlayer = CanSeePlayer();

        if (canSeePlayer)
        {
            // We can still see the player,
            // so keep updating their last known position.
            lastSeenPosition = player.position;

            MoveTowards(player.position, pursuitSpeed);

            Vector3 direction = player.position - transform.position;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else
        {
            // Player escaped line of sight.
            // Move toward where we last saw them.
            MoveTowards(lastSeenPosition, pursuitSpeed);

            Vector3 direction = lastSeenPosition - transform.position;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Once we reach the last known position,
            // return to patrol.
            if (Vector3.Distance(transform.position, lastSeenPosition) < 0.05f)
            {
                currentState = EnemyState.Patrol;
            }
        }
    }
    void Returning()
    {
        Transform targetPoint = patrolPoints[currentPoint];

        MoveTowards(targetPoint.position, patrolSpeed);

        Vector3 direction = targetPoint.position - transform.position;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
        {
            currentState = EnemyState.Patrol;
        }
    }

    // =========================
    // MOVEMENT
    // =========================

    void MoveTowards(Vector3 target, float movementSpeed)
    {
        Vector3 newPosition = Vector3.MoveTowards(
            rigidbody.position,
            target,
            movementSpeed * Time.deltaTime
        );

        rigidbody.MovePosition(newPosition);
    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    public float GetDetectionLevel()
    {
        if (currentState == EnemyState.Alerted ||
            currentState == EnemyState.Pursuing)
        {
            return 1f;
        }

        if (currentState != EnemyState.Detecting)
        {
            return 0f;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        return Mathf.InverseLerp(
            detectionRange,
            alertDistance,
            distance
        );
    }
    public void CameraDetectedPlayer(Vector3 playerPosition)
    {
        lastSeenPosition = playerPosition;
        currentState = EnemyState.Pursuing;
    }
    public void ResetToPatrol()
    {
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        rigidbody.position = startingPosition;
        rigidbody.rotation = startingRotation;

        transform.position = startingPosition;
        transform.rotation = startingRotation;

        currentPoint = 0;
        currentState = EnemyState.Patrol;
        alertTimer = 0f;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Lose.Instance.GameOver();
        }
    }
}
