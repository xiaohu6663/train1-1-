using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAnimationTester : MonoBehaviour
{
    [Header("测试设置")]
    public bool autoCreateTrains = false;
    public float createInterval = 5f;
    public int maxTestTrains = 5;
    
    [Header("手动控制")]
    public KeyCode createTrainKey = KeyCode.Space;
    public KeyCode pauseAllKey = KeyCode.P;
    public KeyCode resumeAllKey = KeyCode.R;
    
    private float lastCreateTime;
    private List<GameObject> testTrains = new List<GameObject>();
    
    void Start()
    {
        // 确保TrackManager存在
        if (TrackManager.Instance == null)
        {
            Debug.LogError("TrackManager未找到！请确保场景中有TrackManager组件。");
        }
        
        // 确保UDPManager存在
        if (UDPManager.instance == null)
        {
            Debug.LogWarning("UDPManager未找到，将无法通过UDP创建列车。");
        }
    }
    
    void Update()
    {
        // 自动创建列车
        if (autoCreateTrains && Time.time - lastCreateTime > createInterval)
        {
            if (testTrains.Count < maxTestTrains)
            {
                CreateTestTrain();
                lastCreateTime = Time.time;
            }
        }
        
        // 手动控制
        if (Input.GetKeyDown(createTrainKey))
        {
            CreateTestTrain();
        }
        
        if (Input.GetKeyDown(pauseAllKey))
        {
            PauseAllTrains();
        }
        
        if (Input.GetKeyDown(resumeAllKey))
        {
            ResumeAllTrains();
        }
        
        // 清理已销毁的列车
        testTrains.RemoveAll(train => train == null);
    }
    
    public void CreateTestTrain()
    {
        if (TrackManager.Instance == null)
        {
            Debug.LogError("TrackManager未找到，无法创建列车");
            return;
        }
        
        // 创建测试列车配置
        string testConfig = "{model1;D1:R,D2:G,D3:B}";
        GameObject newTrain = TrackManager.Instance.CreateTrain(testConfig);
        
        if (newTrain != null)
        {
            testTrains.Add(newTrain);
            Debug.Log($"创建测试列车成功，当前测试列车数: {testTrains.Count}");
        }
        else
        {
            Debug.LogWarning("创建测试列车失败，可能是轨道已满或配置错误");
        }
    }
    
    public void PauseAllTrains()
    {
        foreach (GameObject train in testTrains)
        {
            if (train != null)
            {
                TrainController controller = train.GetComponent<TrainController>();
                if (controller != null)
                {
                    controller.PauseAnimation();
                }
            }
        }
        Debug.Log("暂停所有测试列车");
    }
    
    public void ResumeAllTrains()
    {
        foreach (GameObject train in testTrains)
        {
            if (train != null)
            {
                TrainController controller = train.GetComponent<TrainController>();
                if (controller != null)
                {
                    controller.ResumeAnimation();
                }
            }
        }
        Debug.Log("恢复所有测试列车");
    }
    
    public void ClearAllTestTrains()
    {
        foreach (GameObject train in testTrains)
        {
            if (train != null)
            {
                Destroy(train);
            }
        }
        testTrains.Clear();
        Debug.Log("清除所有测试列车");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 220, 300, 200));
        GUILayout.Label("列车动画测试控制", GUI.skin.box);
        
        if (GUILayout.Button("创建测试列车"))
        {
            CreateTestTrain();
        }
        
        if (GUILayout.Button("暂停所有列车"))
        {
            PauseAllTrains();
        }
        
        if (GUILayout.Button("恢复所有列车"))
        {
            ResumeAllTrains();
        }
        
        if (GUILayout.Button("清除所有测试列车"))
        {
            ClearAllTestTrains();
        }
        
        GUILayout.Label($"测试列车数量: {testTrains.Count}");
        GUILayout.Label($"自动创建: {(autoCreateTrains ? "开启" : "关闭")}");
        GUILayout.Label($"创建间隔: {createInterval}秒");
        
        autoCreateTrains = GUILayout.Toggle(autoCreateTrains, "自动创建列车");
        createInterval = GUILayout.HorizontalSlider(createInterval, 1f, 10f);
        GUILayout.Label($"间隔: {createInterval:F1}秒");
        
        GUILayout.EndArea();
    }
    
    // 通过UDP模拟创建列车
    public void SimulateUDPTrainCreation(string config)
    {
        if (UDPManager.instance != null)
        {
            // 模拟UDP消息
            MainThreadDispatcher.RunOnMainThread(() => {
                try
                {
                    if (TrackManager.Instance != null)
                    {
                        TrackManager.Instance.CreateTrain(config);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"模拟UDP创建列车失败: {ex.Message}");
                }
            });
        }
    }
}
