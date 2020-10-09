using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitSound : MonoBehaviour
{
    public List<AudioClip> damageSounds;

    void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();
        int indexSound = Random.Range(0, damageSounds.Count);

        source.PlayOneShot(damageSounds[indexSound]);
    }


}
