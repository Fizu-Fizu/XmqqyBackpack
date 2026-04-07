using UnityEngine;
using UnityEngine.UI;

public class DragClone : MonoBehaviour
{
    public static DragClone Instance { get; private set; }

    [SerializeField] private Image iconImage;
    [SerializeField] private Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);               // 默认隐藏
        transform.position = new Vector3(-1000, -1000, 0); // 扔到屏幕外
    }

    public void Show(Sprite icon, int amount, Vector2 position)
    {
        if (icon == null)
        {
            Debug.LogWarning("拖拽克隆体：图标为 null，不显示");
            return;
        }

        iconImage.sprite = icon;
        amountText.text = amount > 1 ? amount.ToString() : "";
        canvasGroup.alpha = 1f;
        transform.position = position;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        transform.position = new Vector3(-1000, -1000, 0); // 隐藏后归位
    }

    public void UpdatePosition(Vector2 position)
    {
        if (gameObject.activeSelf)
            transform.position = position;
    }
}