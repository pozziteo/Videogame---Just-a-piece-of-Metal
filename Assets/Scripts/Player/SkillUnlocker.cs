using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUnlocker : MonoBehaviour
{
    public PlayerSkills.SkillType m_SkillUnlock;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            PlayerSkills.instance.UnlockSkill(m_SkillUnlock);
            Destroy(gameObject);
        }
    }
}
