using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public static TrackManager Instance;
    
    [Header("轨道配置")]
    public Transform entryPoint;
    public Transform exitPoint;
    public int maxTrains = 30;
    public AnimationClip trackAnimation;
    
    [Header("动画设置")]
    public float animationSpeed = 1f;
    public bool loopAnimation = true;
    
    [Header("全局设置")]
    public int maxGlobalTrains = 30;
    public GameObject trainPrefab;
    
    [Header("调试信息")]
    public bool showDebugInfo = true;
    [Tooltip("严格校验颜色区域；若找不到任何一个区域则取消创建")] public bool strictRegionValidation = true;
    
    private List<GameObject> currentTrains = new List<GameObject>();
    private Queue<GameObject> allTrains = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // 验证配置
        ValidateConfiguration();
    }
    
    private void ValidateConfiguration()
    {
        if (trainPrefab == null)
        {
            Debug.LogError("TrackManager: trainPrefab未设置！");
        }
        
        if (entryPoint == null)
        {
            Debug.LogError("TrackManager: 轨道入口点未设置！");
        }
        
        if (trackAnimation == null)
        {
            Debug.LogWarning("TrackManager: 轨道动画片段未设置！");
        }
    }
    
    // 创建新列车并添加到轨道
    public GameObject CreateTrain(string config = "")
    {
        if (trainPrefab == null)
        {
            Debug.LogError("无法创建列车：trainPrefab未设置");
            return null;
        }
        
        // 检查全局列车数量限制
        if (allTrains.Count >= maxGlobalTrains)
        {
            RemoveOldestTrain();
        }
        
        // 检查轨道是否有空间
        if (currentTrains.Count >= maxTrains)
        {
            Debug.Log($"轨道已满 ({currentTrains.Count}/{maxTrains})，无法创建新列车");
            return null;
        }
        
        // 实例化前先校验：颜色代码与区域名
        if (!string.IsNullOrEmpty(config))
        {
            if (!ValidateColorCodes(config, out string unknownCode))
            {
                string msg = $"取消创建列车：不支持的颜色代码 '{unknownCode}'。允许值: R/G/B/Y/W/K 以及自定义映射";
                Debug.Log(msg);
                Debug.LogError(msg);
                return null;
            }
            if (!ValidateRegionsOnPrefab(trainPrefab, config, out string missingRegion))
            {
                string msg = $"取消创建列车：找不到材质区域 '{missingRegion}'。";
                Debug.Log(msg);
                Debug.LogError(msg);
                return null;
            }
        }

        // 创建列车
        GameObject newTrain = Instantiate(trainPrefab);
        newTrain.name = $"Train_{System.DateTime.Now:HHmmss}";
        
        // 添加到轨道
        bool success = TryAddTrainToTrack(newTrain);
        if (!success)
        {
            Destroy(newTrain);
            return null;
        }
        
        // 处理颜色配置（如果有的话）：直接对已有实例应用颜色
        if (string.IsNullOrEmpty(config))
        {
            Debug.Log("TrackManager: 配置为空，跳过颜色应用");
        }
        else if (TrainManager.Instance == null)
        {
            Debug.LogWarning("TrackManager: TrainManager.Instance 为空，使用内置颜色应用");
            ApplyColorsInline(newTrain, config);
        }
        else
        {
            Debug.Log($"TrackManager: 调用颜色应用，config={config}");
            TrainManager.Instance.ApplyColorsToExistingTrain(newTrain, config);
        }
        
        return newTrain;
    }

    // 解析配置并校验所有区域名是否存在于预制体材质列表中
    private bool ValidateRegionsOnPrefab(GameObject prefab, string config, out string missingRegion)
    {
        missingRegion = null;
        if (prefab == null) return true;

        var regions = new List<string>(ExtractRegions(config));
        HashSet<string> prefabMaterialNames = GetAllMaterialNames(prefab);
        Debug.Log($"[Validate] config={config}, 提取区域=[{string.Join(", ", regions)}], 预制体材质=[{string.Join(", ", prefabMaterialNames)}]");
        if (prefabMaterialNames == null || prefabMaterialNames.Count == 0)
        {
            Debug.LogWarning("ValidateRegionsOnPrefab: 未能从预制体读取到材质名称");
            return !strictRegionValidation; // 严格模式下直接判失败
        }

        foreach (string region in regions)
        {
            if (!prefabMaterialNames.Contains(region))
            {
                missingRegion = region;
                return false;
            }
        }
        return true;
    }

    private static IEnumerable<string> ExtractRegions(string config)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(config)) return list;

        var match = Regex.Match(config, "\\{[^;]*;([^}]*)\\}");
        if (!match.Success) return list;

        string raw = match.Groups[1].Value.Replace('，', ',').Replace('；', ',');
        var parts = raw.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var p = part.Replace('：', ':');
            int idx = p.IndexOf(':');
            if (idx <= 0) continue;
            string region = p.Substring(0, idx).Trim();
            if (!string.IsNullOrEmpty(region)) list.Add(region);
        }
        return list;
    }

    private static IEnumerable<string> ExtractCodes(string config)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(config)) return list;

        var match = Regex.Match(config, "\\{[^;]*;([^}]*)\\}");
        if (!match.Success) return list;

        string raw = match.Groups[1].Value.Replace('，', ',').Replace('；', ',');
        var parts = raw.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var p = part.Replace('：', ':');
            int idx = p.IndexOf(':');
            if (idx <= 0 || idx >= p.Length - 1) continue;
            string code = p.Substring(idx + 1).Trim();
            if (!string.IsNullOrEmpty(code)) list.Add(code);
        }
        return list;
    }

    private bool ValidateColorCodes(string config, out string unknownCode)
    {
        unknownCode = null;
        // 默认允许的颜色代码
        var allowed = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        { "R", "G", "B", "Y", "W", "K" };

        // 加上 TrainManager 中的自定义映射
        if (TrainManager.Instance != null && TrainManager.Instance.colorMappings != null)
        {
            foreach (var m in TrainManager.Instance.colorMappings)
            {
                if (m == null || string.IsNullOrEmpty(m.colorCode)) continue;
                allowed.Add(m.colorCode.Trim());
            }
        }

        foreach (var code in ExtractCodes(config))
        {
            string c = code.Trim();
            if (!allowed.Contains(c))
            {
                unknownCode = c;
                return false;
            }
        }
        return true;
    }

    private static HashSet<string> GetAllMaterialNames(GameObject prefab)
    {
        var names = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        if (prefab == null) return names;

        var renderers = prefab.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                string n = m.name.Replace(" (Instance)", "");
                names.Add(n);
            }
        }
        return names;
    }

    // 备用：在没有 TrainManager 的情况下直接应用颜色
    private void ApplyColorsInline(GameObject train, string config)
    {
        if (string.IsNullOrEmpty(config) || train == null) return;

        var match = Regex.Match(config, "\\{[^;]*;([^}]*)\\}");
        if (!match.Success) return;

        string raw = match.Groups[1].Value.Replace('，', ',').Replace('；', ',');
        var parts = raw.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        int applied = 0;
        foreach (var part in parts)
        {
            var p = part.Replace('：', ':');
            int idx = p.IndexOf(':');
            if (idx <= 0) continue;
            string region = p.Substring(0, idx).Trim();
            string code = p.Substring(idx + 1).Trim().ToUpper();

            var renderers = train.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                var mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null) continue;
                    string n = m.name.Replace(" (Instance)", "");
                    if (!string.Equals(n, region, System.StringComparison.OrdinalIgnoreCase)) continue;

                    Color c;
                    if (!TryColorByCode(code, out c)) continue;
                    if (m.HasProperty("_Color")) m.SetColor("_Color", c);
                    applied++;
                }
                r.materials = mats;
            }
        }
        Debug.Log($"TrackManager: 内置颜色应用完成，applied={applied}");
    }

    private bool TryColorByCode(string code, out Color color)
    {
        switch (code)
        {
            case "R": color = Color.red; return true;
            case "G": color = Color.green; return true;
            case "B": color = Color.blue; return true;
            case "Y": color = Color.yellow; return true;
            case "W": color = Color.white; return true;
            case "K": color = Color.black; return true;
            default: color = default; return false;
        }
    }
    
    // 尝试添加列车到轨道
    public bool TryAddTrainToTrack(GameObject train)
    {
        if (currentTrains.Count >= maxTrains)
        {
            Debug.Log($"轨道已满 ({currentTrains.Count}/{maxTrains})");
            return false;
        }
        
        if (allTrains.Count >= maxGlobalTrains)
        {
            RemoveOldestTrain();
        }
        
        // 设置列车位置
        if (entryPoint != null)
        {
            train.transform.position = entryPoint.position;
            train.transform.rotation = entryPoint.rotation;
        }
        
        // 添加到轨道
        currentTrains.Add(train);
        allTrains.Enqueue(train);
        
        // 设置动画控制器
        TrainController controller = train.GetComponent<TrainController>();
        if (controller != null)
        {
            controller.InitializeForTrack("Track");
        }
        else
        {
            Debug.LogWarning("列车缺少TrainController组件");
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"列车添加到轨道, 当前: {currentTrains.Count}/{maxTrains}, 全局: {allTrains.Count}/{maxGlobalTrains}");
        }
        return true;
    }
    
    // 列车完成轨道动画后的处理
    public void OnTrainCompletedTrack(GameObject train)
    {
        if (loopAnimation)
        {
            // 如果轨道是循环的，重新开始动画
            TrainController controller = train.GetComponent<TrainController>();
            if (controller != null)
            {
                controller.SwitchToTrack("Track");
            }
            
            if (showDebugInfo)
            {
                Debug.Log("列车完成轨道，重新开始循环");
            }
        }
        else
        {
            // 如果不循环，移除列车
            RemoveTrain(train);
        }
    }
    
    private void RemoveTrain(GameObject train)
    {
        currentTrains.Remove(train);
        
        // 从全局队列中移除
        Queue<GameObject> newQueue = new Queue<GameObject>();
        while (allTrains.Count > 0)
        {
            GameObject t = allTrains.Dequeue();
            if (t != train)
            {
                newQueue.Enqueue(t);
            }
        }
        allTrains = newQueue;
        
        // 销毁列车
        Destroy(train);
        
        if (showDebugInfo)
        {
            Debug.Log("列车完成轨道，已移除");
        }
    }
    
    private void RemoveOldestTrain()
    {
        if (allTrains.Count == 0) return;
        
        GameObject oldestTrain = allTrains.Dequeue();
        
        // 从轨道移除
        currentTrains.Remove(oldestTrain);
        
        // 销毁或回收
        Destroy(oldestTrain);
        Debug.Log("移除最早列车，新列车加入");
    }
    
    // 颜色配置已在 CreateTrain 中直接应用，不再通过旧流程创建新列车
    
    // 清理无效引用
    public void CleanupTrainLists()
    {
        currentTrains.RemoveAll(t => t == null);
        
        // 清理全局队列
        Queue<GameObject> validTrains = new Queue<GameObject>();
        while (allTrains.Count > 0)
        {
            GameObject train = allTrains.Dequeue();
            if (train != null) validTrains.Enqueue(train);
        }
        allTrains = validTrains;
    }
    
    public int GetGlobalTrainCount()
    {
        return allTrains.Count;
    }
    
    public int GetTrackTrainCount()
    {
        return currentTrains.Count;
    }
    
    // 获取轨道信息
    public string GetTrackInfo()
    {
        return $"轨道列车: {currentTrains.Count}/{maxTrains}, 全局列车: {allTrains.Count}/{maxGlobalTrains}";
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("单轨道列车管理系统", GUI.skin.box);
        GUILayout.Label($"全局列车: {allTrains.Count}/{maxGlobalTrains}");
        GUILayout.Label($"轨道列车: {currentTrains.Count}/{maxTrains}");
        GUILayout.Label($"循环模式: {(loopAnimation ? "开启" : "关闭")}");
        GUILayout.Label($"动画片段: {(trackAnimation != null ? trackAnimation.name : "未设置")}");
        GUILayout.EndArea();
    }
}