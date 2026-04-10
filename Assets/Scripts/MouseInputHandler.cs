using UnityEngine;
using XmqqyBackpack;

public class MouseInputHandler : MonoBehaviour
{
    public static MouseInputHandler Instance { get; private set; }

    [SerializeField] private GameObject deconstructButtonPrefab;
    private GameObject currentButton;
    private Vector3Int currentBuildingGridPos;
    private BuildingData currentBuildingData;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!IsPointerOverButton())
                HideButton();
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (IsPointerOverUI()) return;
            TryShowDeconstructButton();
        }
    }
    private void TryShowDeconstructButton()
    {
        if (Camera.main == null)
        {
            Debug.LogError("MouseInputHandler: Camera.main 为空，请确保场景中存在 Tag 为 MainCamera 的摄像机");
            return;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 【修正】加 0.5 再向下取整，实现四舍五入效果
        Vector3Int gridPos = new Vector3Int(
            Mathf.FloorToInt(worldPos.x + 0.5f),
            Mathf.FloorToInt(worldPos.y + 0.5f),
            0
        );

        // 后续代码保持不变...
        if (ObjectMapManager.Instance == null)
        {
            Debug.LogError("MouseInputHandler: ObjectMapManager.Instance 为空，请确保场景中存在 ObjectMapManager 脚本");
            return;
        }

        string defName = ObjectMapManager.Instance.GetDefName(gridPos);
        if (string.IsNullOrEmpty(defName))
        {
            Debug.Log($"[MouseInputHandler] 格子 {gridPos} 无建筑数据");
            return;
        }

        BuildingData data = DataManager.GetBuilding(defName);
        if (data == null)
        {
            Debug.LogWarning($"MouseInputHandler: 找不到建筑数据 DefName = {defName}");
            return;
        }

        currentBuildingGridPos = gridPos;
        currentBuildingData = data;

        if (currentButton != null) Destroy(currentButton);
        currentButton = Instantiate(deconstructButtonPrefab, worldPos, Quaternion.identity);
        var btnScript = currentButton.GetComponent<DeconstructButton>();
        if (btnScript == null)
        {
            Debug.LogError("MouseInputHandler: 按钮预制体上缺少 DeconstructButton 组件！");
            Destroy(currentButton);
            currentButton = null;
            return;
        }
        btnScript.Initialize(gridPos, data);
    }
    public void HideButton()
    {
        if (currentButton != null)
        {
            Destroy(currentButton);
            currentButton = null;
        }
    }

    private bool IsPointerOverButton()
    {
        if (currentButton == null) return false;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D col = currentButton.GetComponent<Collider2D>();
        return col != null && col.OverlapPoint(mousePos);
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}