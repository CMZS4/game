using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Prefabs")]
    [SerializeField] private List<GameObject> levelPrefabs = new List<GameObject>();
    [SerializeField] private Transform levelRoot;

    [Header("Player")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform playerSpawn;

    private GameObject currentLevel;
    private int currentIndex;
    private int currentKeys;

    public int CurrentKeys => currentKeys;
    public int CurrentLevelIndex => currentIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadLevel(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            NextLevel();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PreviousLevel();
        }
    }

    public void CollectKey()
    {
        currentKeys++;
        UIController.Instance?.UpdateKeyText(currentKeys);
    }

    public void ResetLevel()
    {
        LoadLevel(currentIndex);
    }

    public void NextLevel()
    {
        if (levelPrefabs.Count == 0) return;
        int next = (currentIndex + 1) % levelPrefabs.Count;
        LoadLevel(next);
    }

    public void PreviousLevel()
    {
        if (levelPrefabs.Count == 0) return;
        int prev = (currentIndex - 1 + levelPrefabs.Count) % levelPrefabs.Count;
        LoadLevel(prev);
    }

    public void RespawnPlayer()
    {
        if (player == null) return;
        player.Respawn();
    }

    public void SetCheckpoint(Vector3 checkpoint)
    {
        if (player == null) return;
        player.SetCheckpoint(checkpoint);
    }

    private void LoadLevel(int index)
    {
        if (levelPrefabs.Count == 0) return;

        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        currentIndex = Mathf.Clamp(index, 0, levelPrefabs.Count - 1);
        currentLevel = Instantiate(levelPrefabs[currentIndex], levelRoot != null ? levelRoot : transform);

        currentKeys = 0;
        UIController.Instance?.UpdateKeyText(currentKeys);

        if (player != null)
        {
            Vector3 spawnPos = playerSpawn != null ? playerSpawn.position : Vector3.zero;
            player.transform.position = spawnPos;
            player.SetCheckpoint(spawnPos);
        }
    }
}
