using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills 
{

    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;
    public class OnSkillUnlockedEventArgs : EventArgs {
        public SkillType skillType;
    }

    public static PlayerSkills instance;
    public static float m_PropulsorsNewSpeed = 13f;
    public static float m_AcceleratorsNewSpeed = 7f;
    public static float m_JetpackBoostVelocity = 15f;
    public static float m_StandardMaxJetpackFuel = 4f;

    public enum SkillType {
        Propulsors,
        MagneticAccelerators,
        ExtendableArm,
        Jetpack
    }

    List<SkillType> m_UnlockedSkillsList;

    PlayerSkills()
    {
        m_UnlockedSkillsList = new List<SkillType>();
        instance = this;
    }

    public static void GetSkills()
    {
        if (instance == null)
        {
            new PlayerSkills();
        }
    }

    public void UnlockSkill(SkillType type)
    {
        if (!m_UnlockedSkillsList.Contains(type))
        {
            m_UnlockedSkillsList.Add(type);
            OnSkillUnlocked?.Invoke(this, new OnSkillUnlockedEventArgs { skillType = type });
        }
    }

    public bool IsSkillUnlocked(SkillType type)
    {
        return m_UnlockedSkillsList.Contains(type);
    }

}
