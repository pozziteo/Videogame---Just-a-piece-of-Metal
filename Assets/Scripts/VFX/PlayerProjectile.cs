using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float Damage {
        get {
            return m_Damage;
        }
        set {
            m_Damage = value;
        }
    }
    public float lifeTimer = 2.0f;
    float m_ActualTimer;
    Rigidbody2D m_Rigidbody;
    float m_Damage;
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_ActualTimer = lifeTimer;
    }

    // Update is called once per frame
    void Update()
    {
        m_ActualTimer -= Time.deltaTime;
        if (m_ActualTimer < 0)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 direction, float force)
    {
        m_Rigidbody.AddForce(direction * force);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.ChangeHealth(-m_Damage);
        }
        Destroy(gameObject);
    }
}
