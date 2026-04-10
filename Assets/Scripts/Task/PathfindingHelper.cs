using System.Collections;
using UnityEngine;

public static class PathfindingHelper
{
    /// <summary>
    /// 寻找目标格子周围最近的可行走位置
    /// </summary>
    public static Vector3Int FindNearestWalkableAdjacent(Vector3Int center)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
            new Vector3Int(1,1,0), new Vector3Int(1,-1,0), new Vector3Int(-1,1,0), new Vector3Int(-1,-1,0)
        };
        foreach (var dir in directions)
        {
            Vector3Int check = center + dir;
            if (Pathfinder.IsWalkable(check))
                return check;
        }
        return new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    }

    /// <summary>
    /// 等待角色到达目标点附近（距离 <= threshold）
    /// </summary>
    public static IEnumerator WaitUntilReachDestination(PawnManager pawn, Vector2 targetPos, float threshold = 1.5f)
    {
        if (pawn == null) yield break;

        while (pawn != null && Vector2.Distance(pawn.transform.position, targetPos) > threshold)
        {
            yield return null;
        }

        Debug.Log($"[PathfindingHelper] 已接近目标，距离: {Vector2.Distance(pawn.transform.position, targetPos):F2}");
    }
}