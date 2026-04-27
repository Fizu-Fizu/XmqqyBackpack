using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XmqqyBackpack;

public class InventoryView : MonoBehaviour
{
    public static InventoryView Instance { get; private set; }

    [Header("配置")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    // 常量
    private const float ROW_HEIGHT = 35f;
    private const int RECYCLE_ROWS = 2;
    private const int POOL_SIZE = 70;

    // 对象列表（常驻70个）
    private List<InventorySlotUI> activeSlots = new List<InventorySlotUI>();

    // 数据索引
    private int firstVisibleDataIndex = 0;
    private int columnCount;

    // 防重入
    private bool isRecycling;

    private InventoryModel model;
    private int lastDataCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        model = new InventoryModel();
        model.OnSlotsChanged += HandleSlotsChanged;

        InitializeInventory();
    }

    private void OnEnable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    private void OnDisable()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }

    private void OnDestroy()
    {
        if (model != null)
        {
            model.OnSlotsChanged -= HandleSlotsChanged;
        }

        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }
    }

    // ========== 初始化 ==========
    private void InitializeInventory()
    {
        // 清空原有内容
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        activeSlots.Clear();

        // 获取列数
        columnCount = gridLayoutGroup.constraintCount;
        if (columnCount <= 0) columnCount = 10;

        // 初始化 70 个对象：全部激活，初始 id -1
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotsParent);
            InventorySlotUI slotUI = go.GetComponent<InventorySlotUI>();
            if (slotUI == null)
            {
                Debug.LogError("预制体缺少 InventorySlotUI 组件！");
                Destroy(go);
                continue;
            }
            go.SetActive(true);
            slotUI.Initialize(-1);
            activeSlots.Add(slotUI);
        }

        firstVisibleDataIndex = 0;
        lastDataCount = model.SlotIds.Count;

        // 重置布局
        gridLayoutGroup.padding.top = 20;
        scrollRect.verticalNormalizedPosition = 1f;

        Canvas.ForceUpdateCanvases();

        // 初始绑定数据
        BindActiveSlotsFromFirstVisible();
        UpdateContentHeight();
    }

    // ========== 核心：绑定活跃格子数据 ==========
    private void BindActiveSlotsFromFirstVisible()
    {
        int totalDataCount = model.SlotIds.Count;
        for (int i = 0; i < activeSlots.Count; i++)
        {
            int dataIndex = firstVisibleDataIndex + i;
            InventorySlotUI slotUI = activeSlots[i];

            if (dataIndex < totalDataCount)
            {
                int slotId = model.SlotIds[dataIndex];
                InventorySlot slotData = model.GetSlot(slotId);
                slotUI.Initialize(slotId);
                slotUI.Refresh(slotData);
            }
            else
            {
                slotUI.Initialize(-1);
                slotUI.Refresh(null); // 会调用 SetEmpty
            }
        }
    }

    // ========== 更新 Content 高度 ==========
    private void UpdateContentHeight()
    {
        int totalDataCount = model.SlotIds.Count;
        int totalRows = Mathf.CeilToInt(totalDataCount / (float)columnCount);
        RectTransform contentRect = scrollRect.content;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalRows * ROW_HEIGHT);
    }

    // ========== 公共接口 ==========
    public int AddItem(string def, int amount) => model.AddItem(def, amount);
    public int RemoveItem(string def, int amount) => model.RemoveItem(def, amount);
    public bool RemoveItemAt(int slotId, int amount) => model.RemoveItemAt(slotId, amount);
    public InventorySlot GetSlot(int slotId) => model.GetSlot(slotId);
    public void SwapSlots(int idA, int idB) => model.SwapSlots(idA, idB);
    public int GetTotalAmount(string def) => model.GetTotalAmount(def);
    public bool CraftItem(string resultDef) => model.CraftItem(resultDef);
    public List<SlotSaveData> SaveSlots() => model.SaveSlots();
    public void LoadSlots(List<SlotSaveData> list) => model.LoadSlots(list);
    public IReadOnlyList<int> SlotIds => model.SlotIds;

    // ========== 滚动监听 ==========
    private void OnScrollValueChanged(Vector2 val)
    {
        if (isRecycling) return;
        int totalDataCount = model.SlotIds.Count;
        if (totalDataCount <= POOL_SIZE) return;

        float viewportHeight = scrollRect.viewport.rect.height;
        float contentHeight = scrollRect.content.rect.height;
        if (contentHeight <= 0) return;

        float size = viewportHeight / contentHeight;
        float value = scrollRect.verticalNormalizedPosition;

        if (value <= size * 0.2f && firstVisibleDataIndex + activeSlots.Count < totalDataCount)
        {
            RecycleFromTop(RECYCLE_ROWS);
        }
        else if (value >= size * 0.8f && firstVisibleDataIndex > 0)
        {
            RecycleFromBottom(RECYCLE_ROWS);
        }
    }

    // ========== 循环回收从顶部 ==========
    private void RecycleFromTop(int rows)
    {
        if (isRecycling || rows <= 0) return;
        isRecycling = true;

        try
        {
            int recycleCount = rows * columnCount;
            if (recycleCount > activeSlots.Count) recycleCount = activeSlots.Count;
            if (recycleCount <= 0) return;

            // 取前N个格子移到列表末尾，同时移动 transform 顺序
            List<InventorySlotUI> movedSlots = activeSlots.GetRange(0, recycleCount);
            activeSlots.RemoveRange(0, recycleCount);
            foreach (var slotUI in movedSlots)
            {
                slotUI.transform.SetAsLastSibling();
            }
            activeSlots.AddRange(movedSlots);

            firstVisibleDataIndex += recycleCount;

            // 重新绑定这移动的N个格子的数据
            int totalDataCount = model.SlotIds.Count;
            int bindStartIndex = firstVisibleDataIndex + activeSlots.Count - recycleCount;
            for (int i = 0; i < movedSlots.Count; i++)
            {
                int dataIndex = bindStartIndex + i;
                InventorySlotUI slotUI = movedSlots[i];

                if (dataIndex < totalDataCount)
                {
                    int slotId = model.SlotIds[dataIndex];
                    InventorySlot slotData = model.GetSlot(slotId);
                    slotUI.Initialize(slotId);
                    slotUI.Refresh(slotData);
                }
                else
                {
                    slotUI.Initialize(-1);
                    slotUI.Refresh(null);
                }
            }

            // 更新 Padding 和位置
            gridLayoutGroup.padding.top += rows * (int)ROW_HEIGHT;
            RectTransform contentRect = scrollRect.content;
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contentRect.anchoredPosition.y - rows * ROW_HEIGHT);
            Canvas.ForceUpdateCanvases();
        }
        finally
        {
            isRecycling = false;
        }
    }

    // ========== 循环回收从底部 ==========
    private void RecycleFromBottom(int rows)
    {
        if (isRecycling || rows <= 0) return;
        isRecycling = true;

        try
        {
            int recycleCount = rows * columnCount;
            if (recycleCount > activeSlots.Count) recycleCount = activeSlots.Count;
            if (recycleCount <= 0) return;

            // 取末尾N个格子移到列表开头，同时移动 transform 顺序
            int removeStartIndex = activeSlots.Count - recycleCount;
            List<InventorySlotUI> movedSlots = activeSlots.GetRange(removeStartIndex, recycleCount);
            activeSlots.RemoveRange(removeStartIndex, recycleCount);
            for (int i = movedSlots.Count - 1; i >= 0; i--)
            {
                movedSlots[i].transform.SetAsFirstSibling();
            }
            activeSlots.InsertRange(0, movedSlots);

            firstVisibleDataIndex -= recycleCount;
            if (firstVisibleDataIndex < 0) firstVisibleDataIndex = 0;

            // 重新绑定这移动的N个格子的数据
            for (int i = 0; i < movedSlots.Count; i++)
            {
                int dataIndex = firstVisibleDataIndex + i;
                InventorySlotUI slotUI = movedSlots[i];

                if (dataIndex >= 0 && dataIndex < model.SlotIds.Count)
                {
                    int slotId = model.SlotIds[dataIndex];
                    InventorySlot slotData = model.GetSlot(slotId);
                    slotUI.Initialize(slotId);
                    slotUI.Refresh(slotData);
                }
                else
                {
                    slotUI.Initialize(-1);
                    slotUI.Refresh(null);
                }
            }

            // 更新 Padding 和位置
            int newPaddingTop = gridLayoutGroup.padding.top - rows * (int)ROW_HEIGHT;
            if (newPaddingTop < 0) newPaddingTop = 0;
            gridLayoutGroup.padding.top = newPaddingTop;

            RectTransform contentRect = scrollRect.content;
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, contentRect.anchoredPosition.y + rows * ROW_HEIGHT);
            Canvas.ForceUpdateCanvases();
        }
        finally
        {
            isRecycling = false;
        }
    }

    // ========== 数据变化事件响应 ==========
    private void HandleSlotsChanged(List<int> changedIds)
    {
        // 刷新可见范围内变化的格子
        foreach (var slotUI in activeSlots)
        {
            if (slotUI.slotId != -1 && changedIds.Contains(slotUI.slotId))
            {
                InventorySlot slotData = model.GetSlot(slotUI.slotId);
                slotUI.Refresh(slotData);
            }
        }

        int currentCount = model.SlotIds.Count;
        bool countChanged = currentCount != lastDataCount;
        lastDataCount = currentCount;

        if (countChanged)
        {
            ReEvaluateDisplay();
        }
    }

    // ========== 重新评估显示 ==========
    private void ReEvaluateDisplay()
    {
        if (isRecycling) return;
        isRecycling = true;

        try
        {
            int totalDataCount = model.SlotIds.Count;

            if (totalDataCount == 0)
            {
                // 数据为空：全部格子绑空数据
                foreach (var slotUI in activeSlots)
                {
                    slotUI.Initialize(-1);
                    slotUI.Refresh(null);
                }

                firstVisibleDataIndex = 0;

                gridLayoutGroup.padding.top = 20;
                RectTransform contentRect = scrollRect.content;
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0);
                scrollRect.verticalNormalizedPosition = 1f;
                Canvas.ForceUpdateCanvases();
                return;
            }
            else
            {
                // 检查 visible index 是否超界
                if (firstVisibleDataIndex + activeSlots.Count > totalDataCount)
                {
                    firstVisibleDataIndex = Mathf.Max(0, totalDataCount - activeSlots.Count);
                }

                // 重新绑定所有格子
                BindActiveSlotsFromFirstVisible();
                UpdateContentHeight();
            }
        }
        finally
        {
            isRecycling = false;
        }
    }

    // ========== 全量刷新（保留兼容，仅用于加载存档或完全重建） ==========
    public void RefreshAllUI()
    {
        if (isRecycling) return;
        isRecycling = true;

        try
        {
            // 把所有 activeSlots 对象的位置归位
            foreach (var slotUI in activeSlots)
            {
                slotUI.transform.SetAsLastSibling();
            }

            firstVisibleDataIndex = 0;

            // 重置布局
            gridLayoutGroup.padding.top = 20;
            scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();

            // 重新绑定
            BindActiveSlotsFromFirstVisible();
            UpdateContentHeight();
        }
        finally
        {
            isRecycling = false;
        }
    }
}
