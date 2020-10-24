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

    public void ShowRageMessage()
    {
        StartCoroutine(ShowMessage());
    }

    public void DestroyUI()
    {
        instance = null;
        Destroy(gameObject);
    }

    IEnumerator ShowMessage()
    {
        gameObject.transform.Find("BonusHealthText").gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        gameObject.transform.Find("BonusHealthText").gameObject.SetActive(false);
    }
}
