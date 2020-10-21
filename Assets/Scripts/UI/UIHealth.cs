using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealth : MonoBehaviour
{
    public static UIHealth Instance
    {
        get
        {
            return instance;
        }
    }
    static UIHealth instance;
    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DestroyUI()
    {
        Destroy(gameObject);
    }
}
