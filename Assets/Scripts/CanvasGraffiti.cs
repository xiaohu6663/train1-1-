using UnityEngine;
using UnityEngine.UI;

public class CanvasGraffiti : MonoBehaviour
{
    [Header("UI组件")]
    public RawImage graffitiImage;
    public RectTransform rectTransform;

    // 设置涂鸦纹理
    public void SetGraffiti(Texture2D texture)
    {
        gameObject.SetActive(true);
        graffitiImage.texture = texture;

        // 根据纹理尺寸调整大小
        Vector2 size = new Vector2(texture.width, texture.height);
        rectTransform.sizeDelta = size;
    }
}