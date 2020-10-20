using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalArena : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public float timeToComplete;
    public float maxLivingEnemies;
    public int killedEnemiesForBonus;
    public TMP_Text timerText;
    public TMP_Text bonusHealthText;
    public List<GameObject> arenaDelimeters;
    public GameObject spawnPoints;
    public CanvasGroup arenaCanvas;
    BoxCollider2D m_ArenaTriggerer;
    static List<GameObject> LivingEnemies;
    static int CurrentLivingEnemies = 0;
    static int EnemiesKilledInRow = 0;
    List<GameObject> m_ReadySpawnPoints;
    float m_RemainingTime;
    bool m_Initiated;
    bool m_WaitingToSpawn;

    void Awake()
    {
        LivingEnemies = new List<GameObject>();
        m_ReadySpawnPoints = new List<GameObject>();
        Transform[] spawns = spawnPoints.GetComponentsInChildren<Transform>();
        foreach (Transform sp in spawns)
        {
            m_ReadySpawnPoints.Add(sp.gameObject);
        }
        m_ArenaTriggerer = GetComponent<BoxCollider2D>();
        arenaCanvas.alpha = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(Initiate());
            arenaCanvas.alpha = 1f;
            m_RemainingTime = timeToComplete;
            int minutes = Mathf.FloorToInt(m_RemainingTime / 60);
            int seconds = Mathf.FloorToInt(m_RemainingTime % 60);
            float fraction = m_RemainingTime * 1000;
            fraction = fraction % 1000;
            string timeText = string.Format ("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
            timerText.text = timeText;
            foreach (GameObject delimeter in arenaDelimeters)
            {
                delimeter.SetActive(true);
            }
            m_ArenaTriggerer.enabled = false;
        }
    }

    void Update()
    {
        if (m_Initiated)
        {
            if (m_RemainingTime > 0 && !PlayerController.Player.IsDead)
            {
                UpdateTimer();
                ManageEnemies();

                if (EnemiesKilledInRow == killedEnemiesForBonus)
                {
                    StartCoroutine(GiveBonusHealthToPlayer());
                    EnemiesKilledInRow = 0;
                }
            }
            else if (PlayerController.Player.IsDead)
            {
                Reset();
            }
            else
            {
                m_Initiated = false;
                foreach (GameObject delimeter in arenaDelimeters)
                {
                    delimeter.SetActive(false);
                }
                timerText.text = "00:00:000";
                StartCoroutine(ResetEnemies());
            }
        }
    }

    void Reset()
    {
        m_Initiated = false;
        m_RemainingTime = timeToComplete;
        arenaCanvas.alpha = 0f;

        foreach (GameObject delimiter in arenaDelimeters)
        {
            delimiter.SetActive(false);
        }

        m_ArenaTriggerer.enabled = true;
        StartCoroutine(ResetEnemies());
    }

    void UpdateTimer()
    {
        m_RemainingTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(m_RemainingTime / 60);
        int seconds = Mathf.FloorToInt(m_RemainingTime % 60);
        float fraction = m_RemainingTime * 1000;
        fraction = fraction % 1000;
        string timeText = string.Format ("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
        timerText.text = timeText;
    }

    void ManageEnemies()
    {
        if (CurrentLivingEnemies < maxLivingEnemies && !m_WaitingToSpawn)
        {
            GameObject randomSpawn = m_ReadySpawnPoints[Random.Range(0, m_ReadySpawnPoints.Count)];

            GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject newEnemy = Instantiate(randomEnemy, randomSpawn.transform.position, Quaternion.identity);

            LivingEnemies.Add(newEnemy);

            m_WaitingToSpawn = true;
            m_ReadySpawnPoints.Remove(randomSpawn);
            StartCoroutine(WaitForNextRespawn());
            StartCoroutine(CooldownSpawnPoint(randomSpawn));

            CurrentLivingEnemies += 1;
        }
    }

    public static void KillEnemy(GameObject enemy)
    {
        LivingEnemies.Remove(enemy);
        Destroy(enemy);
        CurrentLivingEnemies -= 1;
        EnemiesKilledInRow += 1;
    }

    IEnumerator Initiate()
    {
        yield return new WaitForSeconds(5f);
        m_Initiated = true;
    }

    IEnumerator WaitForNextRespawn()
    {
        yield return new WaitForSeconds(2.5f);
        m_WaitingToSpawn = false;
    }

    IEnumerator CooldownSpawnPoint(GameObject spawnPoint)
    {
        yield return new WaitForSeconds(5f);

        if (!m_ReadySpawnPoints.Contains(spawnPoint))
        {
            m_ReadySpawnPoints.Add(spawnPoint);
        }
    }

    IEnumerator GiveBonusHealthToPlayer()
    {
        float bonusHealth = Mathf.Round(Random.Range(PlayerController.MaxHealth/3, PlayerController.MaxHealth * 2/3));
        PlayerController.Player.ChangeHealth(bonusHealth);
        bonusHealthText.text = "Bonus health received!";

        yield return new WaitForSeconds(3f);

        bonusHealthText.text = null;
    }

    IEnumerator ResetEnemies()
    {
        yield return new WaitForSeconds(2f);
        while (LivingEnemies.Count != 0)
        {
            GameObject enemy = LivingEnemies[0];
            KillEnemy(enemy);
        }
        EnemiesKilledInRow = 0;
        StopAllCoroutines();
    }
}
