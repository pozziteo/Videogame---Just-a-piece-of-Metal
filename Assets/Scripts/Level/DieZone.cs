using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == PlayerController.Player.gameObject)
        {
            PlayerController.Player.ChangeHealth(-PlayerController.MaxHealth);
        }
        else 
        {
            BaseEnemy enemy = other.gameObject.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.ChangeHealth(-enemy.startHealth);
            }
        }
    }
}
