using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KillerTeacherAI : MonoBehaviour
{
    private enum TeacherState
    {
        Patrol,
        Chase,
        Quiz
    }

    [Header("State")]
    [SerializeField] private TeacherState currentState = TeacherState.Patrol;

    [Header("Target")]
    public Transform player;
    public PlayerController playerController;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolMoveSpeed = 1.6f;
    public float patrolPointReachDistance = 0.1f;
    public float patrolWaitTime = 0.75f;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float losePlayerRange = 7f;

    [Header("Chase")]
    public float chaseMoveSpeed = 2.7f;
    public float stoppingDistance = 0.05f;
    public float catchDistance = 0.45f;
    public float repathInterval = 0.25f;
    public float escapeGraceTime = 1.5f;

    [Header("Tilemap Pathfinding")]
    public Tilemap groundTilemap;
    public Tilemap[] obstacleTilemaps;
    public bool requireGroundTile = true;
    public int maxSearchIterations = 5000;

    [Header("Quiz")]
    public TeacherQuizManager quizManager;
    public int startingQuizDifficulty = 1;
    public int maxQuizDifficulty = 8;

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;

    private List<Vector3> currentPath = new List<Vector3>();
    private int pathIndex;

    private int currentPatrolIndex;
    private float patrolWaitTimer;

    private float repathTimer;
    private float graceTimer;

    private int currentQuizDifficulty;

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection = Vector2.down;

    private static readonly Vector3Int[] directions =
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0)
    };

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
                FollowPath(patrolMoveSpeed);
                break;

            case TeacherState.Chase:
                FollowPath(chaseMoveSpeed);
                break;

            case TeacherState.Quiz:
                rb.linearVelocity = Vector2.zero;
                moveDirection = Vector2.zero;
                break;
        }
    }

    private void UpdatePatrolState()
    {
        if (CanDetectPlayer())
        {
            ChangeState(TeacherState.Chase);
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            patrolWaitTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
            return;
        }

        if (currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            FindPathToPosition(patrolPoints[currentPatrolIndex].position);
        }

        float distanceToPatrolPoint =
            Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position);

        if (distanceToPatrolPoint <= patrolPointReachDistance)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }

            currentPath.Clear();
            pathIndex = 0;

            patrolWaitTimer = patrolWaitTime;
        }
    }

    private void UpdateChaseState()
    {
        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > losePlayerRange)
        {
            ChangeState(TeacherState.Patrol);
            return;
        }

        if (distanceToPlayer <= catchDistance && graceTimer <= 0f)
        {
            CatchPlayer();
            return;
        }

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            FindPathToPosition(player.position);
        }
    }

    private bool CanDetectPlayer()
    {
        if (graceTimer > 0f)
            return false;

        float distanceToPlayer =
            Vector2.Distance(transform.position, player.position);

        return distanceToPlayer <= detectionRange;
    }

    private void CatchPlayer()
    {
        ChangeState(TeacherState.Quiz);

        rb.linearVelocity = Vector2.zero;
        moveDirection = Vector2.zero;

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
        Debug.Log("Player survived the teacher's quiz!");

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

        // Later, replace this with Game Over.
        ChangeState(TeacherState.Patrol);
    }

    private void ChangeState(TeacherState newState)
    {
        currentState = newState;

        currentPath.Clear();
        pathIndex = 0;

        rb.linearVelocity = Vector2.zero;
        moveDirection = Vector2.zero;

        if (newState == TeacherState.Chase)
        {
            repathTimer = 0f;
        }

        if (newState == TeacherState.Patrol)
        {
            patrolWaitTimer = 0f;
        }
    }

    private void FindPathToPosition(Vector3 targetPosition)
    {
        if (groundTilemap == null && requireGroundTile)
        {
            Debug.LogWarning("Teacher pathfinding needs a Ground Tilemap.");
            return;
        }

        Vector3Int startCell = WorldToCell(transform.position);
        Vector3Int targetCell = WorldToCell(targetPosition);

        List<Vector3Int> cellPath = FindPath(startCell, targetCell);

        currentPath.Clear();
        pathIndex = 0;

        if (cellPath == null || cellPath.Count == 0)
            return;

        foreach (Vector3Int cell in cellPath)
        {
            currentPath.Add(CellToWorldCenter(cell));
        }
    }

    private void FollowPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
            return;
        }

        if (pathIndex >= currentPath.Count)
        {
            rb.linearVelocity = Vector2.zero;
            moveDirection = Vector2.zero;
            return;
        }

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = currentPath[pathIndex];

        Vector2 difference = targetPosition - currentPosition;

        if (difference.magnitude <= stoppingDistance)
        {
            pathIndex++;
            return;
        }

        moveDirection = difference.normalized;

        if (moveDirection != Vector2.zero)
        {
            lastMoveDirection = moveDirection;
        }

        rb.linearVelocity = moveDirection * speed;
    }

    private List<Vector3Int> FindPath(Vector3Int startCell, Vector3Int targetCell)
    {
        if (!IsWalkable(startCell) || !IsWalkable(targetCell))
            return null;

        List<PathNode> openList = new List<PathNode>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        Dictionary<Vector3Int, PathNode> allNodes =
            new Dictionary<Vector3Int, PathNode>();

        PathNode startNode = new PathNode(startCell);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startCell, targetCell);

        openList.Add(startNode);
        allNodes[startCell] = startNode;

        int iterations = 0;

        while (openList.Count > 0)
        {
            iterations++;

            if (iterations > maxSearchIterations)
            {
                Debug.LogWarning("Teacher pathfinding stopped: too many searched tiles.");
                return null;
            }

            PathNode currentNode = GetLowestCostNode(openList);

            if (currentNode.cell == targetCell)
            {
                return RetracePath(currentNode);
            }

            openList.Remove(currentNode);
            closedSet.Add(currentNode.cell);

            foreach (Vector3Int direction in directions)
            {
                Vector3Int neighborCell = currentNode.cell + direction;

                if (!IsWalkable(neighborCell))
                    continue;

                if (closedSet.Contains(neighborCell))
                    continue;

                int newGCost = currentNode.gCost + 1;

                if (!allNodes.TryGetValue(neighborCell, out PathNode neighborNode))
                {
                    neighborNode = new PathNode(neighborCell);
                    allNodes[neighborCell] = neighborNode;
                }

                if (newGCost < neighborNode.gCost || !openList.Contains(neighborNode))
                {
                    neighborNode.gCost = newGCost;
                    neighborNode.hCost = GetDistance(neighborCell, targetCell);
                    neighborNode.parent = currentNode;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        return null;
    }

    private bool IsWalkable(Vector3Int cell)
    {
        if (groundTilemap != null && requireGroundTile)
        {
            if (!groundTilemap.HasTile(cell))
            {
                return false;
            }
        }

        if (obstacleTilemaps != null)
        {
            foreach (Tilemap obstacleTilemap in obstacleTilemaps)
            {
                if (obstacleTilemap == null)
                    continue;

                if (obstacleTilemap.HasTile(cell))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Vector3Int WorldToCell(Vector3 worldPosition)
    {
        if (groundTilemap != null)
        {
            return groundTilemap.WorldToCell(worldPosition);
        }

        return obstacleTilemaps[0].WorldToCell(worldPosition);
    }

    private Vector3 CellToWorldCenter(Vector3Int cell)
    {
        if (groundTilemap != null)
        {
            return groundTilemap.GetCellCenterWorld(cell);
        }

        return obstacleTilemaps[0].GetCellCenterWorld(cell);
    }

    private PathNode GetLowestCostNode(List<PathNode> nodes)
    {
        PathNode bestNode = nodes[0];

        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < bestNode.fCost)
            {
                bestNode = nodes[i];
            }
            else if (nodes[i].fCost == bestNode.fCost &&
                     nodes[i].hCost < bestNode.hCost)
            {
                bestNode = nodes[i];
            }
        }

        return bestNode;
    }

    private int GetDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector3Int> RetracePath(PathNode endNode)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.cell);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
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

    private class PathNode
    {
        public Vector3Int cell;

        public int gCost = int.MaxValue;
        public int hCost;

        public int fCost => gCost + hCost;

        public PathNode parent;

        public PathNode(Vector3Int cell)
        {
            this.cell = cell;
        }
    }
}