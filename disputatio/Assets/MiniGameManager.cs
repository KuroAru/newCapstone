using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("Game Settings")]
    public float gameDuration = 60f;
    public int maxHealth = 5;
    public Vector2 mapSize = new Vector2(5000, 3000); // 전체 맵 사이즈 [cite: 64]
    
    [Header("Spawn Points")]
    public Vector3 playerSpawnPos = new Vector3(500, 1500, 0); 

    [Header("Prefabs (Assign in Inspector)")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;
    public GameObject exitPrefab;

    private GameObject playerInstance; // 소환된 플레이어 추적용
    private int currentHealth;
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        
        // 1. 플레이어 소환
        SpawnPlayer();
        
        SpawnExit();
        
        // 3. 적 스폰 시작
        StartCoroutine(EnemySpawner());
    }

    // 플레이어 소환 로직 추가
    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
            Debug.Log($"플레이어가 {playerSpawnPos} 위치에 소환되었습니다.");
        }
        else
        {
            Debug.LogError("Player Prefab이 할당되지 않았습니다!");
        }
    }

    void SpawnExit()
    {
        Vector3 exitPos = new Vector3(4500, 1500, 0);
        Instantiate(exitPrefab, exitPos, Quaternion.identity);
    }

    IEnumerator EnemySpawner()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(2f);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // 적들이 플레이어 주변이나 랜덤한 위치에서 생성됨
        Vector3 spawnPos = new Vector3(Random.Range(1000, 4000), Random.Range(500, 2500), 0);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over! Reloading...");

    }

    public void Win()
    {
        isGameOver = true;
        Debug.Log("Escaped! Transitioning to 1st Person...");
    }
}