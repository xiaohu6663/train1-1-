using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CanvasController : MonoBehaviour
{
    public int canvasIndex;
    [Header("涂鸦大小参数")]
    [Range(0.1f, 2f)]
    public float graffitiScale = 0.5f;
    [Header("涂鸦移动速度")]
    [Range(10f, 500f)]
    public float graffitiSpeed = 100f;
    public float padding = 50f;

    public float topUnusableArea = 0.15f;
    public float bottomUnusableArea = 0.15f;

    public RectTransform canvasRect;
    public float CanvasWidth => canvasRect.rect.width;
    public float CanvasHeight => canvasRect.rect.height;

    public List<GraffitiDisplay> activeDisplays = new List<GraffitiDisplay>();

    [Header("Y轴分布参数")]
    [Range(-0.5f, 0.5f)]
    public float graffitiYCenter = 0f; // Y轴中心基准（-0.5底部，0中间，0.5顶部）
    [Range(0f, 0.5f)]
    public float graffitiYRange = 0.4f; // Y轴上下可随机范围（0~0.5，越大越满屏）

    private void Awake()
    {
        canvasRect = GetComponent<RectTransform>();
    }

    public void AddGraffitiDisplay(GraffitiDisplay display)
    {
        activeDisplays.Add(display);
        display.transform.SetParent(transform, false); // 保证RectTransform坐标系正确
    }

    public void RemoveGraffitiDisplay(GraffitiDisplay display)
    {
        activeDisplays.Remove(display);
    }

    public bool IsFull()
    {
        // 每个Canvas最多容纳33个涂鸦，确保3个Canvas总共不超过100个
        return activeDisplays.Count >= 33;
    }

    // 更新所有涂鸦的速度
    public void UpdateAllGraffitiSpeed()
    {
        foreach (var display in activeDisplays)
        {
            if (display != null)
            {
                display.UpdateSpeedFromCanvas();
            }
        }
    }

    // 当速度参数在Inspector中改变时调用
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateAllGraffitiSpeed();
        }
    }
}
