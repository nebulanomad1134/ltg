using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime;

    public int bossSpriteType;
    public SpawnData bossData;

    int level;
    float timer;
    bool bossSpawned = false;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }
    void Update()
    {
        if (!GameManager.instance.isLive) return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt (GameManager.instance.gameTime / levelTime), spawnData.Length - 1 );

        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            SpawnEnemy();
        }

        if (!bossSpawned && GameManager.instance.maxGameTime - GameManager.instance.gameTime <= 10f)
        {
            bossSpawned = true;
            SpawnBoss();
        }
    }
    void SpawnEnemy()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);

        enemy.transform.localScale = Vector3.one;
    }

    void SpawnBoss()
    {
        if (bossSpriteType >= spawnData.Length)
        {
            Debug.LogError($"bossSpriteType ({bossSpriteType}) > size of spawnData ({spawnData.Length})");
            return;
        }

        SpawnData baseBossData = spawnData[bossSpriteType];

        SpawnData bossData = new SpawnData
        {
            spriteType = bossSpriteType,
            health = baseBossData.health * 10,
            speed = baseBossData.speed * 2,
            spawnTime = 0f,

            attackType = baseBossData.attackType,
            meleeDamage = baseBossData.meleeDamage * 3,
            rangedDamage = baseBossData.rangedDamage * 3,
            meleeCooldown = baseBossData.meleeCooldown * 0.7f,
            rangedCooldown = baseBossData.rangedCooldown * 0.7f,
            rangedBulletPrefab = baseBossData.rangedBulletPrefab
        };

        GameObject boss = GameManager.instance.pool.Get(0);
        boss.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        boss.transform.localScale = Vector3.one * 3f;
        boss.GetComponent<Enemy>().Init(bossData);
    }
}


public enum AttackType
{
    Melee,
    Ranged
}
[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float spawnTime;
    public int health;
    public float speed;

    public AttackType attackType;
    public float meleeDamage;
    public float rangedDamage;
    public float meleeCooldown;
    public float rangedCooldown;
    public GameObject rangedBulletPrefab;
}
