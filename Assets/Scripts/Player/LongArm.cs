using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongArm : MonoBehaviour
{
    public float attractionSpeed = 8.0f;
    PlayerController m_Player;
    [SerializeField] BaseEnemy m_CaughtEnemy;        //Enemy to which the arm collided when launched
    BoxCollider2D m_BoxCollider;

    void Awake()
    {
        m_Player = transform.parent.parent.GetComponent<PlayerController>();
        m_BoxCollider = GetComponent<BoxCollider2D>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (m_CaughtEnemy != null)
        {
            if (gameObject.activeSelf)
            {
                m_CaughtEnemy.FollowArm(m_BoxCollider, attractionSpeed);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        BaseEnemy enemy = collision.gameObject.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            m_CaughtEnemy = enemy;
            m_CaughtEnemy.SetCaught(true);
            m_Player.SetKinematic(true);
        }
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Foothold")
        {
            m_Player.SetGrabbed(true);
        }
    }

    void OnDisable()
    {
        if (m_CaughtEnemy != null)
        {
            m_CaughtEnemy.SetCaught(false);
            m_Player.SetKinematic(false);
            m_CaughtEnemy = null;
        }
        m_Player.SetGrabbed(false);
    }

}
