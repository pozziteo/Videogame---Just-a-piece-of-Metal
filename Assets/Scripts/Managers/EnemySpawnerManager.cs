using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerManager : MonoBehaviour
{
    public static EnemySpawnerManager Instance
    {
        get
        {
            return instance;
        }
    }
    static EnemySpawnerManager instance;

    List<BaseEnemy> m_EnemyToRespawn;
    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this);
            instance = this;
            m_EnemyToRespawn = new List<BaseEnemy>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDeadEnemy(BaseEnemy enemy, float timeRespawn)
    {
        m_EnemyToRespawn.Add(enemy);
        enemy.enabled = false;
        StartCoroutine(RunTimer(enemy, timeRespawn));
    }

    void RespawnEnemy(BaseEnemy enemy)
    {
        BaseEnemy toRespawn = m_EnemyToRespawn.Find(en => en == enemy);
        if (toRespawn != null)
        {
            m_EnemyToRespawn.Remove(toRespawn);
            toRespawn.enabled = true;
            toRespawn.Respawn();
        }
    }

    IEnumerator RunTimer(BaseEnemy enemy, float timeRespawn)
    {
        yield return new WaitForSeconds(timeRespawn);
        RespawnEnemy(enemy);
    }

    
}
