using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BioriskBarrel : Barrel
{
    public float statsModifier = 0.65f;
    public float poisoningTime = 3.0f;
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

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
        
        if (player != null)
        {
            player.ChangeHealth(-damage);
            player.Poison(statsModifier, poisoningTime);
        }
        else if (enemy != null)
        {
            enemy.ChangeHealth(-damage);
            enemy.Poison(statsModifier, poisoningTime);
        }
        
    }
}
