using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{

    public float moveSpeed;         //speed of the character
    public float startHealth;
    public float enemyDamage;
    public ParticleSystem hurtEffect;
    public Transform rightBoundary;
    public Transform leftBoundary;
    public float Health {
        get
        {
            return m_Health;
        }
    }
    public float respawnTime;
    AudioSource m_AudioSource;
    [SerializeField] float m_Health;
    protected float m_PoisonedTime;
    protected float m_PoisonTotalTime;
    [SerializeField] protected float m_StatsModifier;
    [SerializeField] protected bool m_Poisoned;
    protected Animator m_Animator;
    protected Rigidbody2D m_RigidBody;
    [SerializeField] protected bool m_PlayerInRange;           //player is in range to be attacked
    [SerializeField] protected bool m_Caught;              //enemy has been caught by the player's long arm
    [SerializeField] protected bool m_Cooling;                 //cooling down from an attack
    [SerializeField] protected int m_LookDirection = 1;        //direction of movement
    protected Transform target;
    protected bool m_EnemyDead;
    protected bool m_IsFixedEnemy;
    Transform m_InitialPosition;


    protected virtual void Awake()
    {
        m_InitialPosition = gameObject.transform;
        m_Animator = GetComponent<Animator>();
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_AudioSource = GetComponent<AudioSource>();
        m_Health = startHealth;
        m_StatsModifier = 1f;
        m_Animator.SetFloat("Look Direction", m_LookDirection);

        if (leftBoundary == null || rightBoundary == null)
        {
            m_IsFixedEnemy = true;
        }

        SelectNextPatrolPoint();
    }

    protected virtual void Update()
    {
        if (!m_IsFixedEnemy)
        {
            if (!IsBetweenBoundaries() && !m_PlayerInRange && !m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Melee"))
            {
                SelectNextPatrolPoint();
            }
        }
    }

    protected virtual void UpdateTimers()
    {
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

    void SelectNextPatrolPoint()
    {
        if (!m_IsFixedEnemy)
        {
            float distanceToLeft = Vector2.Distance(transform.position, leftBoundary.position);
            float distanceToRight = Vector2.Distance(transform.position, rightBoundary.position);

            if (distanceToLeft > distanceToRight)
            {
                target = leftBoundary;
            }
            else
            {
                target = rightBoundary;
            }

            Flip();
        }
    }

    void Flip()
    {
        if (transform.position.x > target.position.x)
        {
            m_LookDirection = -1;
        }
        else
        {
            m_LookDirection = 1;
        }

        m_Animator.SetFloat("Look Direction", m_LookDirection);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "EnemyPatrol" || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Flip();
        }
    }

    public void ChangeHealth(float amount)
    {
        if (!m_EnemyDead)
        {
            m_Health = Mathf.Clamp(m_Health + amount, 0, startHealth);
            Instantiate(hurtEffect, transform.position, Quaternion.identity);
            if (m_Health == 0)
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                m_Animator.enabled = false;
                gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                foreach (Transform child in transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
                }

                m_EnemyDead = true;
                EnemySpawnerManager.Instance.AddDeadEnemy(this, respawnTime);
            }
        }
    }

    public void Respawn()
    {
        gameObject.transform.position = m_InitialPosition.position;
        m_LookDirection = 1;
        m_Animator.enabled = true;
        m_Animator.SetFloat("Look Direction", m_LookDirection);
        m_Health = startHealth;
        m_EnemyDead = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        foreach (Transform child in transform)
        {   
            child.gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        SelectNextPatrolPoint();
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

    bool IsBetweenBoundaries()
    {
        return transform.position.x > leftBoundary.position.x && transform.position.x < rightBoundary.position.x;
    }

    protected void PlaySound(AudioClip clip)
    {
        m_AudioSource.PlayOneShot(clip);
    }
}
