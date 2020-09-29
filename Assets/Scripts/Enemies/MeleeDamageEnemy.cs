using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDamageEnemy : MonoBehaviour
{
    public float damage;

    void OnTriggerEnter2D(Collider2D other)
    {        
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        
        if (player != null)
        {
            player.ChangeHealth(-damage);
        }
    }
}
