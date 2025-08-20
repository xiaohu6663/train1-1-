using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.IO;

public class GraffitiManager : MonoBehaviour
{
    public static GraffitiManager Instance;
    public CanvasController[] canvasControllers; // 三个Canvas的控制器
    private int currentCanvasIndex = 0; // 当前添加涂鸦的Canvas索引
    [Header("Ϳѻ��ʾ����")]
    public Transform graffitiParent;
    public GameObject graffitiPrefab;
    public int maxGraffitiCount = 100;

    [Header("Ϳѻ��������")]
    public Vector2 spawnAreaSize = new Vector2(10f, 5f);
    [Tooltip("涂鸦最小缩放比例")]
    [Range(0.1f, 1.0f)]
    public float minScale = 0.3f;
    
    [Tooltip("涂鸦最大缩放比例")]
    [Range(0.5f, 3.0f)]
    public float maxScale = 1.0f;
    public float minRotation = -30f;
    public float maxRotation = 30f;
    public float moveSpeed = 0.5f;
   
    [Header("��Ļ��ȫ��������")]
    [Tooltip("��Ļ��������������ĸ߶ȱ��� (0-1)")]
    [Range(0f, 0.5f)]
    public float topUnusableArea = 0.15f; // ��Ļ����10%������

    [Tooltip("��Ļ�ײ�����������ĸ߶ȱ��� (0-1)")]
    [Range(0f, 0.5f)]
    public float bottomUnusableArea = 0.15f; // ��Ļ�ײ�10%������
    private Queue<GraffitiData> graffitiQueue = new Queue<GraffitiData>();
    private Dictionary<string, GraffitiData> graffitiDictionary = new Dictionary<string, GraffitiData>();
    private List<GraffitiDisplay> activeDisplays = new List<GraffitiDisplay>();
    private Stack<GameObject> displayPool = new Stack<GameObject>();
    
    // 添加待显示队列，确保按时间顺序显示
    private Queue<GraffitiData> pendingDisplayQueue = new Queue<GraffitiData>();
    private bool isProcessingDisplay = false;

    private Canvas canvas;
    private RectTransform canvasRect;
    private float canvasWidth;
    private float canvasHeight;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
            GetCanvasSize();
            ValidateCanvasControllers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 验证Canvas控制器设置
    /// </summary>
    private void ValidateCanvasControllers()
    {
        if (canvasControllers == null || canvasControllers.Length == 0)
        {
            Debug.LogError("[GraffitiManager] canvasControllers未设置！");
            return;
        }
        
        if (canvasControllers.Length < 3)
        {
            Debug.LogWarning($"[GraffitiManager] 建议设置3个Canvas控制器，当前只有{canvasControllers.Length}个");
        }
        
        for (int i = 0; i < canvasControllers.Length; i++)
        {
            if (canvasControllers[i] == null)
            {
                Debug.LogError($"[GraffitiManager] Canvas控制器[{i}]为null！");
            }
            else
            {
                Debug.Log($"[GraffitiManager] Canvas控制器[{i}]: {canvasControllers[i].name}");
            }
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < maxGraffitiCount; i++)
        {
            GameObject displayObj = Instantiate(graffitiPrefab, graffitiParent);
            displayObj.SetActive(false);
            displayPool.Push(displayObj);
        }
    }

