using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XmqqyBackpack;

public class PawnManager : MonoBehaviour
{
    [Header("目标坐标（世界坐标）")]
    public Vector2 targetPosition = new Vector2(5, 0);

    [Header("移动速度")]
    public float speed = 3f;

    [Header("方向模型")]
    public GameObject frontObj;
    public GameObject backObj;
    public GameObject leftObj;
    public GameObject rightObj;

    [Header("世界引用")]
    public InfiniteWorld infiniteWorld;

    [Header("路径预制体")]
    public GameObject pathLinePrefab;

    [Header("路径父对象")]
    public Transform pathLineParent;

    // 当前路径节点列表（格子坐标）
    private List<Vector3Int> currentPath;
    private int currentPathIndex;

    // 当前显示的方向模型
    private GameObject currentActiveObj;
    private Vector2 lastMoveDir;

    // 移动状态
    private bool isMoving;
    private Vector2 currentDestination;

    // 可视化对象
    private LineRenderer pathLineRenderer;
    private GameObject pathLineInstance;

    // 路径重算协程
    private Coroutine repathCoroutine;

    private void Awake()
    {
        if (infiniteWorld == null)
            infiniteWorld = FindObjectOfType<InfiniteWorld>();

        SetActiveDirection(frontObj);
        lastMoveDir = Vector2.zero;
    }

    private void Start()
    {
        // 初始化路径可视化
        if (pathLinePrefab != null)
        {
            // 使用指定父对象，若未指定则放在场景根节点
            Transform parent = pathLineParent != null ? pathLineParent : null;
            pathLineInstance = Instantiate(pathLinePrefab, parent);
            pathLineRenderer = pathLineInstance.GetComponent<LineRenderer>();
            if (pathLineRenderer == null)
                pathLineRenderer = pathLineInstance.AddComponent<LineRenderer>();
            pathLineRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// 设置目标并开始移动（会停止之前的重算协程并开启新的）
    /// </summary>
    public void SetDestination(Vector2 worldTarget)
    {
        targetPosition = worldTarget;
        currentDestination = worldTarget;

        // 停止旧的协程
        if (repathCoroutine != null)
            StopCoroutine(repathCoroutine);

        // 立即计算一次路径
        RecalculatePath();

        // 启动每秒重算的协程
        repathCoroutine = StartCoroutine(RepathRoutine());
    }

    private void RecalculatePath()
    {
        Vector3Int startGrid = WorldToGrid(transform.position);
        Vector3Int goalGrid = WorldToGrid(currentDestination);

        // 计算新路径
        List<Vector3Int> newPath = Pathfinder.FindPath(startGrid, goalGrid);

        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            currentPathIndex = 0; // 跳过起点（当前位置）
            isMoving = true;
            UpdatePathVisualization();
        }
        else
        {
            Debug.LogWarning($"重新计算路径失败：{startGrid} -> {goalGrid}");
            // 不改变 currentPath，等待下一次重算
        }
    }

    private IEnumerator RepathRoutine()
    {
        while (isMoving && currentPath != null && currentPathIndex < currentPath.Count)
        {
            yield return new WaitForSeconds(1f);
            RecalculatePath();
        }
        repathCoroutine = null;
    }

    private void Update()
    {
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector3 targetWorldPos = GridToWorld(currentPath[currentPathIndex]);
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetWorldPos, speed * Time.deltaTime);
            transform.position = newPos;

            // 方向显示
            Vector2 moveDelta = newPos - currentPos;
            if (moveDelta.magnitude > 0.01f)
                UpdateDirection(moveDelta.normalized);

            // 更新世界管理器
            if (infiniteWorld != null)
                infiniteWorld.SetPlayerPosition(new Vector3(transform.position.x, 0, transform.position.y));

            // 到达当前节点
            if (Vector2.Distance(newPos, targetWorldPos) < 0.01f)
            {
                currentPathIndex++;
                UpdatePathVisualization(); // 更新线条显示剩余路径

                if (currentPathIndex >= currentPath.Count)
                {
                    OnMoveEnd();
                }
            }
        }
    }

    private void OnMoveEnd()
    {
        isMoving = false;
        SetActiveDirection(frontObj);
        currentPath = null;
        if (repathCoroutine != null)
        {
            StopCoroutine(repathCoroutine);
            repathCoroutine = null;
        }
        ClearPathVisualization();
        Debug.Log("到达目的地");
    }

    private void UpdateDirection(Vector2 moveDir)
    {
        if (moveDir == Vector2.zero) return;

        if (Mathf.Abs(moveDir.x) > 0.01f)
        {
            if (moveDir.x > 0) SetActiveDirection(rightObj);
            else SetActiveDirection(leftObj);
        }
        else if (Mathf.Abs(moveDir.y) > 0.01f)
        {
            if (moveDir.y > 0) SetActiveDirection(backObj);
            else SetActiveDirection(frontObj);
        }
        lastMoveDir = moveDir;
    }

    private void SetActiveDirection(GameObject activeObj)
    {
        if (activeObj == null) return;
        if (currentActiveObj == activeObj) return;

        frontObj?.SetActive(activeObj == frontObj);
        backObj?.SetActive(activeObj == backObj);
        leftObj?.SetActive(activeObj == leftObj);
        rightObj?.SetActive(activeObj == rightObj);

        currentActiveObj = activeObj;
    }

    // ---------- 路径可视化 ----------
    private void UpdatePathVisualization()
    {
        if (pathLineRenderer == null || currentPath == null) return;

        int remainingCount = currentPath.Count - currentPathIndex;
        if (remainingCount <= 0)
        {
            pathLineRenderer.positionCount = 0;
            return;
        }

        Vector3[] positions = new Vector3[remainingCount];
        for (int i = 0; i < remainingCount; i++)
        {
            Vector3Int gridPos = currentPath[currentPathIndex + i];
            positions[i] = GridToWorld(gridPos);
        }
        pathLineRenderer.positionCount = remainingCount;
        pathLineRenderer.SetPositions(positions);
    }

    private void ClearPathVisualization()
    {
        if (pathLineRenderer != null)
            pathLineRenderer.positionCount = 0;
    }

    // ---------- 坐标转换 ----------
    private Vector3 GridToWorld(Vector3Int gridPos)
    {
        return new Vector3(gridPos.x, gridPos.y, 0);
    }

    private Vector3Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector3Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
    }
}