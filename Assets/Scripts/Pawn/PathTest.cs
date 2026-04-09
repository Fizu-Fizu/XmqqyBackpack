using UnityEngine;

/// <summary>
/// 测试用脚本：提供按钮调用方法，让 PawnManager 移动到指定坐标。
/// </summary>
public class PathTest : MonoBehaviour
{
    [Header("测试目标坐标")]
    public Vector2 testTarget1 = new Vector2(5, 0);
    public Vector2 testTarget2 = new Vector2(0, 5);

    [Header("PawnManager 引用（不填则自动查找）")]
    public PawnManager pawnManager;

    private void Awake()
    {
        if (pawnManager == null)
            pawnManager = FindObjectOfType<PawnManager>();
    }

    /// <summary>
    /// 测试：移动到坐标1（按钮调用）
    /// </summary>
    [ContextMenu("Move To Target 1")]
    public void MoveToTarget1()
    {
        if (pawnManager == null)
        {
            Debug.LogError("未找到 PawnManager！");
            return;
        }
        pawnManager.SetDestination(testTarget1);
        Debug.Log($"开始寻路至: {testTarget1}");
    }

    /// <summary>
    /// 测试：移动到坐标2（按钮调用）
    /// </summary>
    [ContextMenu("Move To Target 2")]
    public void MoveToTarget2()
    {
        if (pawnManager == null)
        {
            Debug.LogError("未找到 PawnManager！");
            return;
        }
        pawnManager.SetDestination(testTarget2);
        Debug.Log($"开始寻路至: {testTarget2}");
    }

    // 如果你希望提供一个方法供外部按钮（如 UI Button）调用，可以使用以下两个：
    public void MoveToTarget1_UI()
    {
        MoveToTarget1();
    }

    public void MoveToTarget2_UI()
    {
        MoveToTarget2();
    }
}