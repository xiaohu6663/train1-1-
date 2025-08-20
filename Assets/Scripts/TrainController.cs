using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainController : MonoBehaviour
{
    [Header("动画控制")]
    public Animator animator;
    
    [Header("轨道设置")]
    public string currentTrack = "Track1";
    
    [Header("动画事件")]
    public bool enableAnimationEvents = true;
    
    private bool isPaused = false;
    private float pausedTime = 0f;
    private bool isInitialized = false;
    
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // 确保动画控制器存在
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("TrainController: Animator没有设置RuntimeAnimatorController");
        }
    }
    
    public void InitializeForTrack(string trackName)
    {
        currentTrack = trackName;
        isInitialized = true;
        
        if (animator != null)
        {
            // 重置所有触发器
            animator.ResetTrigger("StartTrack");
            
            // 开始轨道动画
            animator.SetTrigger("StartTrack");
        }
        
        Debug.Log($"列车初始化到轨道: {trackName}");
    }
    
    public void SwitchToTrack(string newTrack)
    {
        if (currentTrack == newTrack) return;
        
        currentTrack = newTrack;
        
        if (animator != null)
        {
            // 重置所有触发器
            animator.ResetTrigger("StartTrack");
            
            // 触发轨道动画
            animator.SetTrigger("StartTrack");
        }
        
        Debug.Log($"列车开始轨道动画: {newTrack}");
    }
    
    public void PauseAnimation()
    {
        if (isPaused || animator == null) return;
        
        isPaused = true;
        pausedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animator.speed = 0f;
    }
    
    public void ResumeAnimation()
    {
        if (!isPaused || animator == null) return;
        
        isPaused = false;
        animator.speed = 1f;
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, pausedTime);
    }
    
    // 动画事件：当前轨道完成
    public void OnTrackCompleted()
    {
        if (!enableAnimationEvents) return;
        
        Debug.Log($"轨道 {currentTrack} 完成");
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.OnTrainCompletedTrack(gameObject);
        }
    }
    
    // 动画事件：轨道开始
    public void OnTrackStarted()
    {
        if (!enableAnimationEvents) return;
        
        Debug.Log($"开始轨道: {currentTrack}");
    }
    
    // 获取当前动画状态信息
    public AnimatorStateInfo GetCurrentStateInfo()
    {
        if (animator != null)
        {
            return animator.GetCurrentAnimatorStateInfo(0);
        }
        return new AnimatorStateInfo();
    }
    
    // 检查动画是否完成
    public bool IsAnimationComplete()
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.normalizedTime >= 1.0f;
        }
        return false;
    }
    
    void Update()
    {
        // 检查动画是否完成，如果启用了动画事件则自动切换
        if (enableAnimationEvents && isInitialized && IsAnimationComplete())
        {
            OnTrackCompleted();
        }
    }
    
    void OnDestroy()
    {
        if (TrackManager.Instance != null)
        {
            TrackManager.Instance.CleanupTrainLists();
        }
    }
}
