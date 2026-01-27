using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { Dueling, PlayerAdvancing, EnemyAdvancing }
    private GameState currentState;
    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    public CinemachineCamera virtualCamera;
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;
    public float respawnDelay = 2f;
    public Transform playerInitialSpawn;
    public Transform opponentInitialSpawn;
    private List<Transform> spawnPoints = new List<Transform>();

    private GameObject playerInstance;
    private GameObject opponentInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (var spo in spawnPointObjects)
        {
            spawnPoints.Add(spo.transform);
        }

        StartNewRound(playerInitialSpawn.position, opponentInitialSpawn.position);
    }

    void Update()
    {
        if (playerInstance == null || opponentInstance == null)
            return;

        Vector3 targetPosition = cameraTarget.position;

        switch (currentState)
        {
            case GameState.Dueling:
                if (playerInstance.activeSelf && opponentInstance.activeSelf)
                {
                    targetPosition.x = (playerInstance.transform.position.x + opponentInstance.transform.position.x) / 2;
                }
                break;

            case GameState.PlayerAdvancing:
                targetPosition.x = Mathf.Max(targetPosition.x, playerInstance.transform.position.x);
                break;

            case GameState.EnemyAdvancing:
                targetPosition.x = Mathf.Min(targetPosition.x, opponentInstance.transform.position.x);
                break;
        }

        cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
    }

    void StartNewRound(Vector3 playerPos, Vector3 opponentPos)
    {
        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        else
            playerInstance.transform.position = playerPos;

        if (opponentInstance == null)
            opponentInstance = Instantiate(opponentPrefab, opponentPos, Quaternion.identity);
        else
            opponentInstance.transform.position = opponentPos;

        playerInstance.GetComponent<DamageBox>().ResetCharacter();
        opponentInstance.GetComponent<DamageBox>().ResetCharacter();

        opponentInstance.GetComponent<EnemyAI>().player = playerInstance.transform;

        playerInstance.GetComponent<PlayerMovement>().enabled = true;
        opponentInstance.GetComponent<EnemyAI>().SetAIActive(true);
        currentState = GameState.Dueling;
    }

    void SetGameState(GameState newState)
    {
        currentState = newState;
    }

    public void OnCharacterDied(GameObject character)
    {
        if (character.CompareTag("Player"))
        {
            character.GetComponent<PlayerMovement>().enabled = false;
            SetGameState(GameState.EnemyAdvancing);
            StartCoroutine(Respawn(playerInstance, opponentInstance));
        }
        else if (character.CompareTag("Enemy"))
        {
            SetGameState(GameState.PlayerAdvancing);
            StartCoroutine(Respawn(opponentInstance, playerInstance));
        }
    }

    private IEnumerator Respawn(GameObject loser, GameObject winner)
    {
        yield return new WaitForSeconds(respawnDelay);
        loser.GetComponent<DamageBox>().ResetCharacter();
        Transform respawnPoint = FindBestSpawnPoint(winner);

        if (respawnPoint == null)
        {
            Debug.LogError("Nem található megfelelõ spawn pont a gyõztes elõtt!");
            StartNewRound(playerInitialSpawn.position, opponentInitialSpawn.position);
            yield break;
        }

        if (loser.CompareTag("Player"))
        {
            StartNewRound(respawnPoint.position, winner.transform.position);
        }
        else
        {
            StartNewRound(winner.transform.position, respawnPoint.position);
        }
    }

    private Transform FindBestSpawnPoint(GameObject winner)
    {
        Vector3 winnerPos = winner.transform.position;
        List<Transform> candidatePoints = new List<Transform>();

        if (winner.CompareTag("Player"))
        {
            foreach (var point in spawnPoints)
            {
                if (point.position.x > winnerPos.x) candidatePoints.Add(point);
            }
            return candidatePoints.OrderBy(p => p.position.x).FirstOrDefault();
        }
        else
        {
            foreach (var point in spawnPoints)
            {
                if (point.position.x < winnerPos.x) candidatePoints.Add(point);
            }
            return candidatePoints.OrderByDescending(p => p.position.x).FirstOrDefault();
        }
    }
}
