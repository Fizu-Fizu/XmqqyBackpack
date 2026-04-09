using System;
using System.Collections.Generic;
using UnityEngine;
using XmqqyBackpack;

public static class Pathfinder
{
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int( 1,  0), new Vector2Int(-1,  0), new Vector2Int( 0,  1), new Vector2Int( 0, -1),
        new Vector2Int( 1,  1), new Vector2Int( 1, -1), new Vector2Int(-1,  1), new Vector2Int(-1, -1)
    };

    private static readonly float[] Distances = new float[]
    {
        1f, 1f, 1f, 1f,
        Mathf.Sqrt(2f), Mathf.Sqrt(2f), Mathf.Sqrt(2f), Mathf.Sqrt(2f)
    };

    /// <summary>
    /// 获取指定格子的综合移动代价（>=0 可通行，<0 不可通行）
    /// </summary>
    public static float GetCombinedCost(Vector3Int gridPos)
    {
        // 地面代价
        string groundDef = GroundMapManager.Instance.GetDefName(gridPos);
        float groundCost = 0f;
        if (!string.IsNullOrEmpty(groundDef))
        {
            GroundData gData = DataManager.GetGround(groundDef);
            if (gData != null) groundCost = gData.PathCost;
        }

        // 建筑代价
        string objDef = ObjectMapManager.Instance.GetDefName(gridPos);
        float buildingCost = 0f;
        if (!string.IsNullOrEmpty(objDef))
        {
            BuildingData bData = DataManager.GetBuilding(objDef);
            if (bData != null) buildingCost = bData.PathCost;
        }

        // 任一为 -1 则不可通行
        if (groundCost < 0 || buildingCost < 0)
            return -1f;

        return groundCost + buildingCost;
    }

    /// <summary>
    /// 检查格子是否可通行
    /// </summary>
    public static bool IsWalkable(Vector3Int gridPos)
    {
        return GetCombinedCost(gridPos) >= 0;
    }

    /// <summary>
    /// A* 寻路（八方向，有权重）
    /// </summary>
    /// <param name="start">起点格子坐标</param>
    /// <param name="goal">终点格子坐标</param>
    /// <returns>路径点列表（包含起点），若找不到路径则返回 null</returns>
    public static List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        // 快速失败：起点或终点不可通行
        if (!IsWalkable(start) || !IsWalkable(goal))
            return null;

        // 已访问集合
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        // 开放列表（优先队列）
        PriorityQueue<Vector3Int> openSet = new PriorityQueue<Vector3Int>();
        openSet.Enqueue(start, 0f);

        // 来源字典
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        // gScore 和 fScore
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float> { [start] = 0f };
        Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            Vector3Int current = openSet.Dequeue();

            if (current == goal)
            {
                // 重建路径
                List<Vector3Int> path = new List<Vector3Int>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            closedSet.Add(current);

            for (int i = 0; i < 8; i++)
            {
                Vector3Int neighbor = current + new Vector3Int(Directions[i].x, Directions[i].y, 0);

                // 通行检查
                if (!IsWalkable(neighbor))
                    continue;

                // 斜向移动时，两侧相邻格必须可通行
                if (i >= 4) // 斜向
                {
                    Vector3Int check1 = current + new Vector3Int(Directions[i].x, 0, 0);
                    Vector3Int check2 = current + new Vector3Int(0, Directions[i].y, 0);
                    if (!IsWalkable(check1) || !IsWalkable(check2))
                        continue;
                }

                if (closedSet.Contains(neighbor))
                    continue;

                float moveCost = Distances[i] * GetCombinedCost(neighbor);
                float tentativeG = gScore[current] + moveCost;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    float h = Heuristic(neighbor, goal);
                    fScore[neighbor] = tentativeG + h;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        // 无路径
        return null;
    }

    private static float Heuristic(Vector3Int a, Vector3Int b)
    {
        // 欧几里得距离（适用于八方向）
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    // 简单优先队列（基于 SortedDictionary 或 List+Sort，这里给出一个最小堆实现）
    private class PriorityQueue<T>
    {
        private List<(T item, float priority)> heap = new List<(T, float)>();
        private Dictionary<T, int> indexMap = new Dictionary<T, int>();

        public int Count => heap.Count;

        public void Enqueue(T item, float priority)
        {
            heap.Add((item, priority));
            int i = heap.Count - 1;
            indexMap[item] = i;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (heap[parent].priority <= heap[i].priority) break;
                Swap(i, parent);
                i = parent;
            }
        }

        public T Dequeue()
        {
            T result = heap[0].item;
            indexMap.Remove(result);
            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);
            if (heap.Count > 0)
            {
                indexMap[heap[0].item] = 0;
                HeapifyDown(0);
            }
            return result;
        }

        public bool Contains(T item) => indexMap.ContainsKey(item);

        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
            indexMap[heap[i].item] = i;
            indexMap[heap[j].item] = j;
        }

        private void HeapifyDown(int i)
        {
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;
                if (left < heap.Count && heap[left].priority < heap[smallest].priority)
                    smallest = left;
                if (right < heap.Count && heap[right].priority < heap[smallest].priority)
                    smallest = right;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
        }
    }
}