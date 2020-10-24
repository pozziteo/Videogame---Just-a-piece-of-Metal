using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchManager
{
    public static SwitchManager Instance {
        get 
        {
            if (instance == null)
            {
                instance = new SwitchManager();
            }

            return instance;
        }
    }
    static SwitchManager instance;
    List<SwitchBehaviour> m_AllSwitches;
    LevelManager m_LevelManager;

    SwitchManager()
    {
        m_AllSwitches = new List<SwitchBehaviour>();
        m_LevelManager = LevelManager.Instance;
    }

    public SwitchBehaviour FindSwitch(string switId) 
    {
        return m_AllSwitches.Find(s => s.switchID == switId);
    }

    public void AddSwitch(string belongingScene, SwitchBehaviour swit)
    {
        if (!m_AllSwitches.Contains(swit))
        {
            m_AllSwitches.Add(swit);
            m_LevelManager.RegisterSwitch(belongingScene, swit);
        }
    }

    public void DestroySwitches()
    {
        while (m_AllSwitches.Count != 0)
        {
            SwitchBehaviour swit = m_AllSwitches[0];
            m_AllSwitches.Remove(swit);
            swit.DestroySwitch();
        }
        instance = null;
    }
}
