using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { Dueling, PlayerAdvancing, EnemyAdvancing }
    private GameState currentState;

    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public CinemachineCamera virtualCamera;

    public float respawnDelay = 3f;
    public Transform[] playerSpawnPoints;
    public Transform[] enemySpawnPoints;

    private GameObject playerInstance;
    private GameObject enemyInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start()
    {
        StartNewRound();
    }

    void StartNewRound()
    {
        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab, playerSpawnPoints[0].position, Quaternion.identity);
        else
            playerInstance.transform.position = playerSpawnPoints[0].position;

        if (enemyInstance == null)
            enemyInstance = Instantiate(enemyPrefab, enemySpawnPoints[0].position, Quaternion.identity);
        else
            enemyInstance.transform.position = enemySpawnPoints[0].position;

        playerInstance.SetActive(true);
        enemyInstance.SetActive(true);

        SetGameState(GameState.Dueling);
    }

    void SetGameState(GameState newState)
    {
        currentState = newState;
        Transform cameraFollowTarget = null;

        switch (currentState)
        {
            case GameState.Dueling:
                playerInstance.GetComponent<PlayerMovement>().enabled = true;
                enemyInstance.GetComponent<EnemyAI>().SetAIActive(true);
                cameraFollowTarget = null;
                break;
            case GameState.PlayerAdvancing:
                enemyInstance.GetComponent<EnemyAI>().SetAIActive(false);
                cameraFollowTarget = playerInstance.transform;
                break;
            case GameState.EnemyAdvancing:
                playerInstance.GetComponent<PlayerMovement>().enabled = false;
                cameraFollowTarget = enemyInstance.transform;
                break;
        }
        virtualCamera.Follow = cameraFollowTarget;
    }

    public void OnCharacterDied(GameObject character)
    {
        if (character == playerInstance)
        {
            Debug.Log("Játékos meghalt, az ellenség halad!");
            SetGameState(GameState.EnemyAdvancing);
            StartCoroutine(Respawn(playerInstance, enemyInstance));
        }
        else if (character == enemyInstance)
        {
            Debug.Log("Ellenség meghalt, a játékos halad!");
            SetGameState(GameState.PlayerAdvancing);
            StartCoroutine(Respawn(enemyInstance, playerInstance));
        }
    }

    private IEnumerator Respawn(GameObject loser, GameObject winner)
    {
        yield return new WaitForSeconds(respawnDelay);
        float respawnOffset = winner.transform.localScale.x > 0 ? 5f : -5f;
        Vector3 respawnPosition = winner.transform.position + new Vector3(respawnOffset, 2f, 0);

        loser.transform.position = respawnPosition;
        loser.SetActive(true);

        Debug.Log("Újraéledés! Vissza párbaj módba.");
        SetGameState(GameState.Dueling);
    }
}
