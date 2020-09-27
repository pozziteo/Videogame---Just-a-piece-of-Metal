using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float shootCooldown;     //waiting time until next attack
    public float moveSpeed;         //speed of the character
    public float moveTimer;         //timer of moving in the same direction
    public float startHealth;
    public GameObject projectilePrefab;
    public float enemyDamage;
    [SerializeField] float m_Health;
    float m_ShootTimer;             //countdown to be able to attack again
    float m_MovedTime;              //elapsed time moving in a direction
    float m_PoisonedTime;
    float m_PoisonTotalTime;
    [SerializeField] float m_StatsModifier;
    [SerializeField] bool m_Poisoned;
    Animator m_Animator;
    Rigidbody2D m_RigidBody;
    [SerializeField] bool m_PlayerInRange;           //player is in range to be attacked
    [SerializeField] bool m_Caught;              //enemy has been caught by the player's long arm
    [SerializeField] bool m_Cooling;                 //cooling down from an attack
    int m_LookDirection = 1;        //direction of movement

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_MovedTime = moveTimer;
        m_Health = startHealth;
        m_Animator.SetFloat("Look Direction", m_LookDirection);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Caught)
        {
            return;
        }

        UpdateTimers(); 
        
    }

    void FixedUpdate()
    {
        if (m_Caught)
        {
            return;
        }

        if (!m_PlayerInRange)
        {
            m_RigidBody.velocity = (Vector2.right * moveSpeed * m_LookDirection  + Vector2.up * m_RigidBody.velocity.y) * m_StatsModifier;
        }
    }

    void UpdateTimers()
    {
        if (!m_PlayerInRange)
        {
            m_MovedTime -= Time.deltaTime * m_StatsModifier;
            if (m_MovedTime < 0 && !m_PlayerInRange)
            {
                m_LookDirection = -m_LookDirection;
                m_Animator.SetFloat("Look Direction", m_LookDirection);
                m_MovedTime = moveTimer;
            }
        }

        if (m_Cooling)
        {
            m_ShootTimer -= Time.deltaTime * m_StatsModifier;
            if (m_ShootTimer < 0)
            {
                m_Cooling = false;
            }
        }

        if (m_Poisoned)
        {
            m_PoisonedTime -= Time.deltaTime;
            if (m_PoisonedTime < 0)
            {
                m_Poisoned = false;
                m_StatsModifier = 1f;
                m_PoisonTotalTime = 0f;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            m_PlayerInRange = true;
            float dir = other.gameObject.transform.position.x - transform.position.x;
            if (dir > 0)
            {
                m_LookDirection = 1;
            }
            else 
            {
                m_LookDirection = -1;
            }
            m_Animator.SetFloat("Look Direction", m_LookDirection);
            EnemyLogic(player);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        m_PlayerInRange = false;
        m_Animator.SetBool("Shoot", false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            m_RigidBody.velocity = Vector2.zero;
            m_RigidBody.isKinematic = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            m_RigidBody.isKinematic = false;
        }
    } 

    void EnemyLogic(PlayerController player)
    {
        if (m_Cooling)
        {
            m_Animator.SetBool("Shoot", false);
        }
        else
        {
            Shoot(player);
        }
    }

    void Shoot(PlayerController player)
    {
        m_Cooling = true;
        m_ShootTimer = shootCooldown;
        m_Animator.SetBool("Shoot", true);
        Vector2 playerPos = player.gameObject.transform.position;
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

    public void ChangeHealth(float amount)
    {
        m_Health = Mathf.Clamp(m_Health + amount, 0, startHealth);
        if (m_Health == 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetCaught(bool value)
    {
        m_Caught = value;
    }

    public void Poison(float statsModifier, float poisoningTime)
    {
        m_Poisoned = true;
        m_StatsModifier = statsModifier;
        m_PoisonTotalTime = poisoningTime;
        m_PoisonedTime = m_PoisonTotalTime;
    }

    public void FollowArm(BoxCollider2D boxCollider, float attractionSpeed)
    {
        if (m_Caught)
        {
            Vector2 followPos = boxCollider.ClosestPoint(m_RigidBody.position);
            Vector2 direction = followPos - (Vector2) gameObject.transform.position;
            m_RigidBody.transform.Translate(direction * Time.fixedDeltaTime * attractionSpeed);
        }
    }
}

