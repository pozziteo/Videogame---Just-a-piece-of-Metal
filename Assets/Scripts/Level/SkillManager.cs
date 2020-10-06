using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager
{

    public static SkillManager Instance {
        get 
        {
            if (instance == null)
            {
                instance = new SkillManager();
            }

            return instance;
        }
    }
    static SkillManager instance;
    List<SkillUnlocker> m_AllSkills;
    LevelManager m_LevelManager;
    

    SkillManager()
    {
        m_AllSkills = new List<SkillUnlocker>();
        m_LevelManager = LevelManager.Instance;
    }

    public SkillUnlocker FindSkill(PlayerSkills.SkillType skillType) 
    {
        return m_AllSkills.Find(skill => skill.skillUnlock == skillType);
    }

    public void AddSkillUnlocker(string belongingScene, SkillUnlocker unlocker)
    {
        if (!m_AllSkills.Contains(unlocker))
        {
            m_AllSkills.Add(unlocker);
            m_LevelManager.RegisterSkill(belongingScene, unlocker);
            Debug.Log("Added new Skill unlocker: " + unlocker.skillUnlock.ToString());
        }
    }
}
