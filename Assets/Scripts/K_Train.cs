using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class K_Train : MonoBehaviour
{
 
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // ȷ�����㶼����
        if (animator.layerCount > 0) animator.SetLayerWeight(0, 1);
        if (animator.layerCount > 1) animator.SetLayerWeight(1, 1);

        // ͬ���������㶯��
        animator.Play("K_Train", 0, 0);
        animator.Play("K_Wheel", 1, 0);
    }

    void Update()
    {
        // ͬ�����㶯���Ľ���
        if (animator.layerCount > 1)
        {
            AnimatorStateInfo baseState = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play("K_Wheel", 1, baseState.normalizedTime);
        }
    }
}
