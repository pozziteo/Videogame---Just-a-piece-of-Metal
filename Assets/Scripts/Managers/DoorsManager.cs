using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorsManager
{
    public static DoorsManager Instance {
        get 
        {
            if (instance == null)
            {
                instance = new DoorsManager();
            }

            return instance;
        }
    }
    static DoorsManager instance;
    List<DoorBehaviour> m_AllDoors;
    LevelManager m_LevelManager;


    DoorsManager()
    {
        m_AllDoors = new List<DoorBehaviour>();
        m_LevelManager = LevelManager.Instance;
    }

    public DoorBehaviour FindDoor(string doorID)
    {
        return m_AllDoors.Find(door => door.doorID == doorID);
    }

    public void AddDoor(string belongingScene, DoorBehaviour door)
    {
        if (!m_AllDoors.Contains(door))
        {
            m_AllDoors.Add(door);
            m_LevelManager.RegisterDoor(belongingScene, door);
        }
    }

    public void DisableSceneObjects(string currentScene)
    {
        m_LevelManager.DisablePersistentObjects(currentScene);
    }

    public void EnableNextSceneObjects(string nextScene)
    {
        m_LevelManager.EnablePersistentObjects(nextScene);
    }

    public void DestroyDoors()
    {
        while (m_AllDoors.Count != 0)
        {
            DoorBehaviour door = m_AllDoors[0];
            m_AllDoors.Remove(door);
            door.DestroyDoor();
        }
        instance = null;
    }
}
