using UnityEngine;
using UnityEngine.UI;

public class GraffitiTester : MonoBehaviour
{
    [Header("测试设置")]
    public string testFileName = "img_20250725182636476.png";
    
    [Header("UI组件")]
    public Button testButton;
    public Text statusText;
    
    private void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestGraffiti);
        }
        
        UpdateStatus("准备就绪");
    }
    
    public void TestGraffiti()
    {
        Debug.Log("[GraffitiTester] 开始测试涂鸦系统");
        UpdateStatus("测试中...");
        
        if (GraffitiManager.Instance != null)
        {
            try
            {
                GraffitiManager.Instance.AddGraffitiFromFile(testFileName);
                UpdateStatus("测试完成 - 查看Console日志");
                Debug.Log("[GraffitiTester] 涂鸦测试完成");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"测试失败: {ex.Message}");
                Debug.LogError($"[GraffitiTester] 测试失败: {ex.Message}");
            }
        }
        else
        {
            UpdateStatus("错误: GraffitiManager不可用");
            Debug.LogError("[GraffitiTester] GraffitiManager不可用");
        }
    }
    
    public void TestMultipleFiles()
    {
        Debug.Log("[GraffitiTester] 开始测试多个文件");
        UpdateStatus("测试多个文件...");
        
        string[] testFiles = {
            "img_20250725182614806.png",
            "img_20250725182636476.png",
            "img_20250725182651884.png"
        };
        
        foreach (string fileName in testFiles)
        {
            if (GraffitiManager.Instance != null)
            {
                try
                {
                    GraffitiManager.Instance.AddGraffitiFromFile(fileName);
                    Debug.Log($"[GraffitiTester] 成功测试文件: {fileName}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GraffitiTester] 文件 {fileName} 测试失败: {ex.Message}");
                }
            }
        }
        
        UpdateStatus("多文件测试完成");
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[GraffitiTester] 状态: {message}");
    }
    

    
    private void CheckSystemStatus()
    {
        Debug.Log("[GraffitiTester] 检查系统状态");
        
        // 检查GraffitiManager
        if (GraffitiManager.Instance != null)
        {
            Debug.Log("[GraffitiTester] ✓ GraffitiManager可用");
        }
        else
        {
            Debug.LogError("[GraffitiTester] ✗ GraffitiManager不可用");
        }
        
        // 检查文件是否存在
        string filePath = System.IO.Path.Combine("F:/Graffiti", testFileName);
        if (System.IO.File.Exists(filePath))
        {
            Debug.Log($"[GraffitiTester] ✓ 测试文件存在: {filePath}");
        }
        else
        {
            Debug.LogError($"[GraffitiTester] ✗ 测试文件不存在: {filePath}");
        }
        
        UpdateStatus("系统状态检查完成");
    }
}




