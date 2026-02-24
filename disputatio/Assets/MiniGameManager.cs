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
    public Vector2 mapSize = new Vector2(5000, 3000);
    
    [Header("Prefabs (Assign in Inspector)")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject projectilePrefab;
    public GameObject exitPrefab;

    private int currentHealth;
    private bool isGameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        SpawnExit();
        StartCoroutine(EnemySpawner());
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
        // Spawn enemies around player or at random edges
        Vector3 spawnPos = new Vector3(Random.Range(1000, 4000), Random.Range(500, 2500), 0);
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    public void TakeDamage()
    {
        currentHealth--;
        // Trigger red screen UI effect
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over! Reloading...");
        // Logic to reload last save
    }

    public void Win()
    {
        isGameOver = true;
        Debug.Log("Escaped! Transitioning to 1st Person...");
        // Fade out and load next scene
    }
}
