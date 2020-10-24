using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJetpack : MonoBehaviour
{
    public static UIJetpack Instance
    {
        get
        {
            return instance;
        }
    }
    static UIJetpack instance;
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
        instance = null;
        Destroy(gameObject);
    }
}
