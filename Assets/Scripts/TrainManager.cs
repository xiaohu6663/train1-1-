using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;


public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance;
    
    [Header("列车预制体")]
    public GameObject trainPrefab;
    
    [Header("生成点")]
    public Transform spawnPoint;
    
    [Header("移动路径点")]
    public List<Transform> movementPath;
    
    [Header("最大列车数量"), Range(1, 100)]
    public int maxTrains = 30;

    [Header("路径移动调节")]
    [Range(0.1f, 10f)]
    public float segmentSpeedScale = 1f; // 全局段速缩放（作用于每辆列车的 PathFollower.segmentSpeedScale）
    [Tooltip("每段速度缩放，长度建议与路径点数量相同：下标 i 表示从 WayPoint[i] 到 WayPoint[(i+1)%N] 的段")]
    public List<float> perSegmentSpeedScales = new List<float>();

    [Header("颜色映射")]
    public List<ColorMapping> colorMappings = new List<ColorMapping>();
    
    [System.Serializable]
    public class ColorMapping
    {
        public string colorCode;
        public Color colorValue;
    }
    
    private Queue<GameObject> activeTrains = new Queue<GameObject>();
    private Dictionary<string, Color> colorMap = new Dictionary<string, Color>();
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("TrainManager: 单例初始化");
        }
        else
        {
            Destroy(gameObject);
        }
        
        propBlock = new MaterialPropertyBlock();
        
        // 自动补全路径点（若未在 Inspector 配置）
        AutoPopulateMovementPathIfEmpty();
        EnsurePerSegmentScaleLength();

        // 初始化颜色映射
        Debug.Log("初始化颜色映射...");
        foreach (var mapping in colorMappings)
        {
            colorMap[mapping.colorCode.ToUpper()] = mapping.colorValue;
            Debug.Log($"添加颜色映射: {mapping.colorCode} -> {mapping.colorValue}");
        }
        // 确保常用默认颜色存在
        EnsureDefaultColorMap();
        Debug.Log($"总共添加了 {colorMap.Count} 个颜色映射");
    }

    private void OnValidate()
    {
        EnsurePerSegmentScaleLength();
    }

    // 确保 perSegmentSpeedScales 的长度与路径点数量一致，不足补 1f
    private void EnsurePerSegmentScaleLength()
    {
        if (movementPath == null) return;
        int n = movementPath.Count;
        if (n <= 0) return;
        if (perSegmentSpeedScales == null) perSegmentSpeedScales = new List<float>();
        while (perSegmentSpeedScales.Count < n) perSegmentSpeedScales.Add(1f);
        if (perSegmentSpeedScales.Count > n) perSegmentSpeedScales.RemoveRange(n, perSegmentSpeedScales.Count - n);
    }

    // 若未在 Inspector 配置路径点，尝试自动收集
    private void AutoPopulateMovementPathIfEmpty()
    {
        if (movementPath != null && movementPath.Count > 0) return;

        var collected = new List<Transform>();

        // 方案1：查找名为 "Waypoints" 或 "Path" 的容器的直接子节点顺序
        var containerGo = GameObject.Find("Waypoints") ?? GameObject.Find("Path");
        if (containerGo != null)
        {
            var parent = containerGo.transform;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child != parent) collected.Add(child);
            }
        }

        // 方案2：按名称收集所有名含 "Waypoint" 的对象（按名称排序）
        if (collected.Count == 0)
        {
            var all = GameObject.FindObjectsOfType<Transform>()
                .Where(t => t != null && t.name.IndexOf("Waypoint", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(t => t.name, StringComparer.Ordinal)
                .ToList();
            collected.AddRange(all);
        }

        if (collected.Count > 0)
        {
            movementPath = collected;
            Debug.Log($"自动收集到 {movementPath.Count} 个路径点。");
        }
        else
        {
            Debug.LogWarning("未找到路径点。请在 Inspector 配置 Movement Path，或创建 'Waypoints' 容器并添加子节点。");
        }
    }

    private void EnsureDefaultColorMap()
    {
        void Add(string code, Color c)
        {
            var key = code.ToUpper();
            if (!colorMap.ContainsKey(key)) colorMap[key] = c;
        }
        Add("R", Color.red);
        Add("G", Color.green);
        Add("B", Color.blue);
        Add("Y", Color.yellow);
        Add("W", Color.white);
        Add("K", Color.black);
    }

    // 在实例化前验证：颜色配置中的区域名在预制体材质中是否存在
    public bool ValidateColorsForPrefab(GameObject prefab, string config, out string missingRegion)
    {
        missingRegion = null;
        if (prefab == null) return true;
        if (string.IsNullOrEmpty(config)) return true; // 无颜色要求则不做限制

        var match = Regex.Match(config, @"\{(\w+);([^}]+)\}");
        if (!match.Success)
        {
            // 配置格式不标准，放行但给出提示
            Debug.LogWarning($"ValidateColorsForPrefab: 配置格式不正确: {config}");
            return true;
        }

        string colorConfigRaw = match.Groups[2].Value
            .Replace('，', ',')
            .Replace('；', ',');
        string[] colorConfigs = colorConfigRaw.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);

        foreach (string configPart in colorConfigs)
        {
            var normalized = configPart.Replace('：', ':');
            string[] parts = normalized.Split(':');
            if (parts.Length != 2) continue;

            string region = parts[0].Trim();

            var targets = FindRendererMaterialsByName(prefab.transform, region);
            if (targets == null || targets.Count == 0)
            {
                missingRegion = region;
                return false;
            }
        }

        return true;
    }

    public void CreateTrain(string config)
    {
        try
        {
            // 检查必要组件是否已设置
            if (trainPrefab == null)
            {
                Debug.LogError("Train Prefab 未设置！请在Inspector中设置列车预制体。");
                return;
            }
            
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn Point 未设置！请在Inspector中设置生成点。");
                return;
            }

            // 解析消息格式 {model1;D1:R,D2:G,D3:B}
            var match = Regex.Match(config, @"\{(\w+);([^}]+)\}");
            if (!match.Success)
            {
                Debug.LogError($"无效的列车配置: {config}");
                return;
            }

            string modelID = match.Groups[1].Value;
            // 兼容全角或异常逗号，统一按','切分
            string colorConfigRaw = match.Groups[2].Value
                .Replace('，', ',')
                .Replace('；', ',');
            string[] colorConfigs = colorConfigRaw.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);

            // 创建新列车
            GameObject newTrain = Instantiate(trainPrefab, spawnPoint.position, spawnPoint.rotation);
            newTrain.name = $"Train_{System.DateTime.Now:HHmmss}";
            
            // 配置颜色
            foreach (string configPart in colorConfigs)
            {
                // 兼容全角/异常分隔符
                var normalized = configPart.Replace('：', ':');
                string[] parts = normalized.Split(':');
                if (parts.Length != 2) continue;

                string region = parts[0].Trim();
                string colorCode = parts[1].Trim().ToUpper();
                
                // 通过材质名称查找需要修改颜色的具体材质槽位
                List<RendererMaterialRef> targetMaterials = FindRendererMaterialsByName(newTrain.transform, region);
                if (targetMaterials.Count > 0)
                {
                    Debug.Log($"找到使用材质 '{region}' 的材质槽位: {targetMaterials.Count} 个");
                    foreach (var rm in targetMaterials)
                    {
                        Debug.Log($"开始应用颜色到: {rm.renderer.name} 的材质槽 {rm.materialIndex}");
                        ApplyTintColorToMaterialSlot(rm.renderer, rm.materialIndex, colorCode);
                    }
                }
                else
                {
                    Debug.LogWarning($"未找到使用材质 '{region}' 的材质槽，请检查材质名称");
                    // 输出所有材质信息用于调试
                    Debug.Log("列车所有材质信息:");
                    PrintAllMaterials(newTrain.transform);
                }
            }
            
            // 尝试使用新的TrackManager系统，如果可用的话
            if (TrackManager.Instance != null)
            {
                if (!TrackManager.Instance.TryAddTrainToTrack(newTrain))
                {
                    Debug.LogWarning("无法添加到轨道，列车将被销毁");
                    Destroy(newTrain);
                    return;
                }
            }
            else
            {
                // 回退到旧的PathFollower系统
                // 配置移动路径（循环）
                PathFollower follower = newTrain.AddComponent<PathFollower>();
                follower.waypoints = movementPath;
                follower.minSpeed = 4f;
                follower.maxSpeed = 7f;
                follower.loop = true;
                follower.startFromFirst = true;
                follower.startFromNearestNext = false;
                follower.arriveThreshold = 5f;
                follower.pingPong = true;
                follower.segmentSpeedScale = segmentSpeedScale;
                if (perSegmentSpeedScales != null)
                {
                    follower.segmentSpeedScales = new List<float>(perSegmentSpeedScales);
                }
                
                follower.debugLogs = true;
                Debug.Log($"为新列车分配路径点数量: { (movementPath != null ? movementPath.Count : 0) }，速度区间: {follower.minSpeed}-{follower.maxSpeed}");

                                 // 添加数据记录
                 TrainData data = newTrain.AddComponent<TrainData>();
                 data.trainID = Guid.NewGuid().ToString();
            }
            
            // 管理列车数量
            activeTrains.Enqueue(newTrain);
            if (activeTrains.Count > maxTrains)
            {
                GameObject oldest = activeTrains.Dequeue();
                Destroy(oldest);
            }
            
            Debug.Log($"创建新列车: {newTrain.name}，当前列车数: {activeTrains.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"创建列车失败: {e.Message}");
        }
    }

    // 对已存在的列车应用颜色映射（不创建新对象）
    public void ApplyColorsToExistingTrain(GameObject targetTrain, string config)
    {
        if (targetTrain == null)
        {
            Debug.LogError("ApplyColorsToExistingTrain: 目标列车为空");
            return;
        }

        try
        {
            if (string.IsNullOrEmpty(config))
            {
                Debug.LogWarning("ApplyColorsToExistingTrain: 配置为空");
                return;
            }

            // 解析消息格式 {model1;D1:R,D2:G,D3:B}
            var match = Regex.Match(config, @"\{(\w+);([^}]+)\}");
            if (!match.Success)
            {
                Debug.LogWarning($"ApplyColorsToExistingTrain: 无效的颜色配置: {config}");
                return;
            }

            string colorConfigRaw = match.Groups[2].Value
                .Replace('，', ',')
                .Replace('；', ',');
            string[] colorConfigs = colorConfigRaw.Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);

            Debug.Log($"ApplyColorsToExistingTrain: 开始应用颜色，目标={targetTrain.name}, 片段数={colorConfigs.Length}");

            int applied = 0;
            foreach (string configPart in colorConfigs)
            {
                var normalized = configPart.Replace('：', ':');
                string[] parts = normalized.Split(':');
                if (parts.Length != 2) continue;

                string region = parts[0].Trim();
                string colorCode = parts[1].Trim().ToUpper();

                List<RendererMaterialRef> targetMaterials = FindRendererMaterialsByName(targetTrain.transform, region);
                if (targetMaterials.Count > 0)
                {
                    Debug.Log($"ApplyColorsToExistingTrain: 找到区域 {region} 的材质槽 {targetMaterials.Count} 个");
                    foreach (var rm in targetMaterials)
                    {
                        ApplyTintColorToMaterialSlot(rm.renderer, rm.materialIndex, colorCode);
                        applied++;
                    }
                }
                else
                {
                    Debug.LogWarning($"未找到使用材质 '{region}' 的材质槽，请检查材质名称");
                    Debug.Log("列车所有材质信息:");
                    PrintAllMaterials(targetTrain.transform);
                }
            }

            if (applied == 0)
            {
                Debug.Log("未能应用任何颜色");
            }
            else
            {
                Debug.Log($"已为 {targetTrain.name} 应用 {applied} 处颜色映射");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"应用颜色失败: {ex.Message}");
        }
    }
    
    private void ApplyTintColor(Renderer renderer, string colorCode, string materialNameHint = null)
    {
        if (!colorMap.ContainsKey(colorCode))
        {
            Debug.LogWarning($"未定义的颜色代码: {colorCode}");
            return;
        }
        
        Color targetColor = colorMap[colorCode];
        float intensity = 0.7f; // 默认混合强度
        
        Debug.Log($"应用颜色: {colorCode} -> {targetColor}, 强度: {intensity}");
        
        // 确保使用正确的着色器
        // 如传入了材质名提示且当前材质名不匹配，尝试切到对应shader（D1.shader名为 Unlit/D1）
        if (renderer.sharedMaterial.shader.name != "Unlit/D1")
        {
            Debug.Log("创建新的D1材质");
            Material newMat = new Material(Shader.Find("Unlit/D1"));
            newMat.mainTexture = renderer.sharedMaterial.mainTexture;
            renderer.material = newMat;
        }
        
        // 直接设置材质属性
        if (renderer.material.HasProperty("_Color"))
        {
            renderer.material.SetColor("_Color", targetColor);
            Debug.Log($"设置_Color属性: {targetColor}");
        }
        else
        {
            Debug.LogWarning("材质没有_Color属性");
        }
        
        if (renderer.material.HasProperty("_TintIntensity"))
        {
            renderer.material.SetFloat("_TintIntensity", intensity);
            Debug.Log($"设置_TintIntensity属性: {intensity}");
        }
        else
        {
            Debug.LogWarning("材质没有_TintIntensity属性");
        }
        
        if (renderer.material.HasProperty("_Roughness"))
        {
            float roughness = renderer.material.GetFloat("_Roughness");
            renderer.material.SetFloat("_Roughness", roughness);
            Debug.Log($"保持_Roughness属性: {roughness}");
        }
        
        Debug.Log("颜色应用完成");
    }

    private struct RendererMaterialRef
    {
        public Renderer renderer;
        public int materialIndex;
    }

    // 查找名称匹配的材质槽（而非整个Renderer），避免错改其它材质
    private List<RendererMaterialRef> FindRendererMaterialsByName(Transform parent, string materialName)
    {
        var results = new List<RendererMaterialRef>();
        FindRendererMaterialsByNameRecursive(parent, materialName, results);
        return results;
    }

    private void FindRendererMaterialsByNameRecursive(Transform parent, string materialName, List<RendererMaterialRef> results)
    {
        var renderer = parent.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null) continue;
                string name = mats[i].name.Replace(" (Instance)", "");
                if (name.Equals(materialName, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new RendererMaterialRef { renderer = renderer, materialIndex = i });
                    Debug.Log($"匹配材质槽: {parent.name} -> {name} (index {i})");
                }
            }
        }

        foreach (Transform child in parent)
        {
            FindRendererMaterialsByNameRecursive(child, materialName, results);
        }
    }

    // 仅对指定材质槽设置颜色与强度，保留其它材质不变
    private void ApplyTintColorToMaterialSlot(Renderer renderer, int materialIndex, string colorCode)
    {
        if (!colorMap.TryGetValue(colorCode, out var targetColor))
        {
            Debug.LogWarning($"未定义的颜色代码: {colorCode}");
            return;
        }

        float intensity = 0.7f;

        // 实例化材质数组，避免影响其它实例
        var mats = renderer.materials; // 这会克隆数组及必要实例
        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
            Debug.LogWarning("材质索引越界");
            return;
        }

        var mat = mats[materialIndex];

        // 确保使用 D1 shader（Unlit/D1）
        if (mat.shader == null || mat.shader.name != "Unlit/D1")
        {
            var d1 = Shader.Find("Unlit/D1");
            if (d1 != null)
            {
                var newMat = new Material(d1);
                // 继承主纹理
                if (mat.HasProperty("_MainTex"))
                {
                    newMat.mainTexture = mat.mainTexture;
                }
                mats[materialIndex] = newMat;
                mat = newMat;
            }
        }

        if (mat.HasProperty("_Color")) mat.SetColor("_Color", targetColor);
        if (mat.HasProperty("_TintIntensity")) mat.SetFloat("_TintIntensity", intensity);

        renderer.materials = mats; // 回写

        Debug.Log($"颜色已应用到 {renderer.name} 的槽 {materialIndex}: {targetColor}");
    }
    
    // 深度查找子对象
    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        
        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        
        return null;
    }

    // 通过材质名称查找Renderer
    private List<Renderer> FindRenderersByMaterialName(Transform parent, string materialName)
    {
        List<Renderer> result = new List<Renderer>();
        FindRenderersByMaterialNameRecursive(parent, materialName, result);
        return result;
    }

    private void FindRenderersByMaterialNameRecursive(Transform parent, string materialName, List<Renderer> result)
    {
        // 检查当前对象是否有Renderer
        Renderer renderer = parent.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            string currentMaterialName = renderer.material.name;
            // 移除材质名称中的 "(Instance)" 后缀
            currentMaterialName = currentMaterialName.Replace(" (Instance)", "");
            
            if (currentMaterialName.Equals(materialName, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(renderer);
                Debug.Log($"找到匹配的Renderer: {parent.name}, 材质: {currentMaterialName}");
            }
        }
        
        // 递归检查子对象
        foreach (Transform child in parent)
        {
            FindRenderersByMaterialNameRecursive(child, materialName, result);
        }
    }

    // 打印所有材质信息（用于调试）
    private void PrintAllMaterials(Transform parent)
    {
        PrintAllMaterialsRecursive(parent, 0);
    }

    private void PrintAllMaterialsRecursive(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        Renderer renderer = parent.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            string materialName = renderer.material.name.Replace(" (Instance)", "");
            Debug.Log($"{indent}- {parent.name}: 材质 '{materialName}'");
        }
        else
        {
            Debug.Log($"{indent}- {parent.name}: 无材质");
        }
        
        foreach (Transform child in parent)
        {
            PrintAllMaterialsRecursive(child, depth + 1);
        }
    }
}