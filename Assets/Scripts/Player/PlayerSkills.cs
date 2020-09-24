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

    public enum SkillType {
        Propulsors,
        MagneticAccelerators,
        ExtendableArm
    }

    List<SkillType> m_UnlockedSkillsList;

    public PlayerSkills()
    {
        if (instance == null)
        {
            m_UnlockedSkillsList = new List<SkillType>();
            instance = this;
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
