using UnityEngine;

/// <summary>
/// 临时修复脚本 - 用于解决编译错误
/// </summary>
public class TempFix : MonoBehaviour
{
    [Header("修复设置")]
    public bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixCompilationIssues();
        }
    }
    
    void Update()
    {
        // 按F键手动修复
        if (Input.GetKeyDown(KeyCode.F))
        {
            FixCompilationIssues();
        }
    }
    
    public void FixCompilationIssues()
    {
        Debug.Log("=== 开始修复编译问题 ===");
        
        // 检查并创建必要的组件
        CheckAndCreateComponents();
        
        // 验证系统状态
        ValidateSystem();
        
        Debug.Log("=== 修复完成 ===");
    }
    
    private void CheckAndCreateComponents()
    {
        // 检查TrainManager
        if (FindObjectOfType<TrainManager>() == null)
        {
            Debug.Log("创建TrainManager组件");
            GameObject trainManagerObj = new GameObject("TrainManager");
            trainManagerObj.AddComponent<TrainManager>();
        }
        
        // 检查TrackManager
        if (FindObjectOfType<TrackManager>() == null)
        {
            Debug.Log("创建TrackManager组件");
            GameObject trackManagerObj = new GameObject("TrackManager");
            trackManagerObj.AddComponent<TrackManager>();
        }
        
        // 检查UDPManager
        if (FindObjectOfType<UDPManager>() == null)
        {
            Debug.Log("创建UDPManager组件");
            GameObject udpManagerObj = new GameObject("UDPManager");
            udpManagerObj.AddComponent<UDPManager>();
        }
        
        // 检查MainThreadDispatcher
        if (FindObjectOfType<MainThreadDispatcher>() == null)
        {
            Debug.Log("创建MainThreadDispatcher组件");
            GameObject dispatcherObj = new GameObject("MainThreadDispatcher");
            dispatcherObj.AddComponent<MainThreadDispatcher>();
        }
    }
    
    private void ValidateSystem()
    {
        Debug.Log("验证系统组件:");
        
        if (TrainManager.Instance != null)
            Debug.Log("✓ TrainManager 正常");
        else
            Debug.LogWarning("✗ TrainManager 异常");
            
        if (TrackManager.Instance != null)
            Debug.Log("✓ TrackManager 正常");
        else
            Debug.LogWarning("✗ TrackManager 异常");
            
        if (UDPManager.instance != null)
            Debug.Log("✓ UDPManager 正常");
        else
            Debug.LogWarning("✗ UDPManager 异常");
            
        if (MainThreadDispatcher.Instance != null)
            Debug.Log("✓ MainThreadDispatcher 正常");
        else
            Debug.LogWarning("✗ MainThreadDispatcher 异常");
    }
    

    
    private void CreateTestTrain()
    {
        string testConfig = "{model1;D1:R,D2:G,D3:B}";
        
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.CreateTrain(testConfig);
        }
        else if (TrainManager.Instance != null)
        {
            TrainManager.Instance.CreateTrain(testConfig);
        }
        else
        {
            Debug.LogError("没有可用的列车管理器");
        }
    }
}


