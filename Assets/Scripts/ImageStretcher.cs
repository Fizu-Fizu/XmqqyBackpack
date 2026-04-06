using UnityEngine;
using System.IO;

[RequireComponent(typeof(SpriteRenderer))]
public class ImageStretcher : MonoBehaviour
{
    [Tooltip("图片文件绝对路径，例如 C:\\picture.jpg")]
    public string imagePath = @"C:\YourImage.jpg";

    [Tooltip("拉伸后的宽度（世界单位）")]
    public float width = 2f;

    [Tooltip("拉伸后的高度（世界单位）")]
    public float height = 2f;

    void Start()
    {
        LoadAndStretchImage();
    }

    void LoadAndStretchImage()
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError("图片不存在: " + imagePath);
            return;
        }

        // 加载图片字节
        byte[] bytes = File.ReadAllBytes(imagePath);
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogError("加载图片失败，请确认文件是有效的 JPG/PNG");
            return;
        }

        // 将 Texture2D 转换成 Sprite
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        // 赋值给 SpriteRenderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;

        // 关键：设置拉伸尺寸（世界单位）
        sr.drawMode = SpriteDrawMode.Simple;   // 确保为 Simple 模式，才能自由拉伸
        sr.size = new Vector2(width, height);   // 强制拉伸到目标大小
    }
}