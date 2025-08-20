using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System;

public class GraffitiDisplay : MonoBehaviour
{
    [Header("References")]
    public RawImage graffitiImage; // 改为 public

    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 50f;
    [SerializeField] private float maxSpeed = 200f;
    [SerializeField] private float padding = 50f; // 屏幕边缘内边距

    public string TextureID { get; private set; } // 添加TextureID属性
    public CanvasController CurrentCanvas { get; private set; } // 添加当前画布属性

    private RectTransform rectTransform;
    private float moveSpeed = 100f; // 默认速度
    private int moveDirection = 1; // 1 for right, -1 for left
    private float canvasWidth;
    private float canvasHeight;
    private float imageWidth;

    // 修正：移除重复定义的 startYPosition
    // 原始代码中已有 private float startYPosition; 
    // 现在只需使用原有的变量

    private bool isReturning = false;
    private float initialX;
    private float targetX;

    private GraffitiManager graffitiManager; // 添加对管理器的引用
    private static int ySlotCounter = 0; // 静态计数器用于分布Y轴

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        graffitiManager = GraffitiManager.Instance; // 获取管理器实例

        if (graffitiImage == null)
            graffitiImage = GetComponentInChildren<RawImage>();
    }

    // 修改Initialize方法，添加canvasController参数
    public void Initialize(Texture2D texture, float canvasWidth, float canvasHeight,
                          string textureID, CanvasController canvasController)
    {
        try
        {
            TextureID = textureID; // 设置TextureID
            CurrentCanvas = canvasController; // 设置当前画布

            if (texture == null)
            {
                Debug.LogError("Invalid graffiti texture");
                gameObject.SetActive(false);
                return;
            }

            if (graffitiImage == null)
            {
                Debug.LogError("RawImage reference missing");
                gameObject.SetActive(false);
                return;
            }

            // 应用纹理
            graffitiImage.texture = texture;
            graffitiImage.material = null;
            graffitiImage.color = Color.white;

            // 设置原始尺寸（但不应用，让GraffitiManager控制缩放）
            graffitiImage.SetNativeSize();

            // 缓存画布尺寸
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;

            // 获取图像尺寸
            imageWidth = rectTransform.rect.width;

            // 设置随机起始位置和方向
            SetRandomStartPositionAndDirection();
            
            // 设置初始速度
            UpdateSpeedFromCanvas();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing graffiti display: {e.Message}");
            gameObject.SetActive(false);
        }
    }

    public void SetSpeed(float speed)
    {
        this.moveSpeed = speed;
    }

    // 从CanvasController获取速度
    public void UpdateSpeedFromCanvas()
    {
        if (CurrentCanvas != null)
        {
            this.moveSpeed = CurrentCanvas.graffitiSpeed;
        }
    }

    private void SetRandomStartPositionAndDirection()
    {
        float imageHeight = rectTransform.rect.height;
        float canvasYCenter = CurrentCanvas.graffitiYCenter; // -0.5~0.5
        float canvasYRange = CurrentCanvas.graffitiYRange;   // 0~0.5
        float centerY = canvasYCenter * canvasHeight;
        float rangeY = canvasYRange * canvasHeight;
        float minY = centerY - rangeY;
        float maxY = centerY + rangeY;
        // 保证图片不会超出Canvas
        minY = Mathf.Max(minY, -canvasHeight / 2 + imageHeight / 2 + canvasHeight * CurrentCanvas.bottomUnusableArea);
        maxY = Mathf.Min(maxY, canvasHeight / 2 - imageHeight / 2 - canvasHeight * CurrentCanvas.topUnusableArea);
        float y = UnityEngine.Random.Range(minY, maxY);

        // 随机左右
        if (UnityEngine.Random.value < 0.5f)
        {
            // 从左到右
            initialX = -imageWidth - CurrentCanvas.padding;
            targetX = canvasWidth + imageWidth + CurrentCanvas.padding;
            moveDirection = 1;
        }
        else
        {
            // 从右到左
            initialX = canvasWidth + imageWidth + CurrentCanvas.padding;
            targetX = -imageWidth - CurrentCanvas.padding;
            moveDirection = -1;
        }

        rectTransform.anchoredPosition = new Vector2(initialX, y);
        isReturning = false;
    }

    private void Update()
    {
        if (canvasWidth <= 0) return;

        // 计算当前位置
        float currentX = rectTransform.anchoredPosition.x;
        float currentY = rectTransform.anchoredPosition.y;

        float newX = currentX + (moveSpeed * Time.deltaTime * moveDirection);

        // 检查是否到达目标位置
        if (!isReturning)
        {
            if ((moveDirection > 0 && newX >= targetX) ||
                (moveDirection < 0 && newX <= targetX))
            {
                isReturning = true;
                moveDirection *= -1; // 反转方向
            }
        }
        else
        {
            // 检查是否回到起点
            if ((moveDirection > 0 && newX >= initialX) ||
                (moveDirection < 0 && newX <= initialX))
            {
                // 完成一个Canvas的展示，转移到下一个Canvas
                MoveToNextCanvas();
                return;
            }
        }

        rectTransform.anchoredPosition = new Vector2(newX, currentY);
    }

    private void MoveToNextCanvas()
    {
        // 找到下一个Canvas索引
        int nextCanvasIndex = (System.Array.IndexOf(graffitiManager.canvasControllers, CurrentCanvas) + 1) % 3;
        CanvasController nextCanvas = graffitiManager.canvasControllers[nextCanvasIndex];

        // 如果下一个Canvas已满，直接回收
        if (nextCanvas.IsFull())
        {
            graffitiManager.RecycleGraffiti(this);
            return;
        }

        // 转移到下一个Canvas
        CurrentCanvas.RemoveGraffitiDisplay(this);
        CurrentCanvas = nextCanvas;
        nextCanvas.AddGraffitiDisplay(this);

        // 重置位置和方向
        SetRandomStartPositionAndDirection();
        
        // 更新新画布的速度
        UpdateSpeedFromCanvas();
    }
}