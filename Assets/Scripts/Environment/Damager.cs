using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public float damage;
    public AudioClip damageClip;
    AudioSource m_AudioSource;

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            m_AudioSource.PlayOneShot(damageClip);
            player.ChangeHealth(-damage);
        }

        BaseEnemy enemy = other.gameObject.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            enemy.ChangeHealth(-damage);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            m_AudioSource.PlayOneShot(damageClip);
            m_AudioSource.loop = true;
            player.ChangeHealth(-damage);
        }
        
        BaseEnemy enemy = other.gameObject.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            enemy.ChangeHealth(-damage);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
         PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            m_AudioSource.Stop();
        }
    }
}
