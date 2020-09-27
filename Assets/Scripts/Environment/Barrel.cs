using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Barrel : MonoBehaviour
{
    public float damage;
    public ParticleSystem explosion;
    public float explosionDuration;
    public float delayChainExplosion;
    [SerializeField] bool m_Exploded;
    [SerializeField] float m_ElapsedDuration;
    float m_ExplosionRadius;
    float m_RadiusMultiplier = 1.3f;


    protected virtual void Start()
    {
        m_ExplosionRadius = GetComponent<CircleCollider2D>().radius;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
    }

    protected virtual void Update()
    {
        if (m_Exploded)
        {
            m_ElapsedDuration -= Time.deltaTime;
            if (m_ElapsedDuration < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public virtual void Explode()
    {
        if (!m_Exploded)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            m_Exploded = true;
            Instantiate(explosion, transform.position, Quaternion.identity);
            gameObject.GetComponent<CircleCollider2D>().enabled = true;
            m_ElapsedDuration = explosionDuration;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, m_RadiusMultiplier * m_ExplosionRadius);
            foreach (Collider2D collider in colliders)
            {
                Barrel bar = collider.gameObject.GetComponent<Barrel>();

                if (bar != null)
                {
                    bar.Invoke("Explode", delayChainExplosion);                
                }
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Projectile")
        {
            Explode();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        PlayerDamageMelee playerMelee = other.gameObject.GetComponent<PlayerDamageMelee>();

        if (playerMelee != null)
        {
            Explode();
        }
    }

}
