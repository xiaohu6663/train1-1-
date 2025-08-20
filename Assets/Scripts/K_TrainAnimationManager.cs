using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class K_TrainAnimationManager : MonoBehaviour
{
  
    private Animator animator;
    
    [Header("动画层设置")]
    public int baseLayerIndex = 0;
    public int wheelLayerIndex = 1;
    
    [Header("动画状态名称")]
    public string trainMoveState = "K_Train";
    public string wheelRotateState = "K01";
    
    void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("在对象 " + gameObject.name + " 上找不到Animator组件");
            return;
        }
        
        // 确保动画层权重正确设置
        InitializeLayers();
        
        // 启动动画
        StartAnimations();
    }
    
    void InitializeLayers()
    {
        // 设置Base Layer权重
        if (animator.layerCount > baseLayerIndex)
        {
            animator.SetLayerWeight(baseLayerIndex, 1);
        }
        
        // 设置Wheel Layer权重
        if (animator.layerCount > wheelLayerIndex)
        {
            animator.SetLayerWeight(wheelLayerIndex, 1);
        }
        else
        {
            Debug.LogWarning("Animator没有足够的层，当前层数: " + animator.layerCount);
        }
    }
    
    void StartAnimations()
    {
        // 播放Base Layer动画
        animator.Play(trainMoveState, baseLayerIndex, 0);
        
        // 播放Wheel Layer动画
        if (animator.layerCount > wheelLayerIndex)
        {
            animator.Play(wheelRotateState, wheelLayerIndex, 0);
        }
    }
    
    void Update()
    {
        // 确保层权重始终保持正确
        if (animator != null && animator.layerCount > wheelLayerIndex)
        {
            if (animator.GetLayerWeight(wheelLayerIndex) != 1)
            {
                animator.SetLayerWeight(wheelLayerIndex, 1);
            }
        }
        
        // 同步两个层的动画进度
        SyncAnimationProgress();
    }
    
    void SyncAnimationProgress()
    {
        // 获取Base Layer的当前状态信息
        AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(baseLayerIndex);
        
        // 设置Wheel Layer的播放进度与Base Layer同步
        if (animator.layerCount > wheelLayerIndex)
        {
            animator.Play(wheelRotateState, wheelLayerIndex, baseState.normalizedTime % 1);
        }
    }
    
    // 公共方法用于控制动画
    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }
    
    public void PauseAnimation()
    {
        animator.speed = 0;
    }
    
    public void ResumeAnimation()
    {
        animator.speed = 1;
    }
}