    private void GetCanvasSize()
    {
        canvas = graffitiParent.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;
        }
        else
        {
            Debug.LogError("Canvas not found in parent hierarchy");
        }
    }

    public void AddGraffiti(Texture2D texture)
    {
        try
        {
            string uniqueID = GenerateUniqueID(texture);

            if (graffitiDictionary.ContainsKey(uniqueID))
            {
                Debug.Log($"涂鸦已存在，跳过添加: {uniqueID}");
                return;
            }

            GraffitiData graffiti = new GraffitiData
            {
                id = uniqueID,
                texture = texture,
                createTime = DateTime.Now
            };

            AddGraffitiToQueue(graffiti);
        }
        catch (Exception e)
        {
            Debug.LogError($"添加涂鸦失败: {e.Message}");
        }
    }

    public void AddGraffitiFromUDP(string data)
    {
        try
        {
            string[] parts = data.Split('|');
            if (parts.Length < 2) return;

            string imageID = parts[0];
            string base64Data = parts[1];

            if (!Regex.IsMatch(base64Data, @"^[a-zA-Z0-9\+/]+={0,2}$"))
                return;

            string uniqueID = GenerateUniqueID(Encoding.UTF8.GetBytes(base64Data));

            if (graffitiDictionary.ContainsKey(uniqueID))
            {
                Debug.Log($"UDP涂鸦已存在，跳过添加: {uniqueID}");
                return;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            if (!texture.LoadImage(imageBytes))
                return;

            texture.name = uniqueID;

            GraffitiData graffiti = new GraffitiData
            {
                id = uniqueID,
                texture = texture,
                createTime = DateTime.Now
            };

            AddGraffitiToQueue(graffiti);
        }
        catch (Exception e)
        {
            Debug.LogError($"处理UDP涂鸦数据失败: {e.Message}");
        }
    }

    public void AddGraffitiFromFile(string fileName)
    {
        try
        {
            Debug.Log($"[GraffitiManager] 开始处理文件: {fileName}");
            
            // 清理文件名，移除路径分隔符但保留扩展名
            string cleanFileName = Path.GetFileName(fileName.Trim());
            if (string.IsNullOrEmpty(cleanFileName))
            {
                Debug.LogError("文件名无效: " + fileName);
                return;
            }
            
            Debug.Log($"[GraffitiManager] 清理后的文件名: {cleanFileName}");
            
            // 直接尝试完整文件名
            string filePath = Path.Combine("F:/Graffiti", cleanFileName);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"图片文件不存在: {filePath}");
                return;
            }

            Debug.Log($"[GraffitiManager] 找到图片文件: {filePath}");
            
            byte[] imageBytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(imageBytes))
            {
                Debug.LogError("图片加载失败: " + filePath);
                return;
            }
            
            // 使用完整文件名作为纹理名称
            texture.name = cleanFileName;
            Debug.Log($"[GraffitiManager] 成功加载图片: {cleanFileName} ({texture.width}x{texture.height})");

            AddGraffiti(texture);
            
            // 监控涂鸦数量
            LogGraffitiStatus();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AddGraffitiFromFile异常: {e.Message}\n堆栈: {e.StackTrace}");
        }
    }

    private string GenerateUniqueID(Texture2D texture)
    {
        byte[] textureBytes = texture.EncodeToPNG();
        return GenerateUniqueID(textureBytes);
    }

    private string GenerateUniqueID(byte[] data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(data);
            return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
        }
    }

    private void AddGraffitiToQueue(GraffitiData graffiti)
    {
        lock (graffitiQueue)
        {
            // 如果已经达到最大数量，先移除最旧的
            if (graffitiQueue.Count >= maxGraffitiCount)
            {
                GraffitiData oldest = graffitiQueue.Dequeue();
                graffitiDictionary.Remove(oldest.id);
                DestroyOldestGraffiti(oldest.id);
                Debug.Log($"[GraffitiManager] 队列已满，移除最旧涂鸦: {oldest.id}");
            }
            
            // 添加新的涂鸦
            graffitiDictionary.Add(graffiti.id, graffiti);
            graffitiQueue.Enqueue(graffiti);
            Debug.Log($"[GraffitiManager] 添加新涂鸦到队列: {graffiti.id}, 当前队列数量: {graffitiQueue.Count}");
        }
        
        // 添加到待显示队列，确保按时间顺序显示
        lock (pendingDisplayQueue)
        {
            pendingDisplayQueue.Enqueue(graffiti);
        }
        
        // 开始处理显示队列
        ProcessDisplayQueue();
    }

    private void DestroyOldestGraffiti(string id)
    {
        for (int i = activeDisplays.Count - 1; i >= 0; i--)
        {
            GraffitiDisplay display = activeDisplays[i];
            if (display != null && display.TextureID == id)
            {
                // ��������
                if (display.graffitiImage != null && display.graffitiImage.texture != null)
                {
                    Destroy(display.graffitiImage.texture);
                    display.graffitiImage.texture = null;
                }

                display.gameObject.SetActive(false);
                activeDisplays.RemoveAt(i);
                displayPool.Push(display.gameObject);
                break;
            }
        }
    }

    private void ProcessDisplayQueue()
    {
        if (isProcessingDisplay) return;
        
        isProcessingDisplay = true;
        
        // 使用协程来处理显示队列，确保按顺序显示
        StartCoroutine(ProcessDisplayQueueCoroutine());
    }
    
    private System.Collections.IEnumerator ProcessDisplayQueueCoroutine()
    {
        while (true)
        {
            GraffitiData graffitiToDisplay = null;
            
            lock (pendingDisplayQueue)
            {
                if (pendingDisplayQueue.Count > 0)
                {
                    graffitiToDisplay = pendingDisplayQueue.Dequeue();
                }
            }
            
            if (graffitiToDisplay != null)
            {
                DisplayNewGraffiti(graffitiToDisplay);
                // 添加小延迟，确保显示顺序
                yield return new WaitForEndOfFrame();
            }
            else
            {
                break;
            }
        }
        
        isProcessingDisplay = false;
    }

    // 修改DisplayNewGraffiti方法 - 实现Canvas循环逻辑
    private void DisplayNewGraffiti(GraffitiData graffiti)
    {
        if (displayPool.Count == 0) return;

        // 寻找可用的Canvas，按顺序：Canvas1 -> Canvas2 -> Canvas3 -> 回到Canvas1
        CanvasController targetCanvas = FindAvailableCanvas();
        if (targetCanvas == null)
        {
            // 所有Canvas都满了，等待下次处理
            lock (pendingDisplayQueue)
            {
                pendingDisplayQueue.Enqueue(graffiti);
            }
            return;
        }

        GameObject displayObj = displayPool.Pop();
        displayObj.SetActive(true);

        GraffitiDisplay display = displayObj.GetComponent<GraffitiDisplay>();
        if (display != null)
        {
            // 统一大小
            display.transform.localScale = Vector3.one * targetCanvas.graffitiScale;
            // 统一速度
            display.SetSpeed(targetCanvas.graffitiSpeed);

            display.Initialize(
                graffiti.texture,
                targetCanvas.CanvasWidth,
                targetCanvas.CanvasHeight,
                graffiti.id,
                targetCanvas
            );

            activeDisplays.Add(display);
            targetCanvas.AddGraffitiDisplay(display);
            
            Debug.Log($"[GraffitiManager] 涂鸦 {graffiti.id} 添加到 {targetCanvas.name}");
        }

        // 全局数量控制，超出则回收最早的
        while (activeDisplays.Count > maxGraffitiCount)
        {
            var oldest = activeDisplays[0];
            activeDisplays.RemoveAt(0);
            RecycleGraffiti(oldest);
            Debug.Log($"[GraffitiManager] 回收最旧的涂鸦，当前数量: {activeDisplays.Count}");
        }
    }
    
    /// <summary>
    /// 记录涂鸦状态信息
    /// </summary>
    private void LogGraffitiStatus()
    {
        Debug.Log($"[GraffitiManager] 涂鸦状态: 队列{graffitiQueue.Count}, 显示中{activeDisplays.Count}, 待显示{pendingDisplayQueue.Count}");
        
        if (canvasControllers != null)
        {
            for (int i = 0; i < canvasControllers.Length; i++)
            {
                if (canvasControllers[i] != null)
                {
                    Debug.Log($"[GraffitiManager] {canvasControllers[i].name}: {canvasControllers[i].activeDisplays.Count}/33");
                }
            }
        }
    }
    
    /// <summary>
    /// 寻找可用的Canvas，实现循环逻辑
    /// </summary>
    private CanvasController FindAvailableCanvas()
    {
        // 首先尝试Canvas1
        if (!canvasControllers[0].IsFull())
        {
            return canvasControllers[0];
        }
        
        // Canvas1满了，尝试Canvas2
        if (!canvasControllers[1].IsFull())
        {
            return canvasControllers[1];
        }
        
        // Canvas2满了，尝试Canvas3
        if (!canvasControllers[2].IsFull())
        {
            return canvasControllers[2];
        }
        
        // 所有Canvas都满了，返回null
        return null;
    }
    // 修改回收方法
    public void RecycleGraffiti(GraffitiDisplay display)
    {
        display.gameObject.SetActive(false);
        activeDisplays.Remove(display);
        displayPool.Push(display.gameObject);

        // 通知CanvasController移除涂鸦
        if (display.CurrentCanvas != null)
        {
            display.CurrentCanvas.RemoveGraffitiDisplay(display);
        }
    }
    // ��������任
    private void SetRandomTransform(Transform t)
    {
        // ���λ�� - ֻ����Xλ�ã�Yλ����Ϳѻ��ʾ�������
        // t.localPosition = new Vector3(
            // UnityEngine.Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            // 0, // Yλ����Ϊ0����Ϳѻ��ʾ�������
            // 0
        // );

        // �����ת
        t.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(minRotation, maxRotation));

        // �������
        float scale = UnityEngine.Random.Range(minScale, maxScale);
        t.localScale = new Vector3(scale, scale, 1);
    }
    // 获取涂鸦显示顺序信息（用于调试）
    public string GetGraffitiOrderInfo()
    {
        string info = $"总涂鸦数量: {graffitiQueue.Count}\n";
        info += $"待显示队列: {pendingDisplayQueue.Count}\n";
        info += $"活跃显示: {activeDisplays.Count}\n";
        
        if (graffitiQueue.Count > 0)
        {
            info += "\n最近添加的涂鸦:\n";
            var recentGraffiti = graffitiQueue.Take(5).ToList();
            foreach (var graffiti in recentGraffiti)
            {
                info += $"ID: {graffiti.id.Substring(0, 8)}..., 时间: {graffiti.createTime:HH:mm:ss.fff}\n";
            }
        }
        
        return info;
    }

    private void OnDestroy()
    {
        foreach (var graffiti in graffitiQueue)
        {
            if (graffiti.texture != null)
            {
                Destroy(graffiti.texture);
            }
        }
        graffitiQueue.Clear();
        graffitiDictionary.Clear();

        foreach (var display in activeDisplays)
        {
            if (display != null && display.graffitiImage != null && display.graffitiImage.texture != null)
            {
                Destroy(display.graffitiImage.texture);
            }
        }
        activeDisplays.Clear();

        while (displayPool.Count > 0)
        {
            GameObject obj = displayPool.Pop();
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}

public class GraffitiData
{
    public string id;
    public Texture2D texture;
    public DateTime createTime;
}