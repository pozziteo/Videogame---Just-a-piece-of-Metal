using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    public AudioClip meleeSound;
    public float runSpeed;
    public float attackDistance;
    public float meleeCooldown;
    [SerializeField] float m_MeleeTimer;

    protected override void Update()
    {
        base.Update();

        if (m_Caught)
        {
            m_Animator.SetBool("Can Walk", false);
            m_Animator.SetBool("Can Run", false);
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
        if (m_Caught || m_Cooling || m_IsFixedEnemy)
        {
            return;
        }

        if (!m_PlayerInRange)
        {
          
            m_AudioSource.clip = m_BaseSound;
            m_AudioSource.loop = true;
            m_AudioSource.pitch = 1f;
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
            m_Animator.SetBool("Can Run", false);
            m_Animator.SetBool("Can Walk", true);
            m_RigidBody.velocity = (Vector2.right * moveSpeed * m_LookDirection  + Vector2.up * m_RigidBody.velocity.y) * m_StatsModifier;
        }
        else
        {
            float distance = Vector2.Distance(m_Target.position, transform.position);
            if (distance > attackDistance)
            {
                m_Animator.SetBool("Can Run", true);
                m_Animator.SetBool("Can Walk", false);
                m_RigidBody.velocity = (Vector2.right * runSpeed * m_LookDirection  + Vector2.up * m_RigidBody.velocity.y) * m_StatsModifier;
            }
            else
            {
                m_Animator.SetBool("Can Run", false);
                m_Animator.SetBool("Can Walk", false);
                m_RigidBody.velocity = Vector2.zero;
            }
        }
    }

    protected override void UpdateTimers()
    {
        base.UpdateTimers();

        if (m_Cooling)
        {
            m_MeleeTimer -= Time.deltaTime;
            if (m_MeleeTimer < 0)
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
            m_Target = player.gameObject.transform;
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
        if (!m_Cooling)
        {
            float distance = Vector2.Distance(transform.position, m_Target.position);

            if (distance > attackDistance)
            {
                MoveToPlayer();
                m_Cooling = false;
            }
            else
            {
                Melee();
            }
        }
    }

    void Melee()
    {
        m_MeleeTimer = meleeCooldown;
        m_Cooling = true;
        m_Animator.SetBool("Can Run", false);
        m_AudioSource.Stop();
        m_Animator.SetTrigger("Melee");
    }

    void MoveToPlayer()
    {
        if (!m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Melee"))
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
            m_AudioSource.pitch = 2f;
            Vector2 targetPos = new Vector2(m_Target.position.x, transform.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, runSpeed * Time.deltaTime * m_StatsModifier);
        }
    }

    void PlayMeleeSound()
    {
        m_AudioSource.PlayOneShot(meleeSound);
    }
}
