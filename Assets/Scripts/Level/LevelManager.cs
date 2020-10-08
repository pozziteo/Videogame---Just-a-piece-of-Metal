using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager
{   
    public static LevelManager Instance {
        get
        {
            if (instance == null)
            {
                instance = new LevelManager();
            }
            return instance;
        }
    }
    static LevelManager instance;

    public Dictionary<string, List<DoorBehaviour>> doorsInScene;
    public Dictionary<string, List<SkillUnlocker>> skillsInScene;
    public Dictionary<string, List<SwitchBehaviour>> switchInScene;

    LevelManager()
    {
        doorsInScene = new Dictionary<string, List<DoorBehaviour>>();
        skillsInScene = new Dictionary<string, List<SkillUnlocker>>();
        switchInScene = new Dictionary<string, List<SwitchBehaviour>>();
    }

    public void RegisterDoor(string sceneName, DoorBehaviour door)
    {
        bool present = doorsInScene.TryGetValue(sceneName, out List<DoorBehaviour> doors);

        if (!present)
        {
            doors = new List<DoorBehaviour>();
            doorsInScene.Add(sceneName, doors);
        }

        doors.Add(door);
    }

    public void RegisterSkill(string sceneName, SkillUnlocker skillUnlocker)
    {
        bool present = skillsInScene.TryGetValue(sceneName, out List<SkillUnlocker> skills);

        if (!present)
        {
            skills = new List<SkillUnlocker>();
            skillsInScene.Add(sceneName, skills);
        }

        skills.Add(skillUnlocker);
    }

    public void RegisterSwitch(string sceneName, SwitchBehaviour swit)
    {
        bool present = switchInScene.TryGetValue(sceneName, out List<SwitchBehaviour> switches);

        if (!present)
        {
            switches = new List<SwitchBehaviour>();
            switchInScene.Add(sceneName, switches);
        }

        switches.Add(swit);
    }

    public void DisablePersistentObjects(string sceneName)
    {
        DisableSceneDoors(sceneName);
        DisableSceneSkills(sceneName);
        DisableSceneSwitches(sceneName);
    }

    void DisableSceneDoors(string currentScene)
    {
        doorsInScene.TryGetValue(currentScene, out List<DoorBehaviour> activeDoors);

        if (activeDoors != null)
        {
            foreach (DoorBehaviour door in activeDoors)
            {
                door.gameObject.SetActive(false);
            }
        }
    }

    void DisableSceneSkills(string currentScene)
    {
        skillsInScene.TryGetValue(currentScene, out List<SkillUnlocker> activeSkills);

        if (activeSkills != null)
        {   
            foreach (SkillUnlocker skill in activeSkills)
            {
                skill.gameObject.SetActive(false);
            }
        }
    }

    void DisableSceneSwitches(string currentScene)
    {
        switchInScene.TryGetValue(currentScene, out List<SwitchBehaviour> activeSwitches);

        if (activeSwitches != null)
        {
            foreach (SwitchBehaviour swit in activeSwitches)
            {
                swit.gameObject.SetActive(false);
            }
        }
    }

    public void EnablePersistentObjects(string sceneName)
    {
        EnableNextSceneDoors(sceneName);
        EnableNextSceneSkills(sceneName);
        EnableNextSceneSwitches(sceneName);
    }

    void EnableNextSceneDoors(string nextScene)
    {
        bool alreadyVisited = doorsInScene.TryGetValue(nextScene, out List<DoorBehaviour> doors);

        if (alreadyVisited)
        {
            foreach (DoorBehaviour door in doors)
            {
                door.gameObject.SetActive(true);
            }
        }
    }

    void EnableNextSceneSkills(string nextScene)
    {
        bool alreadyVisited = skillsInScene.TryGetValue(nextScene, out List<SkillUnlocker> skills);

        if (alreadyVisited)
        {
            foreach (SkillUnlocker skill in skills)
            {
                skill.gameObject.SetActive(true);
            }
        }
    }

    void EnableNextSceneSwitches(string nextScene)
    {
        bool alreadyVisited = switchInScene.TryGetValue(nextScene, out List<SwitchBehaviour> switches);

        if (alreadyVisited)
        {
            foreach (SwitchBehaviour swit in switches)
            {
                swit.gameObject.SetActive(true);
            }
        }
    }

}
