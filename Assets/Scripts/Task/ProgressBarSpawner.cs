using UnityEngine;

/// <summary>
/// 负责生成和管理进度条实例
/// </summary>
public class ProgressBarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject progressBarPrefab; // 通过 Inspector 拖入

    private GameObject currentInstance;
    private ProgressBarUI progressBarUI;

    /// <summary>
    /// 在指定世界坐标生成进度条
    /// </summary>
    public void SpawnAt(Vector3 worldPosition)
    {
        if (progressBarPrefab == null)
        {
            Debug.LogError("ProgressBarSpawner: 未指定进度条预制体");
            return;
        }

        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = Instantiate(progressBarPrefab, worldPosition, Quaternion.identity);
        progressBarUI = currentInstance.GetComponent<ProgressBarUI>();
    }

    /// <summary>
    /// 更新进度（0~1）
    /// </summary>
    public void UpdateProgress(float progress)
    {
        progressBarUI?.SetProgress(progress);
    }

    /// <summary>
    /// 销毁进度条
    /// </summary>
    public void DestroyProgressBar()
    {
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
            progressBarUI = null;
        }
    }
}