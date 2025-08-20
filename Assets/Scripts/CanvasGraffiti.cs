using UnityEngine;
using UnityEngine.UI;

public class CanvasGraffiti : MonoBehaviour
{
    [Header("UI���")]
    public RawImage graffitiImage;
    public RectTransform rectTransform;

    // ����Ϳѻ����
    public void SetGraffiti(Texture2D texture)
    {
        gameObject.SetActive(true);
        graffitiImage.texture = texture;

        // ��������ߴ������С
        Vector2 size = new Vector2(texture.width, texture.height);
        rectTransform.sizeDelta = size;
    }
}