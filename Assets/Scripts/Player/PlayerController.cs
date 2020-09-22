using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float jumpSpeed = 400f;
    public float fallJumpMultiplier;
    float m_MaxHealth = 5;
    public float health {get { return currentHealth; }}
    float currentHealth;
    public float timeInvincible;
    public float blinkingHitTime;
    public float blinkingInterval;
    public float timeDead;
    public float attackCooldown;
    public float shootDamage;
    public GameObject projectilePrefab;
    public ParticleSystem shootEffect;
    Vector2 lookDirection = new Vector2(1,0);
    Vector2 m_PlayerInput;
    Rigidbody2D rigidBody;
    Animator m_Animator;
    bool m_IsDead;
    bool m_ShouldJump;
    bool m_IsGrounded;
    bool m_IsInvincible;
    float m_InvincibleTimer;
    float m_BlinkingTime;
    float m_BlinkingPhase;
    float m_DeathTimer;
    float m_AttackTimer;
    bool m_IsHit;
    bool m_IsCooling;
    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        m_Animator = GetComponent<Animator>();

        currentHealth = m_MaxHealth;
   }
    
    // Update is called once per frame
    void Update () 
    {
        if (m_IsDead)
        {
            m_IsInvincible = true;
            rigidBody.velocity = Vector2.zero;
            m_DeathTimer += Time.deltaTime;
            if (m_DeathTimer > timeDead)
            {
                m_IsDead = false;
                m_IsInvincible = false;
                m_DeathTimer = 0f;
            }
            else 
            {
                return;
            }
        }

        UpdateTimers();

        m_PlayerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        bool isRunning = !Mathf.Approximately(m_PlayerInput.x, 0f);
        m_Animator.SetBool("IsRunning", isRunning);

        if(!Mathf.Approximately(m_PlayerInput.x, 0.0f) || !Mathf.Approximately(m_PlayerInput.y, 0.0f))
        {
            lookDirection.Set(m_PlayerInput.x, m_PlayerInput.y);
            lookDirection.Normalize();
        }

        m_Animator.SetFloat("Look X", lookDirection.x);

        if (m_IsHit)
        {
            SpriteBlinkingEffect();
        }

        if(Input.GetKeyDown(KeyCode.Space) && m_IsGrounded)
        {
            m_ShouldJump = true;
        }

         else if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidBody.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("Environment"));
            if (hit.collider != null)
            {
                SwitchBehaviour swit = hit.collider.GetComponent<SwitchBehaviour>();
                if (swit != null)
                {
                    swit.ActivateSwitch();
                }
                DoorBehaviour door = hit.collider.GetComponent<DoorBehaviour>();
                if (door != null)
                {
                    door.OpenDoor();
                }
            }
        }
        
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Melee();
        }

        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Shoot();
        }
    }

    void FixedUpdate() 
    {
        if (m_IsDead)
        {
            return;
        }
        
        if(m_PlayerInput != Vector2.zero) 
        {
            rigidBody.velocity = new Vector2(moveSpeed * m_PlayerInput.x, rigidBody.velocity.y);
        }
        else
        {
            rigidBody.velocity = Vector2.up * rigidBody.velocity.y;
        }
     
        if(m_ShouldJump && m_IsGrounded) 
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed);
            m_Animator.SetTrigger("Jump");
            m_ShouldJump = false;
        }

        if (rigidBody.velocity.y < 0)
        {
            rigidBody.velocity += Vector2.up * Physics2D.gravity.y * fallJumpMultiplier * Time.fixedDeltaTime;
        }
    }

    void UpdateTimers()
    {
        if (m_IsInvincible)
        {
            m_InvincibleTimer -= Time.deltaTime;
            if (m_InvincibleTimer < 0)
                m_IsInvincible = false;
        }

        if (m_IsCooling)
        {
            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer < 0)
            {
                m_IsCooling = false;
                m_Animator.SetBool("Shoot", false);
                m_Animator.SetBool("Melee", false);
            }
            
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            m_IsGrounded = true;
            m_Animator.SetBool("Is Grounded", true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            m_IsGrounded = false;
            m_Animator.SetBool("Is Grounded", false);        }
    }

    public void ChangeHealth(float amount)
    {
        if (amount < 0)
            {
                if (m_IsInvincible)
                    return;
                
                m_IsInvincible = true;
                m_InvincibleTimer = timeInvincible;
                m_IsHit = true;
            }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, m_MaxHealth);
        HealthBar.instance.SetValue(currentHealth / (float)m_MaxHealth);
        if (currentHealth == 0)
        {
            Die();
        }
    }

    void SpriteBlinkingEffect()
    {
        if (currentHealth == 0)
        {
            return;
        }

        m_BlinkingTime += Time.deltaTime;
        if(m_BlinkingTime >= blinkingHitTime)
        {
            m_IsHit = false;
            m_BlinkingTime = 0.0f;
            this.gameObject.GetComponent<SpriteRenderer> ().enabled = true;
            return;
        }
        
        m_BlinkingPhase += Time.deltaTime;
        if(m_BlinkingPhase >= blinkingInterval)
        {
            m_BlinkingPhase = 0.0f;
            if (this.gameObject.GetComponent<SpriteRenderer>().enabled) 
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            } 
            else 
            {
                this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }

    void Die()
    {
        m_IsDead = true;
        m_Animator.SetTrigger("Die");
        currentHealth = m_MaxHealth;
        HealthBar.instance.SetValue(1f);
    }

    void Melee()
    {
        if (m_IsCooling)
        {
            return;
        }

        m_AttackTimer = attackCooldown;
        m_Animator.SetBool("Melee", true);
        m_IsCooling = true;
    }

    void Shoot()
    {
        if (m_IsCooling)
        {
            return;
        }

        m_AttackTimer = attackCooldown;
        m_Animator.SetBool("Shoot", true);
        m_IsCooling = true;

        Vector2 playerPos = rigidBody.transform.position;
        Vector2 direction = new Vector2(Mathf.RoundToInt(lookDirection.x), Mathf.RoundToInt(lookDirection.y));
        direction.Normalize();

        float angle = Mathf.Acos(direction.x / (float) direction.magnitude) * Mathf.Rad2Deg;

        if (lookDirection.y < 0)
        {
            angle = -angle;
        }

        Instantiate(shootEffect, playerPos + Vector2.up * 0.13f + Vector2.right * lookDirection.x * 0.90f,
                        Quaternion.AngleAxis(angle, Vector3.forward));

        GameObject projectileObject = Instantiate(projectilePrefab, playerPos + Vector2.up * 0.13f + 
                                    Vector2.right * lookDirection.x * 0.95f, 
                                    Quaternion.AngleAxis(angle, Vector3.forward));

        PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
        projectile.Damage = shootDamage;
        projectile.Launch(direction, 500);
    }
}
