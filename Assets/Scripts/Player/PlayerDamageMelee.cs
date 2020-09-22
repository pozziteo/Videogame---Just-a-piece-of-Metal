using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageMelee : MonoBehaviour
{
    public float meleeDamage;

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();

        if (enemy != null)
        {
            enemy.ChangeHealth(-meleeDamage);
        }
    }
    
}
