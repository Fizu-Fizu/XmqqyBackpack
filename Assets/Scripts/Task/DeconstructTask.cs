using System.Collections;
using UnityEngine;
using XmqqyBackpack;

/// <summary>
/// 拆除任务：角色移动到建筑附近 → 显示进度条 → 拆除 → 掉落物品 → 删除建筑
/// </summary>
public class DeconstructTask : ITask
{
    private Vector3Int buildingGridPos;
    private BuildingData buildingData;
    private PawnManager pawn;
    private ProgressBarSpawner progressBarSpawner;
    private System.Action<bool> onComplete;

    private bool cancelled;
    private float effectiveWorkSpeed;

    // 接近建筑的判定距离（1.5格内即可开始拆除）
    private const float APPROACH_DISTANCE = 1.5f;

    public DeconstructTask(Vector3Int gridPos, BuildingData data, PawnManager pawn, ProgressBarSpawner progressBarSpawner, System.Action<bool> onComplete = null)
    {
        buildingGridPos = gridPos;
        buildingData = data;
        this.pawn = pawn;
        this.progressBarSpawner = progressBarSpawner;
        this.onComplete = onComplete;
    }

    public IEnumerator Execute()
    {
        if (cancelled)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        // 1. 计算实际工作速度（基于工具加成）
        effectiveWorkSpeed = ToolSpeedCalculator.GetDeconstructWorkSpeed(buildingData);
        Debug.Log($"[DeconstructTask] 有效工作速度: {effectiveWorkSpeed}");

        // 2. 寻找建筑周围最近的可行走格子
        Vector3Int adjacentPos = PathfindingHelper.FindNearestWalkableAdjacent(buildingGridPos);
        if (adjacentPos == new Vector3Int(int.MinValue, int.MinValue, int.MinValue))
        {
            Debug.LogError($"建筑 {buildingData.Label} 周围没有可通行的位置");
            onComplete?.Invoke(false);
            yield break;
        }

        // 3. 设置移动目标并等待接近
        Vector2 targetWorldPos = new Vector2(adjacentPos.x, adjacentPos.y);
        pawn.SetDestination(targetWorldPos);
        yield return PathfindingHelper.WaitUntilReachDestination(pawn, targetWorldPos, APPROACH_DISTANCE);

        if (cancelled) { onComplete?.Invoke(false); yield break; }

        // 4. 在建筑下方生成进度条
        Vector3 progressBarPos = new Vector3(buildingGridPos.x, buildingGridPos.y - 0.4f, 0);
        progressBarSpawner?.SpawnAt(progressBarPos);

        // 5. 拆除工作循环（每帧扣除工作量并更新进度条）
        float totalWork = buildingData.WorkToDeconstruct;
        float remainingWork = totalWork;
        Debug.Log($"[DeconstructTask] 开始拆除 {buildingData.Label}，总工作量: {totalWork}");

        while (remainingWork > 0 && !cancelled)
        {
            float deltaWork = effectiveWorkSpeed * Time.deltaTime;
            remainingWork -= deltaWork;
            float progress = 1f - (remainingWork / totalWork);
            progressBarSpawner?.UpdateProgress(progress);
            yield return null;
        }

        // 6. 清理进度条
        progressBarSpawner?.DestroyProgressBar();

        if (cancelled)
        {
            Debug.Log("[DeconstructTask] 拆除被取消");
            onComplete?.Invoke(false);
            yield break;
        }

        // 7. 掉落物品到背包
        DropItemsToInventory();

        // 8. 从地图中移除建筑
        ObjectMapManager.Instance?.DigObject(buildingGridPos);
        Debug.Log($"[DeconstructTask] 建筑 {buildingData.Label} 已拆除");

        onComplete?.Invoke(true);
    }

    public void Cancel()
    {
        cancelled = true;
        progressBarSpawner?.DestroyProgressBar();
    }

    private void DropItemsToInventory()
    {
        if (buildingData.DeconstructDropList == null || buildingData.DeconstructDropList.Count == 0)
        {
            Debug.Log($"[DeconstructTask] 建筑 {buildingData.Label} 无掉落物");
            return;
        }

        foreach (var drop in buildingData.DeconstructDropList)
        {
            int count = Random.Range(drop.MinAmount, drop.MaxAmount + 1);
            InventoryManager.Instance.AddItem(drop.DefName, count);
            Debug.Log($"[DeconstructTask] 掉落 {drop.DefName} x{count}");
        }
    }
}