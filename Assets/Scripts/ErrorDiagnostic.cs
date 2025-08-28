using UnityEngine;
using System.Collections.Generic;

public class ErrorDiagnostic : MonoBehaviour
{
    [Header("诊断设置")]
    public bool autoDiagnoseOnStart = true;
    public bool showDetailedInfo = true;
    
    void Start()
    {
        if (autoDiagnoseOnStart)
        {
            DiagnoseErrors();
        }
    }
    
    void Update()
    {
        // 按D键诊断
        if (Input.GetKeyDown(KeyCode.D))
        {
            DiagnoseErrors();
        }
    }
    
    public void DiagnoseErrors()
    {
        Debug.Log("=== 开始错误诊断 ===");
        
        // 检查脚本编译
        CheckScriptCompilation();
        
        // 检查组件引用
        CheckComponentReferences();
        
        // 检查文件完整性
        CheckFileIntegrity();
        
        // 提供解决方案
        ProvideSolutions();
        
        Debug.Log("=== 诊断完成 ===");
    }
    
    private void CheckScriptCompilation()
    {
        Debug.Log("检查脚本编译状态:");
        
        // 检查关键脚本是否存在
        var scripts = new string[] 
        {
            "TrainManager", "TrackManager", "UDPManager", 
            "TrainController", "TrainData", "MainThreadDispatcher"
        };
        
        foreach (var scriptName in scripts)
        {
            var scriptType = System.Type.GetType(scriptName);
            if (scriptType != null)
            {
                Debug.Log($"✓ {scriptName} 脚本正常");
            }
            else
            {
                Debug.LogError($"✗ {scriptName} 脚本缺失或编译失败");
            }
        }
    }
    
    private void CheckComponentReferences()
    {
        Debug.Log("检查组件引用:");
        
        if (TrainManager.Instance != null)
            Debug.Log("✓ TrainManager 实例正常");
        else
            Debug.LogWarning("✗ TrainManager 实例缺失");
            
        if (TrackManager.Instance != null)
            Debug.Log("✓ TrackManager 实例正常");
        else
            Debug.LogWarning("✗ TrackManager 实例缺失");
            
        if (UDPManager.instance != null)
            Debug.Log("✓ UDPManager 实例正常");
        else
            Debug.LogWarning("✗ UDPManager 实例缺失");
            
        if (MainThreadDispatcher.Instance != null)
            Debug.Log("✓ MainThreadDispatcher 实例正常");
        else
            Debug.LogWarning("✗ MainThreadDispatcher 实例缺失");
    }
    
    private void CheckFileIntegrity()
    {
        Debug.Log("检查文件完整性:");
        
        // 检查动画文件
        var animationFiles = new string[]
        {
            "Assets/Animations/Track1.anim",
            "Assets/Animations/Track2.anim", 
            "Assets/Animations/Track3.anim",
            "Assets/Animations/SimpleTrainController.controller"
        };
        
        foreach (var file in animationFiles)
        {
            if (System.IO.File.Exists(file))
            {
                Debug.Log($"✓ {file} 存在");
            }
            else
            {
                Debug.LogWarning($"✗ {file} 缺失");
            }
        }
    }
    
    private void ProvideSolutions()
    {
        Debug.Log("=== 解决方案建议 ===");
        Debug.Log("1. 如果脚本编译失败:");
        Debug.Log("   - 删除Library文件夹");
        Debug.Log("   - 重新打开Unity");
        Debug.Log("   - 等待重新编译");
        
        Debug.Log("2. 如果动画控制器损坏:");
        Debug.Log("   - 使用SimpleTrainController.controller");
        Debug.Log("   - 在Unity编辑器中重新配置");
        
        Debug.Log("3. 如果组件缺失:");
        Debug.Log("   - 使用TempFix脚本自动创建");
        Debug.Log("   - 手动添加组件到场景");
        
        Debug.Log("4. 如果仍有问题:");
        Debug.Log("   - 检查Unity版本兼容性");
        Debug.Log("   - 重新导入项目");
    }
    

    
    private void CleanupCache()
    {
        Debug.Log("清理缓存...");
        // 这里可以添加缓存清理逻辑
    }
    
    private void RecompileScripts()
    {
        Debug.Log("重新编译脚本...");
        // 这里可以添加重新编译逻辑
    }
}


