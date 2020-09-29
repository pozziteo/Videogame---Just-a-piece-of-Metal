using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    public float damage;

    void OnCollisionEnter2D(Collision2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
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
            player.ChangeHealth(-damage);
        }
        
        BaseEnemy enemy = other.gameObject.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            enemy.ChangeHealth(-damage);
        }
    }
}
