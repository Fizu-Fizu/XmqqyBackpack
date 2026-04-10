using UnityEngine;

/// <summary>
/// 摄像机控制器：WASD 移动 + Ctrl+滚轮（缩放视野 & 调节移动速度）
/// - 上滚：视野拉近（缩小）+ 移动速度倍率减小
/// - 下滚：视野拉远（放大）+ 移动速度倍率增加
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 30f;

    [Header("缩放设置")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minOrthoSize = 3f;
    [SerializeField] private float maxOrthoSize = 15f;

    private Camera cam;
    private Vector3 moveInput;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleZoomInput();
    }

    private void LateUpdate()
    {
        ApplyMovement();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(horizontal, vertical, 0f).normalized;
    }

    private void ApplyMovement()
    {
        if (moveInput == Vector3.zero) return;
        float currentSpeed = moveSpeed * speedMultiplier;
        Vector3 delta = moveInput * currentSpeed * Time.deltaTime;
        transform.position += delta;
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        // 只有按住 Ctrl 时才响应滚轮（同时调节缩放和移动速度）
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // --- 调节视野缩放（上滚减小，下滚增大）---
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minOrthoSize, maxOrthoSize);
            }

            // --- 调节移动速度倍率：上滚减小，下滚增大（与视野变化同向）---
            // 原来 scroll 正时增加倍率，现改为减去 scroll * zoomSpeed 实现反向
            speedMultiplier = Mathf.Clamp(speedMultiplier - scroll * zoomSpeed, 0.2f, 3f);

            Debug.Log($"视野大小: {cam.orthographicSize:F1}, 速度倍率: {speedMultiplier:F1}");
        }
        else
        {
            // 普通滚轮无效果（如需其他功能可在此添加）
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }
}