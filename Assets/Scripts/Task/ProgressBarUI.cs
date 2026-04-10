using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制 Image 填充的进度条组件（挂载于预制体根节点）
/// </summary>
public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float fillSpeed = 1f; // 平滑插值速度，0表示立即更新

    private float targetProgress;
    private float currentProgress;

    public void SetProgress(float progress01)
    {
        targetProgress = Mathf.Clamp01(progress01);
        if (fillSpeed <= 0)
            fillImage.fillAmount = targetProgress;
    }

    private void Update()
    {
        if (fillSpeed > 0 && Mathf.Abs(currentProgress - targetProgress) > 0.001f)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, fillSpeed * Time.deltaTime);
            fillImage.fillAmount = currentProgress;
        }
    }

    public void SetProgressImmediate(float progress01)
    {
        targetProgress = Mathf.Clamp01(progress01);
        currentProgress = targetProgress;
        fillImage.fillAmount = targetProgress;
    }
}