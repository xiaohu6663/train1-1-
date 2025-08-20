using UnityEngine;

public class SystemTest : MonoBehaviour
{
    [Header("测试设置")]
    public bool testOnStart = false;
    public string testConfig = "{model1;D1:R,D2:G,D3:B}";
    
    void Start()
    {
        if (testOnStart)
        {
            TestSystem();
        }
    }
    
    void Update()
    {
        // 按T键测试系统
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestSystem();
        }
    }
    
    public void TestSystem()
    {
        Debug.Log("=== 系统测试开始 ===");
        
        // 测试TrainManager
        if (TrainManager.Instance != null)
        {
            Debug.Log("✓ TrainManager 存在");
            TrainManager.Instance.CreateTrain(testConfig);
        }
        else
        {
            Debug.LogWarning("✗ TrainManager 不存在");
        }
        
        // 测试TrackManager
        if (TrackManager.Instance != null)
        {
            Debug.Log("✓ TrackManager 存在");
            TrackManager.Instance.CreateTrain(testConfig);
        }
        else
        {
            Debug.LogWarning("✗ TrackManager 不存在");
        }
        
        // 测试UDPManager
        if (UDPManager.instance != null)
        {
            Debug.Log("✓ UDPManager 存在");
        }
        else
        {
            Debug.LogWarning("✗ UDPManager 不存在");
        }
        
        Debug.Log("=== 系统测试完成 ===");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label("系统测试", GUI.skin.box);
        
        if (GUILayout.Button("测试系统"))
        {
            TestSystem();
        }
        
        GUILayout.Label("按T键快速测试");
        GUILayout.EndArea();
    }
}


