using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Player;
    public static float MoveSpeed = 5f;        //Horizontal speed of the player
    public static float JumpSpeed = 9f;      //Vertical speed when player jumps
    public static float MaxHealth = 5f;              //Max health of the player
    public float fallJumpMultiplier;    //Coefficient of boost to gravity when falling down
    public float Health {get { return currentHealth; }} 
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
    public ParticleSystem jetpackEffect;    //Particle system when using jetpack
    Vector2 lookDirection = new Vector2(1,0);       //Look direction of the player
    Vector2 m_PlayerInput;              //Input movement
    Rigidbody2D rigidBody;
    Animator m_Animator;
    [SerializeField] GameObject m_ActualCheckpoint;
    ParticleSystem m_ParticleJetpack;
    [SerializeField] bool m_IsDead;
    [SerializeField] bool m_ShouldJump;
    [SerializeField] bool m_IsGrounded;
    [SerializeField] bool m_IsInvincible;
    bool m_IsHit;
    [SerializeField] bool m_IsCooling;
    [SerializeField] bool m_Poisoned;
    [SerializeField] bool m_UsingLongArm;
    [SerializeField] bool m_Grabbed;
    [SerializeField] bool m_UsingJetpack;
    float m_InvincibleTimer;
    float m_BlinkingTime;
    float m_BlinkingPhase;
    float m_DeathTimer;
    float m_AttackTimer;
    float m_LongArmTimer;
    float m_MaxJetpackFuel;
    float m_CurrentJetpackFuel;
    float m_JetpackBoostVelocity;
    [SerializeField] float m_StatsModifier = 1f;
    float m_PoisonTotalTime;
    float m_PoisonedTime;
    
    void Awake()
    {
        if (Player == null)
        {
            Player = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start () 
    {
        rigidBody = GetComponent<Rigidbody2D> ();
        m_Animator = GetComponent<Animator>();
        PlayerSkills.GetSkills();
        PlayerSkills.instance.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;

        currentHealth = MaxHealth;
    }

    //Method called when player unlocks a skill. Change player parameters
    void PlayerSkills_OnSkillUnlocked(object sender, PlayerSkills.OnSkillUnlockedEventArgs e)
    {
        switch (e.skillType)
        {
            case PlayerSkills.SkillType.Propulsors:
                SetJumpSpeed(PlayerSkills.m_PropulsorsNewSpeed);
                break;
            
            case PlayerSkills.SkillType.MagneticAccelerators:
                SetMoveSpeed(PlayerSkills.m_AcceleratorsNewSpeed);
                break;
            
            case PlayerSkills.SkillType.Jetpack:
                SetJetpackParams(PlayerSkills.m_StandardMaxJetpackFuel, PlayerSkills.m_JetpackBoostVelocity);
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
                currentHealth = MaxHealth;
                HealthBar.instance.SetValue(1f);
                MoveToSpawnpoint(m_ActualCheckpoint);
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
                if (swit != null && !swit.IsActivated()) 
                {
                    swit.ActivateSwitch();
                }
                DoorBehaviour door = hit.collider.GetComponent<DoorBehaviour>();
                if (door != null)
                {
                    if (!door.IsOpen())
                    {
                        door.OpenDoor();
                    }
                    else
                    {
                        door.ChangeLevel();
                    }
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

        if (Input.GetKey(KeyCode.Space) && !m_IsGrounded)
        {
            UseJetpack();
        }

        if (m_UsingJetpack && Input.GetKeyUp(KeyCode.Space))
        {
            TurnOffJetpack();
        }
            
    }

    void FixedUpdate() 
    {
        if (m_IsDead)
        {
            return;
        }

        if (!m_UsingLongArm) 
        {
            rigidBody.velocity = new Vector2(MoveSpeed * m_PlayerInput.x, rigidBody.velocity.y) * m_StatsModifier;
        }

        else 
        {
            if (!m_IsGrounded && !m_Grabbed)
            {
                rigidBody.velocity = new Vector2(MoveSpeed * m_PlayerInput.x * 0.2f, rigidBody.velocity.y * 0.2f) * m_StatsModifier;
            }

            else if (m_IsGrounded && !m_Grabbed)
            {
                rigidBody.velocity = Vector2.zero;
            }

            else 
            {
                rigidBody.velocity = Vector2.right * lookDirection * 7f + Vector2.up * 7f;
            }
        }
     
        if (m_ShouldJump && m_IsGrounded) 
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, JumpSpeed) * m_StatsModifier;
            m_Animator.SetTrigger("Jump");
            m_ShouldJump = false;
        }

        if (m_UsingJetpack)
        {
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.velocity += 2.5f * Vector2.up * Time.fixedDeltaTime * m_JetpackBoostVelocity;
            }
            else
            {
                rigidBody.velocity += Vector2.up * Time.fixedDeltaTime * m_JetpackBoostVelocity;               
            }
        }

        else if (rigidBody.velocity.y < 0)
        {
            rigidBody.velocity += Vector2.up * Physics2D.gravity.y * fallJumpMultiplier * Time.fixedDeltaTime;
        }
    }

    void UpdateTimers()
    {
        if (m_IsInvincible)
        {
            m_InvincibleTimer -= Time.deltaTime * m_StatsModifier;
            if (m_InvincibleTimer < 0)
                m_IsInvincible = false;
        }

        if (m_IsCooling)
        {
            m_AttackTimer -= Time.deltaTime * m_StatsModifier;
            if (m_AttackTimer < 0)
            {
                m_IsCooling = false;
                m_Animator.SetBool("Shoot", false);
                m_Animator.SetBool("Melee", false);
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

        if (m_UsingLongArm)
        {
            m_LongArmTimer -= Time.deltaTime * m_StatsModifier;

            if (m_LongArmTimer < 0)
            {
                m_UsingLongArm = false;
                m_Animator.SetBool("Long Arm", false);
            }
        }

        if (m_UsingJetpack)
        {
            m_CurrentJetpackFuel -= Time.deltaTime;
            
            if (m_CurrentJetpackFuel < 0)
            {
                m_UsingJetpack = false;
                m_Animator.SetBool("Jetpack", false);
                TurnOffJetpack();
            }
            Debug.Log("Current fuel: " + m_CurrentJetpackFuel);

        }

        else if (m_IsGrounded && m_CurrentJetpackFuel < m_MaxJetpackFuel)
        {
            m_CurrentJetpackFuel += Time.deltaTime;

            if (m_CurrentJetpackFuel > m_MaxJetpackFuel)
            {
                m_CurrentJetpackFuel = m_MaxJetpackFuel;
            }
            Debug.Log("Current fuel: " + m_CurrentJetpackFuel);
        }
    }

    public static void MoveToSpawnpoint(GameObject spawnPoint)
    {
        Player.gameObject.transform.position = spawnPoint.transform.position;
    }

    public void SetCurrentCheckpoint(GameObject newSpawnPoint)
    {
        m_ActualCheckpoint = newSpawnPoint;
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

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);
        HealthBar.instance.SetValue(currentHealth / MaxHealth);
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
        TurnOffJetpack();
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

    void UseJetpack()
    {
        if (CanUseJetpack() && IsJetpackReady() && !m_UsingJetpack)
        {
            m_UsingJetpack = true;
            m_Animator.SetBool("Jetpack", true);
            m_ParticleJetpack = Instantiate(jetpackEffect, transform.position + Vector3.down * 1.75f, Quaternion.identity);
            m_ParticleJetpack.transform.parent = gameObject.transform;
        }
    }

    void TurnOffJetpack()
    {
        m_UsingJetpack = false;
        m_Animator.SetBool("Jetpack", false);
        
        if (m_ParticleJetpack != null)
        {
            Destroy(m_ParticleJetpack.gameObject);
        }
    }

    bool IsJetpackReady()
    {
        return m_CurrentJetpackFuel > 0;
    }

    public void Poison(float statsModifier, float poisoningTime)
    {
        m_Poisoned = true;
        m_StatsModifier = statsModifier;
        m_PoisonTotalTime = poisoningTime;
        m_PoisonedTime = m_PoisonTotalTime;
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
        JumpSpeed = speed;
    }

    void SetMoveSpeed(float speed)
    {
        MoveSpeed = speed;
    }

    void SetJetpackParams(float fuel, float jetpackBoost)
    {
        m_MaxJetpackFuel = fuel;
        m_CurrentJetpackFuel = m_MaxJetpackFuel;
        m_JetpackBoostVelocity = jetpackBoost;
    }

    bool CanUseExtendableArm()
    {
        return PlayerSkills.instance.IsSkillUnlocked(PlayerSkills.SkillType.ExtendableArm);
    }

    bool CanUseJetpack()
    {
        return PlayerSkills.instance.IsSkillUnlocked(PlayerSkills.SkillType.Jetpack);
    }
    
}
