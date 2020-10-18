using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJetpack : MonoBehaviour
{
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
}
