using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    public float shootCooldown;     //waiting time until next attack
    public GameObject projectilePrefab;
    [SerializeField] float m_ShootTimer;             //countdown to be able to attack again
    Transform target;

    // Update is called once per frame
    void Update()
    {
        if (m_Caught)
        {
            return;
        }

        UpdateTimers();

        if (m_PlayerInRange)
        {
            EnemyLogic();
        }
    }

    protected override void UpdateTimers()
    {
        base.UpdateTimers();

        if (m_Cooling)
        {
            m_ShootTimer -= Time.deltaTime * m_StatsModifier;
            if (m_ShootTimer < 0)
            {
                m_Cooling = false;
            }
        }

    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            m_PlayerInRange = true;
            m_LookDirection = (int) Mathf.Sign(other.gameObject.transform.position.x - transform.position.x);
            m_Animator.SetFloat("Look Direction", m_LookDirection);
            target = player.gameObject.transform;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_PlayerInRange = false;
        m_Animator.SetBool("Shoot", false);
    }

    void EnemyLogic()
    {
        if (m_Cooling)
        {
            m_Animator.SetBool("Shoot", false);
        }
        else
        {
            Shoot();
        }
    }

    void Shoot()
    {
        m_Cooling = true;
        m_ShootTimer = shootCooldown;
        m_Animator.SetBool("Shoot", true);
        Vector2 playerPos = target.position;
        Vector2 enemyPos = m_RigidBody.transform.position;
        Vector2 direction = playerPos - enemyPos;
        direction.Normalize();

        float angle = Mathf.Acos((playerPos.x-enemyPos.x)/(playerPos-enemyPos).magnitude) * Mathf.Rad2Deg;
        if (enemyPos.y > playerPos.y)
        {
            angle = -angle;
        }

        GameObject projectileObject = Instantiate(projectilePrefab, enemyPos + Vector2.up * 0.25f + 
                                    Vector2.right * m_LookDirection * 0.95f, 
                                    Quaternion.AngleAxis(angle, Vector3.forward));

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Damage = enemyDamage;
        projectile.Launch(direction, 500);
    }
}

