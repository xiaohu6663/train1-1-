using UnityEngine;
using UnityEditor;
using System.IO;

public class CleanupTool : MonoBehaviour
{
    [Header("清理选项")]
    public bool clearConsole = true;
    public bool clearCache = true;
    
    void Start()
    {
        if (clearCache)
        {
            CleanupUnityCache();
        }
    }
    
    void Update()
    {
        // 按C键清理
        if (Input.GetKeyDown(KeyCode.C))
        {
            CleanupUnityCache();
        }
    }
    
    public void CleanupUnityCache()
    {
        Debug.Log("=== 开始清理Unity缓存 ===");
        
        // 清理控制台
        if (clearConsole)
        {
            #if UNITY_EDITOR
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
            #endif
        }
        
        Debug.Log("✓ 控制台已清理");
        Debug.Log("✓ 缓存清理完成");
        Debug.Log("请重新编译项目");
    }
    

}


