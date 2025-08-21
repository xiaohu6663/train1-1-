using UnityEngine;

public class WheelAnimationFixer : MonoBehaviour
{
    [Header("车轮动画修复设置")]
    public bool autoFixOnStart = true;
    public bool enableWheelRotation = true;
    public float wheelRotationSpeed = 1f;
    
    [Header("层级路径设置")]
    public string trainPath = "Train";
    public string wheelParentPath = "Wheel";
    
    [Header("调试设置")]
    public bool showDetailedLogs = true;
    public bool searchForWheelsRecursively = true;
    
    private Transform wheelParent;
    private Animator[] wheelAnimators;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            Invoke("FixWheelAnimation", 0.1f);
        }
    }
    
    [ContextMenu("修复车轮动画")]
    public void FixWheelAnimation()
    {
        Debug.Log("=== 开始修复车轮动画 ===");
        
        // 查找Wheel父对象
        FindWheelParent();
        
        if (wheelParent != null)
        {
            // 详细分析Wheel对象的结构
            AnalyzeWheelStructure();
            
            // 获取所有车轮的Animator组件
            GetWheelAnimators();
            
            // 修复每个车轮的动画
            FixIndividualWheelAnimations();
            
            Debug.Log("=== 车轮动画修复完成 ===");
        }
        else
        {
            Debug.LogError("未找到Wheel父对象，请检查层级结构");
        }
    }
    
    void FindWheelParent()
    {
        if (showDetailedLogs)
        {
            Debug.Log("--- 查找Wheel父对象 ---");
        }
        
        // 尝试多种路径查找Wheel对象
        Transform train = transform.Find(trainPath);
        if (train != null)
        {
            if (showDetailedLogs)
            {
                Debug.Log($"找到Train对象: {train.name}");
            }
            
            wheelParent = train.Find(wheelParentPath);
            if (wheelParent == null)
            {
                // 尝试其他可能的名称
                wheelParent = train.Find("wheel");
                if (wheelParent == null)
                {
                    wheelParent = train.Find("Wheels");
                    if (wheelParent == null)
                    {
                        wheelParent = train.Find("wheels");
                        if (wheelParent == null)
                        {
                            wheelParent = train.Find("WheelGroup");
                            if (wheelParent == null)
                            {
                                wheelParent = train.Find("wheelGroup");
                            }
                        }
                    }
                }
            }
        }
        
        if (wheelParent != null)
        {
            Debug.Log($"找到Wheel父对象: {wheelParent.name}，子对象数量: {wheelParent.childCount}");
        }
        else
        {
            Debug.LogError("未找到Wheel父对象！");
        }
    }
    
    void AnalyzeWheelStructure()
    {
        if (wheelParent == null) return;
        
        Debug.Log("--- 分析Wheel对象结构 ---");
        Debug.Log($"Wheel对象名称: {wheelParent.name}");
        Debug.Log($"Wheel对象子对象数量: {wheelParent.childCount}");
        
        // 打印Wheel对象的所有子对象
        for (int i = 0; i < wheelParent.childCount; i++)
        {
            Transform child = wheelParent.GetChild(i);
            Debug.Log($"  子对象 {i}: {child.name}");
            
            // 检查子对象是否有Animator组件
            Animator childAnimator = child.GetComponent<Animator>();
            if (childAnimator != null)
            {
                Debug.Log($"    ✓ 有Animator组件");
            }
            else
            {
                Debug.LogWarning($"    ✗ 没有Animator组件");
            }
        }
        
        // 如果Wheel对象下没有子对象，尝试查找其他可能的车轮对象
        if (wheelParent.childCount == 0)
        {
            Debug.LogWarning("Wheel对象下没有子对象！");
            SearchForWheelsInTrain();
        }
    }
    
    void SearchForWheelsInTrain()
    {
        Debug.Log("--- 在Train对象中搜索车轮 ---");
        
        Transform train = transform.Find(trainPath);
        if (train == null) return;
        
        // 递归搜索所有子对象，查找包含"wheel"的对象
        SearchForWheelsRecursively(train, 0);
    }
    
    void SearchForWheelsRecursively(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            string childName = child.name.ToLower();
            
            Debug.Log($"{indent}检查: {child.name}");
            
            // 检查对象名称是否包含"wheel"
            if (childName.Contains("wheel") || childName.Contains("轮") || childName.Contains("wheel"))
            {
                Debug.Log($"{indent}✓ 找到可能的车轮对象: {child.name}");
                
                // 检查是否有Animator组件
                Animator childAnimator = child.GetComponent<Animator>();
                if (childAnimator != null)
                {
                    Debug.Log($"{indent}  ✓ 有Animator组件");
                }
                else
                {
                    Debug.LogWarning($"{indent}  ✗ 没有Animator组件");
                }
                
                // 如果这个对象有子对象，也检查子对象
                if (child.childCount > 0)
                {
                    Debug.Log($"{indent}  子对象数量: {child.childCount}");
                    for (int j = 0; j < child.childCount; j++)
                    {
                        Transform grandChild = child.GetChild(j);
                        Animator grandChildAnimator = grandChild.GetComponent<Animator>();
                        Debug.Log($"{indent}    - {grandChild.name} {(grandChildAnimator != null ? "✓" : "✗")}");
                    }
                }
            }
            
            // 递归搜索子对象
            if (searchForWheelsRecursively && child.childCount > 0)
            {
                SearchForWheelsRecursively(child, depth + 1);
            }
        }
    }
    
    void GetWheelAnimators()
    {
        if (wheelParent == null) return;
        
        Debug.Log("--- 获取车轮Animator组件 ---");
        
        // 如果Wheel对象下没有子对象，尝试使用Wheel对象本身
        if (wheelParent.childCount == 0)
        {
            Debug.Log("Wheel对象下没有子对象，尝试使用Wheel对象本身");
            
            Animator wheelAnimator = wheelParent.GetComponent<Animator>();
            if (wheelAnimator != null)
            {
                wheelAnimators = new Animator[] { wheelAnimator };
                Debug.Log($"使用Wheel对象本身的Animator: {wheelParent.name}");
            }
            else
            {
                Debug.LogWarning("Wheel对象本身也没有Animator组件");
                wheelAnimators = new Animator[0];
            }
        }
        else
        {
            wheelAnimators = new Animator[wheelParent.childCount];
            
            for (int i = 0; i < wheelParent.childCount; i++)
            {
                Transform wheelChild = wheelParent.GetChild(i);
                wheelAnimators[i] = wheelChild.GetComponent<Animator>();
                
                if (wheelAnimators[i] == null)
                {
                    Debug.LogWarning($"车轮对象 {wheelChild.name} 没有Animator组件");
                }
                else
                {
                    Debug.Log($"找到车轮Animator: {wheelChild.name}");
                }
            }
        }
    }
    
    void FixIndividualWheelAnimations()
    {
        if (wheelAnimators == null || wheelAnimators.Length == 0)
        {
            Debug.LogWarning("没有找到任何车轮Animator组件");
            return;
        }
        
        Debug.Log($"--- 修复 {wheelAnimators.Length} 个车轮动画 ---");
        
        foreach (Animator wheelAnimator in wheelAnimators)
        {
            if (wheelAnimator != null)
            {
                // 确保Animator组件启用
                wheelAnimator.enabled = true;
                
                // 设置动画速度
                wheelAnimator.speed = wheelRotationSpeed;
                
                // 强制播放K01动画
                wheelAnimator.Play("K01", 0, 0);
                
                // 确保动画循环播放
                wheelAnimator.SetBool("Loop", true);
                
                Debug.Log($"已修复车轮动画: {wheelAnimator.gameObject.name}");
            }
        }
    }
    
    void Update()
    {
        if (enableWheelRotation && wheelAnimators != null)
        {
            // 持续确保车轮动画正在播放
            foreach (Animator wheelAnimator in wheelAnimators)
            {
                if (wheelAnimator != null && wheelAnimator.enabled)
                {
                    // 检查当前动画状态
                    AnimatorStateInfo stateInfo = wheelAnimator.GetCurrentAnimatorStateInfo(0);
                    
                    // 如果动画没有播放或播放的不是K01，重新播放
                    if (!stateInfo.IsName("K01") || stateInfo.normalizedTime >= 1.0f)
                    {
                        wheelAnimator.Play("K01", 0, 0);
                    }
                }
            }
        }
    }
    
    // 公共方法：手动控制车轮动画
    [ContextMenu("启用车轮旋转")]
    public void EnableWheelRotation()
    {
        enableWheelRotation = true;
        FixWheelAnimation();
    }
    
    [ContextMenu("停用车轮旋转")]
    public void DisableWheelRotation()
    {
        enableWheelRotation = false;
        
        if (wheelAnimators != null)
        {
            foreach (Animator wheelAnimator in wheelAnimators)
            {
                if (wheelAnimator != null)
                {
                    wheelAnimator.speed = 0;
                }
            }
        }
    }
    
    [ContextMenu("设置车轮旋转速度")]
    public void SetWheelRotationSpeed()
    {
        if (wheelAnimators != null)
        {
            foreach (Animator wheelAnimator in wheelAnimators)
            {
                if (wheelAnimator != null)
                {
                    wheelAnimator.speed = wheelRotationSpeed;
                }
            }
        }
    }
    
    // 同步车轮动画与主动画
    public void SyncWithMainAnimation(Animator mainAnimator, int mainLayerIndex)
    {
        if (mainAnimator != null && wheelAnimators != null)
        {
            AnimatorStateInfo mainState = mainAnimator.GetCurrentAnimatorStateInfo(mainLayerIndex);
            float normalizedTime = mainState.normalizedTime % 1.0f;
            
            foreach (Animator wheelAnimator in wheelAnimators)
            {
                if (wheelAnimator != null)
                {
                    wheelAnimator.Play("K01", 0, normalizedTime);
                }
            }
        }
    }
    
    // 重置所有车轮动画
    [ContextMenu("重置车轮动画")]
    public void ResetWheelAnimations()
    {
        if (wheelAnimators != null)
        {
            foreach (Animator wheelAnimator in wheelAnimators)
            {
                if (wheelAnimator != null)
                {
                    wheelAnimator.Rebind();
                    wheelAnimator.Update(0f);
                }
            }
        }
    }
    
    // 手动指定车轮对象
    [ContextMenu("手动指定车轮对象")]
    public void ManualSpecifyWheels()
    {
        Debug.Log("请手动指定车轮对象：");
        Debug.Log("1. 在Inspector中找到WheelAnimationFixer组件");
        Debug.Log("2. 将车轮对象拖拽到wheelParent字段");
        Debug.Log("3. 或者修改trainPath和wheelParentPath参数");
    }
}
