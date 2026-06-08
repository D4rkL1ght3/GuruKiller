using UnityEngine;

public class TeacherAI : MonoBehaviour
{
    private enum TeacherState
    {
        Patrol,
        Chase,
        Quiz
    }

    [Header("State")]
    [SerializeField] private TeacherState currentState = TeacherState.Patrol;

    [Header("References")]
    public Transform player;
    public PlayerController playerController;
    public TeacherQuizManager quizManager;
    public TeacherRoomController roomController;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1.5f;
    public float patrolPointReachDistance = 0.15f;
    public float patrolWaitTime = 0.75f;

    [Header("Detection")]
    public float detectionRange = 4.5f;
    public bool requireLineOfSight = true;
    public LayerMask sightBlockerLayers;

    [Header("Chase")]
    public float chaseSpeed = 2.8f;
    public float catchDistance = 0.45f;
    public float losePlayerRange = 7f;
    public float escapeGraceTime = 1.5f;

    [Header("Quiz")]
    public int startingQuizDifficulty = 1;
    public int maxQuizDifficulty = 8;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;

    private int currentPatrolIndex;
    private float patrolWaitTimer;
    private float graceTimer;

    private int currentQuizDifficulty;

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        currentQuizDifficulty = startingQuizDifficulty;

        ChangeState(TeacherState.Patrol);
    }

    void Update()
    {
        if (player == null)
            return;

        if (graceTimer > 0f)
        {
            graceTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case TeacherState.Patrol:
                UpdatePatrolState();
                break;

            case TeacherState.Chase:
                UpdateChaseState();
                break;

            case TeacherState.Quiz:
                break;
        }

        HandleAnimation();
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case TeacherState.Patrol:
                MoveAlongPatrol();
                break;

            case TeacherState.Chase:
                MoveTowardPlayer();
                break;

            case TeacherState.Quiz:
                StopMoving();
                break;
        }
    }

    private void UpdatePatrolState()
    {
        if (CanSeePlayer())
        {
            ChangeState(TeacherState.Chase);
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            StopMoving();
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
        }
    }

    private void UpdateChaseState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= catchDistance && graceTimer <= 0f)
        {
            if (roomController != null)
            {
                if (!roomController.IsTeacherInSameRoomAsPlayer())
                {
                    return;
                }
            }

            CatchPlayer();
            return;
        }

        if (distanceToPlayer > losePlayerRange)
        {
            ChangeState(TeacherState.Patrol);
            return;
        }
    }

    private void MoveAlongPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            StopMoving();
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            StopMoving();
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        MoveTowardPosition(targetPoint.position, patrolSpeed);

        float distanceToPatrolPoint =
            Vector2.Distance(transform.position, targetPoint.position);

        if (distanceToPatrolPoint <= patrolPointReachDistance)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }

            patrolWaitTimer = patrolWaitTime;
        }
    }

    private void MoveTowardPlayer()
    {
        MoveTowardPosition(player.position, chaseSpeed);
    }

    private void MoveTowardPosition(Vector3 targetPosition, float speed)
    {
        Vector2 currentPosition = rb.position;
        Vector2 target = targetPosition;

        Vector2 difference = target - currentPosition;

        if (difference.magnitude <= 0.05f)
        {
            StopMoving();
            return;
        }

        moveDirection = difference.normalized;

        if (moveDirection != Vector2.zero)
        {
            lastMoveDirection = moveDirection;
        }

        Vector2 nextPosition =
            currentPosition + moveDirection * speed * Time.fixedDeltaTime;

        rb.MovePosition(nextPosition);
    }

    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        moveDirection = Vector2.zero;
    }

    private bool CanSeePlayer()
    {
        if (graceTimer > 0f)
            return false;

        if (roomController != null)
        {
            if (!roomController.IsTeacherInSameRoomAsPlayer())
            {
                return false;
            }
        }

        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
            return false;

        if (!requireLineOfSight)
            return true;

        RaycastHit2D hit = Physics2D.Linecast(
            transform.position,
            player.position,
            sightBlockerLayers
        );

        return hit.collider == null;
    }

    private void CatchPlayer()
    {
        ChangeState(TeacherState.Quiz);

        StopMoving();

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        int quizDifficultyForThisCatch = currentQuizDifficulty;

        currentQuizDifficulty++;
        currentQuizDifficulty = Mathf.Clamp(
            currentQuizDifficulty,
            startingQuizDifficulty,
            maxQuizDifficulty
        );

        quizManager.StartQuiz(
            quizDifficultyForThisCatch,
            OnQuizPassed,
            OnQuizFailed
        );
    }

    private void OnQuizPassed()
    {
        Debug.Log("Player passed the teacher's quiz!");

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        graceTimer = escapeGraceTime;

        ChangeState(TeacherState.Patrol);
    }

    private void OnQuizFailed()
    {
        Debug.Log("Player failed the teacher's quiz! Game over goes here.");

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        graceTimer = escapeGraceTime;

        // Later, replace this with your actual Game Over system.
        // For now, return to patrol so we can keep testing.
        ChangeState(TeacherState.Patrol);
    }

    private void ChangeState(TeacherState newState)
    {
        currentState = newState;
        StopMoving();

        if (newState == TeacherState.Patrol)
        {
            patrolWaitTimer = 0f;
        }
    }

    private void HandleAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);

        animator.SetFloat("LastMoveX", lastMoveDirection.x);
        animator.SetFloat("LastMoveY", lastMoveDirection.y);

        animator.SetBool("IsMoving", moveDirection != Vector2.zero);
    }

    public void SetPatrolPoints(Transform[] newPatrolPoints)
    {
        patrolPoints = newPatrolPoints;
        currentPatrolIndex = 0;
    }

    public void TeleportTo(Vector3 position)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.linearVelocity = Vector2.zero;
        rb.position = position;
        transform.position = position;
    }

    public void ReturnToPatrol()
    {
        ChangeState(TeacherState.Patrol);
    }

    public void ForceChase()
    {
        ChangeState(TeacherState.Chase);
    }

    public bool IsInQuiz()
    {
        return currentState == TeacherState.Quiz;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);

        if (patrolPoints == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 0.1f);

            int nextIndex = i + 1;

            if (nextIndex >= patrolPoints.Length)
            {
                nextIndex = 0;
            }

            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[nextIndex].position
                );
            }
        }
    }
}