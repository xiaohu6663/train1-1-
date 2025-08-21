using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_Train : MonoBehaviour
{
 
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // 确保两层都启用
        if (animator.layerCount > 0) animator.SetLayerWeight(0, 1);
        if (animator.layerCount > 1) animator.SetLayerWeight(1, 1);

        // 同步播放两层动画
        animator.Play("K_Train", 0, 0);
        animator.Play("K_Wheel", 1, 0);
    }

    void Update()
    {
        // 同步两层动画的进度
        if (animator.layerCount > 1)
        {
            AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play("K_Wheel", 1, baseState.normalizedTime);
        }
    }
}
