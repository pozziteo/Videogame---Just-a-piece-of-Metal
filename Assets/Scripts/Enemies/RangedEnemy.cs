using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : BaseEnemy
{
    public float shootCooldown;     //waiting time until next attack
    public GameObject projectilePrefab;
    public float projectileForce;
    [SerializeField] float m_ShootTimer;             //countdown to be able to attack again
    Transform target;
    float m_LaunchProjectileAngle;
    Vector2 m_LaunchProjectileDirection;


    // Update is called once per frame
    void Update()
    {
        if (m_Caught)
        {
            m_Animator.SetBool("Can Walk", false);
            return;
        }

        UpdateTimers();

        if (m_PlayerInRange)
        {
            EnemyLogic();
        }
    }

    void FixedUpdate()
    {
        if (m_Caught || m_Cooling)
        {
            return;
        }

        if (!m_PlayerInRange)
        {
            m_Animator.SetBool("Can Walk", true);
            m_RigidBody.velocity = (Vector2.right * moveSpeed * m_LookDirection  + Vector2.up * m_RigidBody.velocity.y) * m_StatsModifier;
        }
        else
        {
            m_RigidBody.velocity = Vector2.zero;
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
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            m_PlayerInRange = false;
        }     
    }

    void EnemyLogic()
    {
        if (m_Cooling)
        {
            return;
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
        m_Animator.SetTrigger("Shoot");
        Vector2 playerPos = target.position;
        Vector2 enemyPos = m_RigidBody.transform.position;
        Vector2 direction = playerPos - enemyPos;
        direction.Normalize();

        float angle = Mathf.Acos((playerPos.x-enemyPos.x)/(playerPos-enemyPos).magnitude) * Mathf.Rad2Deg;
        if (enemyPos.y > playerPos.y)
        {
            angle = -angle;
        }

        m_LaunchProjectileAngle = angle;
        m_LaunchProjectileDirection = direction;
    }

    void CreateProjectile()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, m_RigidBody.transform.position + Vector3.up * 0.25f + 
                                    Vector3.right * m_LookDirection * 0.95f, 
                                    Quaternion.AngleAxis(m_LaunchProjectileAngle, Vector3.forward));

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Damage = enemyDamage;
        projectile.Launch(m_LaunchProjectileDirection, projectileForce);
    }
}

