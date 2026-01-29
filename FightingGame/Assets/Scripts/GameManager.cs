using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public float respawnDelay = 3f;
    public Transform playerInitialSpawn;
    public Transform opponentInitialSpawn;
    private List<Transform> spawnPoints = new List<Transform>();

    private GameObject playerInstance;
    private GameObject opponentInstance;
    public int spawnIndexOffset = 1;
    public TextMeshProUGUI countdownText;
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
        StartCoroutine(StartGameSequence());
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
    IEnumerator StartGameSequence()
    {
        CreateCharacters(playerInitialSpawn.position, opponentInitialSpawn.position);
        currentState = GameState.Dueling;
        cameraTarget.position = (playerInitialSpawn.position + opponentInitialSpawn.position) / 2;
        playerInstance.GetComponent<PlayerMovement>().enabled = false;
        opponentInstance.GetComponent<EnemyAI>().DeactivateAI();
        countdownText.gameObject.SetActive(true);
        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }
        countdownText.text = "FIGHT!";
        yield return new WaitForSeconds(0.5f);

        countdownText.gameObject.SetActive(false);
        SetGameState(GameState.Dueling);
    }
    void CreateCharacters(Vector3 playerPos, Vector3 opponentPos)
    {
        if (playerInstance == null)
            playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        else
            playerInstance.transform.position = playerPos;

        if (opponentInstance == null)
            opponentInstance = Instantiate(opponentPrefab, opponentPos, Quaternion.identity);
        else
            opponentInstance.transform.position = opponentPos;

        playerInstance.SetActive(true);
        opponentInstance.SetActive(true);

        playerInstance.GetComponent<DamageBox>().ResetCharacter();
        opponentInstance.GetComponent<DamageBox>().ResetCharacter();

        opponentInstance.GetComponent<EnemyAI>().player = playerInstance.transform;
    }

    void StartNewRound(Vector3 playerPos, Vector3 opponentPos)
    {
        CreateCharacters(playerPos, opponentPos);
        SetGameState(GameState.Dueling);
    }

    void SetGameState(GameState newState)
    {
        currentState = newState;
        if (playerInstance == null || opponentInstance == null) return;

        var opponentAI = opponentInstance.GetComponent<EnemyAI>();
        var playerMovement = playerInstance.GetComponent<PlayerMovement>();

        switch (currentState)
        {
            case GameState.Dueling:
                playerMovement.enabled = true;
                opponentAI.StartDueling(playerInstance.transform);
                break;

            case GameState.PlayerAdvancing:
                opponentAI.DeactivateAI();
                break;

            case GameState.EnemyAdvancing:
                playerMovement.enabled = false;
                opponentAI.StartAdvancing();
                break;
        }
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
            candidatePoints = candidatePoints.OrderBy(p => p.position.x).ToList();
        }
        else
        {
            foreach (var point in spawnPoints)
            {
                if (point.position.x < winnerPos.x) candidatePoints.Add(point);
            }
            candidatePoints = candidatePoints.OrderByDescending(p => p.position.x).ToList();
        }

        if (candidatePoints.Count == 0) return null;
        int targetIndex = Mathf.Min(spawnIndexOffset, candidatePoints.Count - 1);

        return candidatePoints[targetIndex];
    }
    public void EndGame(string winnerTag)
    {
        Time.timeScale = 0f;
    }
}
