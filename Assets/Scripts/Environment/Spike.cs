using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    public float animationDelay;
    Animator m_Animator;

    void Awake()
    {
        m_Animator = GetComponent<Animator>();
        StartCoroutine(DelayedAnimation());
        m_Animator.enabled = false;
    }

    IEnumerator DelayedAnimation()
    {
        yield return new WaitForSeconds(animationDelay);
        m_Animator.enabled = true;
    }
}
