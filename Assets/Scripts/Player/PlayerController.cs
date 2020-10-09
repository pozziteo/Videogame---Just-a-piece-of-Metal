﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Player;
    public static float MoveSpeed = 5f;        //Horizontal speed of the player
    public static float JumpSpeed = 9f;      //Vertical speed when player jumps
    public static float MaxHealth = 5f;              //Max health of the player
    public static float shootDamage = 1f;           //Damage to health from shooting
    public static float m_MaxJetpackFuel = 0f;
    public float projectileForce;
    public float fallJumpMultiplier;    //Coefficient of boost to gravity when falling down
    public float timeInvincible;        //Time interval in which player is invincible after being hit
    public float blinkingHitTime;       //Blinking animation time after being hit
    public float blinkingInterval;      //Time interval of a single blinking
    public float timeDead;              //Time interval in which player remains dead
    public float attackCooldown;        //Time interval to wait for attacking again
    public float longArmInterval;       //Time interval for long arm animation
    public GameObject usedProjectilePrefab;     //Currently used projectile sprite 
    public GameObject nuclearGunProjectile;     //Projectile prefab of the skill NuclearGun
    public ParticleSystem shootEffect;      //Particle system when shooting
    public ParticleSystem jetpackEffect;    //Particle system when using jetpack
    public AudioClip simpleGunSound;
    public AudioClip nuclearGunSound;
    public List<AudioClip> damageSounds;
    AudioSource m_AudioSource;
    Vector2 m_LookDirection = new Vector2(1,0);       //Look direction of the player
    Vector2 m_PlayerInput;              //Input movement
    Rigidbody2D m_Rigidbody;
    Animator m_Animator;
    PlayerSkills m_PlayerSkills;
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
    [SerializeField] bool m_ArmBoost;
    [SerializeField] bool m_FallingFromJetpack;
    static float m_CurrentHealth = MaxHealth;
    float m_InvincibleTimer;
    float m_BlinkingTime;
    float m_BlinkingPhase;
    float m_DeathTimer;
    float m_AttackTimer;
    float m_LongArmTimer;
    static float m_CurrentJetpackFuel;
    static float m_JetpackBoostVelocity;
    [SerializeField] float m_StatsModifier = 1f;
    float m_PoisonTotalTime;
    float m_PoisonedTime;
    
    void Awake()
    {
        if (Player == null)
        {
            Player = this;
            DontDestroyOnLoad(this);
            m_PlayerSkills = PlayerSkills.GetSkills();
            m_PlayerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start () 
    {
        m_Rigidbody = GetComponent<Rigidbody2D> ();
        m_Animator = GetComponent<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
    }

    //Method called when player unlocks a skill. Change player parameters
    void PlayerSkills_OnSkillUnlocked(object sender, PlayerSkills.OnSkillUnlockedEventArgs e)
    {
        switch (e.skillType)
        {
            case PlayerSkills.SkillType.Propulsors:
                SetJumpSpeed(m_PlayerSkills.propulsorsNewSpeed);
                break;
            
            case PlayerSkills.SkillType.MagneticAccelerators:
                SetMoveSpeed(m_PlayerSkills.acceleratorsNewSpeed);
                break;
            
            case PlayerSkills.SkillType.Jetpack:
                SetJetpackParams(m_PlayerSkills.standardMaxJetpackFuel, m_PlayerSkills.jetpackBoostVelocity);
                break;

            case PlayerSkills.SkillType.NuclearGun:
                SetNuclearGun(m_PlayerSkills.nuclearGunDamage);
                break;

            case PlayerSkills.SkillType.IronSkin:
                SetIronSkin(m_PlayerSkills.ironSkinModifier);
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
            if (m_Rigidbody.velocity.y < 0)
            {
                m_Rigidbody.velocity = Vector2.up * m_Rigidbody.velocity;
            }
            else 
            {
                m_Rigidbody.velocity = Vector2.zero;
            }
            m_DeathTimer += Time.deltaTime;
            if (m_DeathTimer > timeDead)
            {
                m_IsDead = false;
                m_IsInvincible = false;
                m_DeathTimer = 0f;
                m_CurrentHealth = MaxHealth;
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
            m_LookDirection.Set(m_PlayerInput.x, m_PlayerInput.y);
            m_LookDirection.Normalize();
        }

        m_Animator.SetFloat("Look X", m_LookDirection.x);

        if(Input.GetKeyDown(KeyCode.Space) && m_IsGrounded)
        {
            m_ShouldJump = true;
        }

         else if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit2D hit = Physics2D.Raycast(m_Rigidbody.position + Vector2.up * 0.2f, m_LookDirection, 
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

        if (!m_UsingLongArm)        //Condition for standard movement
        {
            m_Rigidbody.velocity = new Vector2(MoveSpeed * m_PlayerInput.x, m_Rigidbody.velocity.y) * m_StatsModifier;
        }

        else 
        {
            if (!m_IsGrounded && !m_ArmBoost)    //Using long arm while in air slows down the character
            {
                m_Rigidbody.velocity = new Vector2(MoveSpeed * m_PlayerInput.x * 0.2f, m_Rigidbody.velocity.y * 0.2f) * m_StatsModifier;
            }

            else if (!m_ArmBoost)        //If using long arm while on ground, stop the character
            {
                m_Rigidbody.velocity = Vector2.zero;
            }
        }
     
        if (m_ShouldJump && m_IsGrounded) 
        {
            m_Rigidbody.velocity = new Vector2(m_Rigidbody.velocity.x, JumpSpeed) * m_StatsModifier;
            m_Animator.SetTrigger("Jump");
            m_ShouldJump = false;
        }

        if (m_UsingJetpack)
        {
            if (m_Rigidbody.velocity.y < 0)
            {
                m_Rigidbody.velocity += 2.5f * Vector2.up * Time.fixedDeltaTime * m_JetpackBoostVelocity;
            }
            else
            {
                m_Rigidbody.velocity += Vector2.up * Time.fixedDeltaTime * m_JetpackBoostVelocity;               
            }
        }

        else if (m_Rigidbody.velocity.y < 0)
        {
            m_Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * fallJumpMultiplier * Time.fixedDeltaTime;
            if (m_ArmBoost)
            {
                m_ArmBoost = false;
            }
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
                m_Grabbed = false;
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

        }

        else if (m_IsGrounded && m_CurrentJetpackFuel < m_MaxJetpackFuel)
        {
            m_CurrentJetpackFuel += Time.deltaTime;

            if (m_CurrentJetpackFuel > m_MaxJetpackFuel)
            {
                m_CurrentJetpackFuel = m_MaxJetpackFuel;
            }
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!m_IsGrounded && collision.collider.gameObject.tag == "Ground")
        {
            m_IsGrounded = true;
            m_Animator.SetBool("Is Grounded", true);

            if (!m_FallingFromJetpack)
            {
                float normalizedVerticalVelocity = collision.relativeVelocity.y * 0.85f;
                Debug.Log(normalizedVerticalVelocity);
                if (normalizedVerticalVelocity > 25f)
                {
                    float fallDamage = normalizedVerticalVelocity * 0.04f;
                    if (Mathf.Abs(fallDamage - Mathf.Floor(fallDamage) - 0.5f) < 0.2f)
                    {
                        fallDamage = Mathf.Floor(fallDamage) + 0.5f;
                    }
                    else
                    {
                        fallDamage = Mathf.Round(fallDamage);
                    }

                    ChangeHealth(-fallDamage);
                }
            }
            else
            {
                m_FallingFromJetpack = false;
            }
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

                int indexSound = Random.Range(0, damageSounds.Count);
                PlaySound(damageSounds[indexSound]);
            }

        m_CurrentHealth = Mathf.Clamp(m_CurrentHealth + amount, 0, MaxHealth);
        Debug.Log("Health: " + m_CurrentHealth);
        HealthBar.instance.SetValue(m_CurrentHealth / MaxHealth);
        if (m_CurrentHealth == 0)
        {
            Die();
        }
    }

    void SpriteBlinkingEffect()
    {
        if (m_CurrentHealth == 0)
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

        Vector2 playerPos = m_Rigidbody.transform.position;
        Vector2 direction = new Vector2(Mathf.RoundToInt(m_LookDirection.x), Mathf.RoundToInt(m_LookDirection.y));
        direction.Normalize();

        float angle = Mathf.Acos(direction.x / (float) direction.magnitude) * Mathf.Rad2Deg;

        if (m_LookDirection.y < 0)
        {
            angle = -angle;
        }

        Instantiate(shootEffect, playerPos + Vector2.up * 0.13f + Vector2.right * m_LookDirection.x * 0.90f,
                        Quaternion.AngleAxis(angle, Vector3.forward));

        GameObject projectileObject = Instantiate(usedProjectilePrefab, playerPos + Vector2.up * 0.13f + 
                                    Vector2.right * m_LookDirection.x * 0.95f, 
                                    Quaternion.AngleAxis(angle, Vector3.forward));

        PlayerProjectile projectile = projectileObject.GetComponent<PlayerProjectile>();
        projectile.Damage = shootDamage;
        projectile.Launch(direction, projectileForce);
        if (m_PlayerSkills.IsSkillUnlocked(PlayerSkills.SkillType.NuclearGun))
        {
            PlaySound(nuclearGunSound);
        }
        else
        {
            PlaySound(simpleGunSound);
        }
    }

    void UseLongArm()
    {
        if (CanUseExtendableArm() && !m_UsingLongArm)
        {
            m_UsingLongArm = true;
            m_Animator.SetBool("Long Arm", true);
            m_LongArmTimer = longArmInterval;
            if (m_IsGrounded)
            {
                m_Rigidbody.velocity = Vector2.zero;
            }
             
        }
    }

    public void ApplyArmVelocityBoost()
    {
        if (m_Grabbed)
        {
            m_ArmBoost = true;
            Vector2 forceDir = Vector2.right * m_LookDirection.x + Vector2.up;
            forceDir.Normalize();

            m_Rigidbody.AddForce(forceDir * 20f, ForceMode2D.Impulse);
            Debug.Log("Applied force: " + forceDir * 20f);
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
        m_FallingFromJetpack = true;
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
        m_Rigidbody.isKinematic = value;
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

    void SetNuclearGun(float newDamage)
    {
        shootDamage = newDamage;
        usedProjectilePrefab = nuclearGunProjectile;
    }

    void SetIronSkin(float modifier)
    {
        MaxHealth = modifier * MaxHealth;
        m_CurrentHealth = MaxHealth;
    }

    bool CanUseExtendableArm()
    {
        return m_PlayerSkills.IsSkillUnlocked(PlayerSkills.SkillType.ExtendableArm);
    }

    bool CanUseJetpack()
    {
        return m_PlayerSkills.IsSkillUnlocked(PlayerSkills.SkillType.Jetpack);
    }
    
    public void PlaySound(AudioClip clip)
    {
        m_AudioSource.PlayOneShot(clip);
    }
}
