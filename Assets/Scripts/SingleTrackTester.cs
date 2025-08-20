using UnityEngine;

public class SingleTrackTester : MonoBehaviour
{
    [Header("测试设置")]
    public bool autoTestOnStart = true;
    public float testInterval = 2f;
    public int maxTestTrains = 5;
    
    private float lastTestTime;
    private int testCount = 0;
    
    void Start()
    {
        if (autoTestOnStart)
        {
            Invoke("TestCreateTrain", 1f);
        }
    }
    
    void Update()
    {
        // 按T键手动测试
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestCreateTrain();
        }
        
        // 按C键清理所有列车
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllTrains();
        }
        
        // 自动测试
        if (autoTestOnStart && Time.time - lastTestTime > testInterval)
        {
            if (testCount < maxTestTrains)
            {
                TestCreateTrain();
                testCount++;
            }
            lastTestTime = Time.time;
        }
    }
    
    public void TestCreateTrain()
    {
        Debug.Log("=== 测试创建列车 ===");
        
        if (TrackManager.Instance != null)
        {
            GameObject train = TrackManager.Instance.CreateTrain("test");
            if (train != null)
            {
                Debug.Log($"✓ 成功创建列车: {train.name}");
            }
            else
            {
                Debug.LogError("✗ 创建列车失败");
            }
        }
        else
        {
            Debug.LogError("✗ TrackManager不存在");
        }
    }
    
    public void ClearAllTrains()
    {
        Debug.Log("=== 清理所有列车 ===");
        
        // 查找所有列车并销毁
        GameObject[] trains = GameObject.FindGameObjectsWithTag("Train");
        foreach (GameObject train in trains)
        {
            Destroy(train);
        }
        
        // 清理管理器中的引用
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.CleanupTrainLists();
        }
        
        Debug.Log("✓ 已清理所有列车");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 280, 250, 120));
        GUILayout.Label("单轨道测试工具", GUI.skin.box);
        
        if (GUILayout.Button("测试创建列车"))
        {
            TestCreateTrain();
        }
        
        if (GUILayout.Button("清理所有列车"))
        {
            ClearAllTrains();
        }
        
        GUILayout.Label("按T键快速测试");
        GUILayout.Label("按C键清理列车");
        
        if (TrackManager.Instance != null)
        {
            GUILayout.Label($"轨道列车: {TrackManager.Instance.GetTrackTrainCount()}");
            GUILayout.Label($"全局列车: {TrackManager.Instance.GetGlobalTrainCount()}");
        }
        
        GUILayout.EndArea();
    }
}


