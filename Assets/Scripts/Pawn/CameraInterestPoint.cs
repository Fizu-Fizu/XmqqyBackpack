using UnityEngine;
using XmqqyBackpack;

public class CameraInterestPoint : MonoBehaviour
{
    private InfiniteWorld world;
    private string interestKey;

    private void Start()
    {
        world = FindObjectOfType<InfiniteWorld>();
        interestKey = "Camera_" + GetInstanceID();
        // 转换坐标：摄像机(x, y, z) -> 世界需要的(x, z, 0)，其中世界z = 摄像机y
        Vector3 worldPos = new Vector3(transform.position.x, 0, transform.position.y);
        world?.RegisterInterestPoint(interestKey, worldPos);
    }

    private void Update()
    {
        // 每帧更新，同样进行坐标转换
        Vector3 worldPos = new Vector3(transform.position.x, 0, transform.position.y);
        world?.RegisterInterestPoint(interestKey, worldPos);
    }

    private void OnDestroy()
    {
        world?.UnregisterInterestPoint(interestKey);
    }
}