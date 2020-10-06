using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills 
{

    public event EventHandler<OnSkillUnlockedEventArgs> OnSkillUnlocked;
    public class OnSkillUnlockedEventArgs : EventArgs {
        public SkillType skillType;
    }

    static PlayerSkills instance;
    public float propulsorsNewSpeed = 13f;
    public float acceleratorsNewSpeed = 7f;
    public float jetpackBoostVelocity = 15f;
    public float standardMaxJetpackFuel = 4f;
    public float nuclearGunDamage = 3f;
    public float ironSkinModifier = 2f;

    public enum SkillType {

        [Description("Now you can jump higher than before")]
        Propulsors,
        [Description("Now you can move faster than before")]
        MagneticAccelerators,
        [Description("Now you can use an extendable arm. Use it against enemies or with the environment\n\nPress Q to use the arm")]
        ExtendableArm,
        [Description("Now you can use a jetpack when you are in air. Hold SPACE to use it")]
        Jetpack,
        [Description("Now your projectiles are more powerful")]
        NuclearGun,
        [Description("Your maximum health is now increased")]
        IronSkin
    }

    List<SkillType> m_UnlockedSkillsList;

    PlayerSkills()
    {
        m_UnlockedSkillsList = new List<SkillType>();
        instance = this;
    }

    public static PlayerSkills GetSkills()
    {
        if (instance == null)
        {
            new PlayerSkills();
        }

        return instance;
    }

    public static string GetSkillDescription(SkillType value)
    {
        DescriptionAttribute[] da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString()))
                                        .GetCustomAttributes(typeof(DescriptionAttribute), false);
		return da.Length > 0 ? da[0].Description : value.ToString();
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
