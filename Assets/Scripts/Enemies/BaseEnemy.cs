using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{

    public float moveSpeed;         //speed of the character
    public float moveTimer;         //timer of moving in the same direction
    public float startHealth;
    public float enemyDamage;
    [SerializeField] float m_Health;
    [SerializeField] protected float m_MovedTime;              //elapsed time moving in a direction
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


    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_MovedTime = moveTimer;
        m_Health = startHealth;
        m_StatsModifier = 1f;
        m_Animator.SetFloat("Look Direction", m_LookDirection);
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

    protected virtual void UpdateTimers()
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
