using UnityEngine;
using XmqqyBackpack;

public class DeconstructButton : MonoBehaviour
{
    private Vector3Int gridPos;
    private BuildingData buildingData;

    public void Initialize(Vector3Int pos, BuildingData data)
    {
        gridPos = pos;
        buildingData = data;
        Debug.Log($"[DeconstructButton] 初始化完成，建筑: {data.Label}");
    }

    // UI Button 方式调用
    public void OnButtonClick()
    {
        Debug.Log("[DeconstructButton] OnButtonClick 调用");
        ExecuteDeconstruct();
    }

    // 碰撞体方式调用（鼠标左键点击时自动触发）
    private void OnMouseDown()
    {
        Debug.Log("[DeconstructButton] OnMouseDown 触发");
        ExecuteDeconstruct();
    }

    private void ExecuteDeconstruct()
    {
        PawnManager pawn = FindObjectOfType<PawnManager>();
        ProgressBarSpawner spawner = FindObjectOfType<ProgressBarSpawner>();

        if (pawn == null || spawner == null)
        {
            Debug.LogError("PawnManager 或 ProgressBarSpawner 未找到");
            return;
        }

        var task = new DeconstructTask(gridPos, buildingData, pawn, spawner, (success) =>
        {
            Debug.Log(success ? "拆除完成" : "拆除中断");
        });

        TaskManager.Instance.AddTask(task);
        MouseInputHandler.Instance?.HideButton();
    }
}