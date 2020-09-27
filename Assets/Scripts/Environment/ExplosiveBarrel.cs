using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : Barrel
{
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        base.OnCollisionEnter2D(other);
    }

    public override void Explode()
    {
        base.Explode();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        
        if (player != null)
        {
            player.ChangeHealth(-damage);
        }
        else if (enemy != null)
        {
            enemy.ChangeHealth(-damage);
        }
        
    }
}
