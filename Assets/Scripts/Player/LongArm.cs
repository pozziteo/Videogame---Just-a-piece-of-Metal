using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongArm : MonoBehaviour
{
    [SerializeField] EnemyController m_CaughtEnemy;        //Enemy to which the arm collided when launched
    BoxCollider2D m_BoxCollider;


    void Start()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
    }
    
    // Update is called once per frame
    void Update()
    {
        if (m_CaughtEnemy != null)
        {
            if (gameObject.activeSelf)
            {
                m_CaughtEnemy.FollowArm(m_BoxCollider);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();

        if (enemy != null)
        {
            m_CaughtEnemy = enemy;
            m_CaughtEnemy.SetCaught(true);
        }
    }

    void OnDisable()
    {
        if (m_CaughtEnemy != null)
        {
            m_CaughtEnemy.SetCaught(false);
            m_CaughtEnemy = null;
        }
    }

}
