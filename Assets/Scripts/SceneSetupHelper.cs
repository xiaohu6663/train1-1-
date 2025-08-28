using UnityEngine;
using System.Collections.Generic;

public class SceneSetupHelper : MonoBehaviour
{
    [Header("自动设置选项")]
    public bool autoSetupOnStart = true;
    public bool createTrainPrefab = true;
    public bool createSpawnPoint = true;
    
    [Header("预制件设置")]
    public GameObject trainPrefabTemplate;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupScene();
        }
    }
    
    void Update()
    {
        // 按S键快速设置
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetupScene();
        }
    }
    
    public void SetupScene()
    {
        Debug.Log("=== 开始自动设置场景 ===");
        
        // 1. 创建或检查MainThreadDispatcher
        SetupMainThreadDispatcher();
        
        // 2. 创建或检查UDPManager
        SetupUDPManager();
        
        // 3. 创建或检查TrainManager
        SetupTrainManager();
        
        // 4. 创建或检查TrackManager
        SetupTrackManager();
        
        // 5. 创建列车预制件
        if (createTrainPrefab)
        {
            SetupTrainPrefab();
        }
        
        // 6. 创建生成点
        if (createSpawnPoint)
        {
            SetupSpawnPoint();
        }
        
        Debug.Log("=== 场景设置完成 ===");
    }
    
    private void SetupMainThreadDispatcher()
    {
        if (MainThreadDispatcher.Instance == null)
        {
            GameObject dispatcherObj = new GameObject("MainThreadDispatcher");
            dispatcherObj.AddComponent<MainThreadDispatcher>();
            Debug.Log("✓ 创建了MainThreadDispatcher");
        }
        else
        {
            Debug.Log("✓ MainThreadDispatcher已存在");
        }
    }
    
    private void SetupUDPManager()
    {
        if (UDPManager.instance == null)
        {
            GameObject udpObj = new GameObject("UDPManager");
            UDPManager udpManager = udpObj.AddComponent<UDPManager>();
            Debug.Log("✓ 创建了UDPManager");
        }
        else
        {
            Debug.Log("✓ UDPManager已存在");
        }
    }
    
    private void SetupTrainManager()
    {
        if (TrainManager.Instance == null)
        {
            GameObject trainManagerObj = new GameObject("TrainManager");
            TrainManager trainManager = trainManagerObj.AddComponent<TrainManager>();
            
            // 设置默认值
            trainManager.maxTrains = 30;
            trainManager.segmentSpeedScale = 1f;
            
            Debug.Log("✓ 创建了TrainManager");
        }
        else
        {
            Debug.Log("✓ TrainManager已存在");
        }
    }
    
    private void SetupTrackManager()
    {
        if (TrackManager.Instance == null)
        {
            GameObject trackManagerObj = new GameObject("TrackManager");
            TrackManager trackManager = trackManagerObj.AddComponent<TrackManager>();
            
            // 设置默认配置
            trackManager.maxGlobalTrains = 30;
            trackManager.maxTrains = 30;
            trackManager.showDebugInfo = true;
            trackManager.loopAnimation = true;
            trackManager.animationSpeed = 1f;
            
            Debug.Log("✓ 创建了TrackManager");
        }
        else
        {
            Debug.Log("✓ TrackManager已存在");
        }
    }
    
    private void SetupTrainPrefab()
    {
        // 检查是否已有列车预制件
        GameObject existingPrefab = GameObject.Find("TrainPrefab");
        if (existingPrefab != null)
        {
            Debug.Log("✓ 列车预制件已存在");
            return;
        }
        
        // 创建简单的列车预制件
        GameObject trainPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainPrefab.name = "TrainPrefab";
        trainPrefab.transform.localScale = new Vector3(2f, 1f, 4f);
        
        // 添加必要的组件
        trainPrefab.AddComponent<TrainController>();
        trainPrefab.AddComponent<TrainData>();
        
        // 添加Animator组件
        Animator animator = trainPrefab.AddComponent<Animator>();
        
        // 尝试加载动画控制器
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("SingleTrackController");
        if (controller == null)
        {
            // 如果找不到，尝试从Assets加载
            controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/SingleTrackController.controller");
        }
        if (controller == null)
        {
            // 如果还是找不到，尝试加载SimpleTrainController作为备用
            controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/SimpleTrainController.controller");
        }
        
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log("✓ 为列车预制件添加了动画控制器");
        }
        else
        {
            Debug.LogWarning("⚠ 未找到动画控制器，列车将使用默认移动");
        }
        
        // 设置材质颜色
        Renderer renderer = trainPrefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.blue;
        }
        
        Debug.Log("✓ 创建了列车预制件");
        
        // 将预制件分配给管理器
        if (TrainManager.Instance != null)
        {
            TrainManager.Instance.trainPrefab = trainPrefab;
        }
        
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.trainPrefab = trainPrefab;
        }
    }
    
    private void SetupSpawnPoint()
    {
        // 检查是否已有生成点
        Transform existingSpawnPoint = GameObject.Find("SpawnPoint")?.transform;
        if (existingSpawnPoint != null)
        {
            Debug.Log("✓ 生成点已存在");
            return;
        }
        
        // 创建生成点
        GameObject spawnPointObj = new GameObject("SpawnPoint");
        spawnPointObj.transform.position = new Vector3(0, 1, 0);
        
        // 创建一个可视化标记
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "SpawnMarker";
        marker.transform.SetParent(spawnPointObj.transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = Vector3.one * 0.5f;
        
        // 设置材质颜色
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            markerRenderer.material.color = Color.green;
        }
        
        // 分配给TrainManager
        if (TrainManager.Instance != null)
        {
            TrainManager.Instance.spawnPoint = spawnPointObj.transform;
        }
        
        // 分配给TrackManager
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.entryPoint = spawnPointObj.transform;
        }
        
        Debug.Log("✓ 创建了生成点");
    }
    

    
    private void CheckComponentStatus()
    {
        Debug.Log("=== 组件状态检查 ===");
        
        Debug.Log($"MainThreadDispatcher: {(MainThreadDispatcher.Instance != null ? "✓" : "✗")}");
        Debug.Log($"UDPManager: {(UDPManager.instance != null ? "✓" : "✗")}");
        Debug.Log($"TrainManager: {(TrainManager.Instance != null ? "✓" : "✗")}");
        Debug.Log($"TrackManager: {(TrackManager.Instance != null ? "✓" : "✗")}");
        
        if (TrainManager.Instance != null)
        {
            Debug.Log($"TrainManager.trainPrefab: {(TrainManager.Instance.trainPrefab != null ? "✓" : "✗")}");
            Debug.Log($"TrainManager.spawnPoint: {(TrainManager.Instance.spawnPoint != null ? "✓" : "✗")}");
        }
        
        if (TrackManager.Instance != null)
        {
            Debug.Log($"TrackManager.trainPrefab: {(TrackManager.Instance.trainPrefab != null ? "✓" : "✗")}");
        }
        
        Debug.Log("=== 检查完成 ===");
    }
}
