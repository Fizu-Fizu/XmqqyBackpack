using UnityEngine;

namespace XmqqyBackpack
{
    /// <summary>
    /// 动态加载 Resources 中的图片，并强制使用 Full Rect 网格类型
    /// 支持九宫格拉伸（如果原 Sprite 配置了 border）
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TextureImage : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Sprite currentCreatedSprite; // 用于后续销毁，避免内存泄漏

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            // 释放动态创建的 Sprite，防止内存泄漏
            if (currentCreatedSprite != null)
            {
                Destroy(currentCreatedSprite);
                currentCreatedSprite = null;
            }
        }

        /// <summary>
        /// 从 Resources 文件夹加载图片并显示
        /// </summary>
        /// <param name="resourcePath">Resources 下的路径，不含扩展名</param>
        public void SetImageFromResources(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogError("TextureImage: resourcePath 为空");
                return;
            }

            // 加载原始 Texture2D
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                Debug.LogError($"TextureImage: 未找到 Resources 中的纹理 -> {resourcePath}");
                return;
            }

            // 尝试加载同名的 Sprite，以获取其九宫格 border 和 pixelsPerUnit 配置
            Sprite originalSprite = Resources.Load<Sprite>(resourcePath);
            Vector4 border = Vector4.zero;
            float pixelsPerUnit = 100f;

            if (originalSprite != null)
            {
                if (originalSprite.border != Vector4.zero)
                    border = originalSprite.border;
                pixelsPerUnit = originalSprite.pixelsPerUnit;
            }

            // 动态创建 Full Rect 的 Sprite
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            Sprite newSprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect, border);

            // 替换旧的 Sprite
            if (currentCreatedSprite != null && currentCreatedSprite != spriteRenderer.sprite)
            {
                Destroy(currentCreatedSprite);
            }

            spriteRenderer.sprite = newSprite;
            currentCreatedSprite = newSprite;

            // 5. 设置为九宫格拉伸模式，并设置初始大小
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = Vector2.one;
        }

        /// <summary>
        /// 手动清除当前图片并释放资源
        /// </summary>
        public void ClearImage()
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = null;
            if (currentCreatedSprite != null)
            {
                Destroy(currentCreatedSprite);
                currentCreatedSprite = null;
            }
        }
    }
}