using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;        //Horizontal speed of the player
    public float jumpSpeed = 400f;      //Vertical speed when player jumps
    public float fallJumpMultiplier;    //Coefficient of boost to gravity when falling down
    float m_MaxHealth = 5;              //Max health of the player
    public float health {get { return currentHealth; }} 
    float currentHealth;
    public float timeInvincible;        //Time interval in which player is invincible after being hit
    public float blinkingHitTime;       //Blinking animation time after being hit
    public float blinkingInterval;      //Time interval of a single blinking
    public float timeDead;              //Time interval in which player remains dead
    public float attackCooldown;        //Time interval to wait for attacking again
    public float shootDamage;           //Damage to health from shooting
    public float longArmInterval;       //Time interval for long arm animation
    public GameObject projectilePrefab;     //Projectile sprite
    public ParticleSystem shootEffect;      //Particle system when shooting
    Vector2 lookDirection = new Vector2(1,0);       //Look direction of the player
    Vector2 m_PlayerInput;              //Input movement
    Rigidbody2D rigidBody;
    Animator m_Animator;
    [SerializeField] bool m_IsDead;
    [SerializeField] bool m_ShouldJump;
    [SerializeField] bool m_IsGrounded;
    [SerializeField] bool m_IsInvincible;
    bool m_IsHit;
    bool m_IsCooling;
    [SerializeField] bool m_UsingLongArm;
    [SerializeField] bool m_Grabbed;
    float m_InvincibleTimer;
    float m_BlinkingTime;
    float m_BlinkingPhase;
    float m_DeathTimer;
    float m_AttackTimer;
    float m_LongArmTimer;
    
    void Start () {
        rigidBody = GetComponent<Rigidbody2D> ();
        m_Animator = GetComponent<Animator>();
        new PlayerSkills();
        PlayerSkills.instance.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;

        currentHealth = m_MaxHealth;
    }

    //Method called when player unlocks a skill. Change player parameters
    void PlayerSkills_OnSkillUnlocked(object sender, PlayerSkills.OnSkillUnlockedEventArgs e)
    {
        switch (e.skillType)
        {
            case PlayerSkills.SkillType.Propulsors:
                SetJumpSpeed(1.4f * jumpSpeed);
                break;
            
            case PlayerSkills.SkillType.MagneticAccelerators:
                SetMoveSpeed(1.4f * moveSpeed);
                break;
        }
    }
    
    void Update () 
    {
        //If player is dead, lock all input controls and stop the player, unless it is falling for gravity. 
        //When timer elapses, respawn
        if (m_IsDead)
        {
            m_IsInvincible = true;
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.velocity = Vector2.up * rigidBody.velocity;
            }
            else 
            {
                rigidBody.velocity = Vector2.zero;
            }
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

        //Update all timers of the player
        UpdateTimers();

        if (m_IsHit)
        {
            SpriteBlinkingEffect();
        }

        //Lock all input and avoid movement if player is using long arm
        if (m_UsingLongArm)
        {
            return;
        }

        m_PlayerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        bool isRunning = !Mathf.Approximately(m_PlayerInput.x, 0f);
        m_Animator.SetBool("IsRunning", isRunning);

        if(!Mathf.Approximately(m_PlayerInput.x, 0.0f) || !Mathf.Approximately(m_PlayerInput.y, 0.0f))
        {
            lookDirection.Set(m_PlayerInput.x, m_PlayerInput.y);
            lookDirection.Normalize();
        }

        m_Animator.SetFloat("Look X", lookDirection.x);

        if(Input.GetKeyDown(KeyCode.Space) && m_IsGrounded)
        {
            m_ShouldJump = true;
        }

         else if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidBody.position + Vector2.up * 0.2f, lookDirection, 
                                    1.5f, LayerMask.GetMask("Environment"));
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

        else if (Input.GetKeyDown(KeyCode.Q))
        {
            UseLongArm();
        }
    }

    void FixedUpdate() 
    {
        if (m_IsDead)
        {
            return;
        }

        /*if (m_BoostVelocity)
        {
            //rigidBody.isKinematic = false;      
        }*/

        if (!m_UsingLongArm) 
        {
            rigidBody.velocity = new Vector2(moveSpeed * m_PlayerInput.x, rigidBody.velocity.y);
        }

        else 
        {
            if (!m_IsGrounded && !m_Grabbed)
            {
                //rigidBody.velocity += Vector2.up * Physics2D.gravity * Time.fixedDeltaTime;
                rigidBody.velocity = new Vector2(moveSpeed * m_PlayerInput.x * 0.2f, rigidBody.velocity.y * 0.2f);
            }
            else if (m_IsGrounded && !m_Grabbed)
            {
                rigidBody.velocity = Vector2.zero;
            }
            else 
            {
                rigidBody.velocity = Vector2.right * lookDirection * 7f + Vector2.up * 7f;
                Debug.Log(rigidBody.velocity);
            }
        }

        /*if (m_PlayerInput != Vector2.zero && !m_UsingLongArm) 
        {
            rigidBody.velocity = new Vector2(moveSpeed * m_PlayerInput.x, rigidBody.velocity.y);
        }

        else if (m_UsingLongArm)
        {
            if (!m_IsGrounded)
            {
                rigidBody.velocity += Vector2.up * Physics2D.gravity * Time.fixedDeltaTime;
            }
            else
            {
                rigidBody.velocity = Vector2.zero;
            }
        }

        else
        {
            rigidBody.velocity = Vector2.up * rigidBody.velocity;
        } */
     
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

        if (m_UsingLongArm)
        {
            m_LongArmTimer -= Time.deltaTime;

            if (m_LongArmTimer < 0)
            {
                m_UsingLongArm = false;
                //rigidBody.isKinematic = false;
                m_Animator.SetBool("Long Arm", false);
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!m_IsGrounded && collision.collider.gameObject.tag == "Ground")
        {
            m_IsGrounded = true;
            m_Animator.SetBool("Is Grounded", true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.gameObject.tag == "Ground")
        {
            m_IsGrounded = false;
            m_Animator.SetBool("Is Grounded", false);        
        }
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
        HealthBar.instance.SetValue(currentHealth / m_MaxHealth);
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
            gameObject.GetComponent<SpriteRenderer> ().enabled = true;
            return;
        }
        
        m_BlinkingPhase += Time.deltaTime;
        if(m_BlinkingPhase >= blinkingInterval)
        {
            m_BlinkingPhase = 0.0f;
            if (gameObject.GetComponent<SpriteRenderer>().enabled) 
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            } 
            else 
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
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

    void UseLongArm()
    {
        if (CanUseExtendableArm())
        {
            m_UsingLongArm = true;
            m_Animator.SetBool("Long Arm", true);
            m_LongArmTimer = longArmInterval;
            if (m_IsGrounded)
            {
                //rigidBody.isKinematic = true;
                rigidBody.velocity = Vector2.zero;
            }
             
        }
    }

    public void SetKinematic(bool value)
    {
        rigidBody.isKinematic = value;
    }

    public void SetGrabbed(bool value)
    {
        m_Grabbed = value;
    }

    void SetJumpSpeed(float speed)
    {
        jumpSpeed = speed;
    }

    void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public bool CanUseExtendableArm()
    {
        return PlayerSkills.instance.IsSkillUnlocked(PlayerSkills.SkillType.ExtendableArm);
    }

    
}
