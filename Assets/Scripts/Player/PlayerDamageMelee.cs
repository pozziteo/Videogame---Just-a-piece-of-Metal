using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageMelee : MonoBehaviour
{
    public float meleeDamage;
    void OnTriggerEnter2D(Collider2D other)
    {
        BaseEnemy enemy = other.gameObject.GetComponent<BaseEnemy>();

        if (enemy != null)
        {
            enemy.ChangeHealth(-meleeDamage);
            if (enemy.Health == 0)
            {
                PlayerController.Player.UseRage(enemy.startHealth);
            }
        }
    }
    
}
