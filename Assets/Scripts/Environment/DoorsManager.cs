using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsManager
{
    static DoorsManager instance;
    public static List<DoorBehaviour> AllDoors;

    DoorsManager()
    {
        AllDoors = new List<DoorBehaviour>();
    }

    public static DoorsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new DoorsManager();
        }

        return instance;
    }

    public DoorBehaviour FindDoor(string doorID)
    {
        return AllDoors.Find(door => door.doorID == doorID);
    }

    public void AddDoor(DoorBehaviour door)
    {
        if (!AllDoors.Contains(door))
        {
            AllDoors.Add(door);
            Debug.Log("Door added: " + door.doorID);
        }
    }


}
