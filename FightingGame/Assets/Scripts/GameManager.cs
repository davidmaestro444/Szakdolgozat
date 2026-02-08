using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { 
        Dueling, 
        PlayerAdvancing, 
        EnemyAdvancing 
    }

    private GameState currentState;
    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    public CinemachineCamera virtualCamera;
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;
    public Transform leftWall;
    public Transform rightWall;
    public float respawnDelay = 3f;
    public Transform playerInitialSpawn;
    public Transform opponentInitialSpawn;
    private List<Transform> spawnPoints = new List<Transform>();
    private GameObject playerInstance;
    private GameObject opponentInstance;
    public int spawnIndexOffset = 1;
    public TextMeshProUGUI countdownText;

    private Vector3 lockedCameraPos;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (var sp in spawnPointObjects) { spawnPoints.Add(sp.transform); }
        UpdateWallPositions();

        StartCoroutine(StartGameSequence());
    }

    void Update()
    {
        if (playerInstance == null || opponentInstance == null) return;

        float camHeight = virtualCamera.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        float margin = 1.5f;

        Vector3 targetPosition = cameraTarget.position;

        switch (currentState)
        {
            case GameState.Dueling:
                targetPosition.x = lockedCameraPos.x;
                break;

            case GameState.PlayerAdvancing:
                float p1LeadingPos = playerInstance.transform.position.x + camWidth - margin;
                targetPosition.x = Mathf.Max(targetPosition.x, p1LeadingPos);
                break;

            case GameState.EnemyAdvancing:
                float enemyLeadingPos = opponentInstance.transform.position.x - camWidth + margin;
                targetPosition.x = Mathf.Min(targetPosition.x, enemyLeadingPos);
                break;
        }
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
        UpdateWallPositions();
    }

    void UpdateWallPositions()
    {
        if (virtualCamera == null || leftWall == null || rightWall == null) return;

        float camHeight = virtualCamera.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        leftWall.localPosition = new Vector3(-camWidth, 0, 0);
        rightWall.localPosition = new Vector3(camWidth, 0, 0);
    }

    IEnumerator StartGameSequence()
    {
        CreateCharacters(playerInitialSpawn.position, opponentInitialSpawn.position);
        lockedCameraPos = (playerInitialSpawn.position + opponentInitialSpawn.position) / 2;
        cameraTarget.position = lockedCameraPos;
        playerInstance.GetComponent<PlayerMovement>().enabled = false;
        var ai = opponentInstance.GetComponent<EnemyAI>();
        var p2Movement = opponentInstance.GetComponent<PlayerMovement>();
        if (ai != null) ai.DeactivateAI();
        if (p2Movement != null) p2Movement.enabled = false;
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
        StartDuel();
    }

    void StartDuel()
    {
        playerInstance.GetComponent<DamageBox>().ResetCharacter();
        opponentInstance.GetComponent<DamageBox>().ResetCharacter();
        lockedCameraPos.x = (playerInstance.transform.position.x + opponentInstance.transform.position.x) / 2;
        var ai = opponentInstance.GetComponent<EnemyAI>();
        if (ai != null) ai.player = playerInstance.transform;
        SetGameState(GameState.Dueling);
    }

    void CreateCharacters(Vector3 playerPos, Vector3 opponentPos)
    {
        if (playerInstance == null) playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        else playerInstance.transform.position = playerPos;
        if (opponentInstance == null) opponentInstance = Instantiate(opponentPrefab, opponentPos, Quaternion.identity);
        else opponentInstance.transform.position = opponentPos;
        playerInstance.SetActive(true);
        opponentInstance.SetActive(true);

        var p1WM = playerInstance.GetComponentInChildren<WeaponManager>();
        if (p1WM != null) p1WM.RefreshWeapon();

        var p2WM = opponentInstance.GetComponentInChildren<WeaponManager>();
        if (p2WM != null) p2WM.RefreshWeapon();
        playerInstance.GetComponentInChildren<KnightControl>().idle();
        opponentInstance.GetComponentInChildren<KnightControl>().idle();
        playerInstance.GetComponent<DamageBox>().ResetCharacter();
        opponentInstance.GetComponent<DamageBox>().ResetCharacter();
    }

    void SetGameState(GameState newState)
    {
        currentState = newState;
        if (playerInstance == null || opponentInstance == null) return;
        var opponentAI = opponentInstance.GetComponent<EnemyAI>();
        var p2Movement = opponentInstance.GetComponent<PlayerMovement>();
        var playerMovement = playerInstance.GetComponent<PlayerMovement>();
        switch (currentState)
        {
            case GameState.Dueling:
                playerMovement.enabled = true;
                if (opponentAI != null) opponentAI.StartDueling(playerInstance.transform);
                if (p2Movement != null) p2Movement.enabled = true;
                break;
            case GameState.PlayerAdvancing:
                if (opponentAI != null) opponentAI.DeactivateAI();
                if (p2Movement != null) p2Movement.enabled = false;
                break;
            case GameState.EnemyAdvancing:
                playerMovement.enabled = false;
                if (opponentAI != null) opponentAI.StartAdvancing();
                if (p2Movement != null) p2Movement.enabled = true;
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
        if (respawnPoint == null) yield break;
        loser.transform.position = respawnPoint.position;
        loser.SetActive(true);
        StartDuel();
    }

    private Transform FindBestSpawnPoint(GameObject winner)
    {
        Vector3 winnerPos = winner.transform.position;
        List<Transform> candidatePoints = new List<Transform>();
        if (winner.CompareTag("Player"))
        {
            foreach (var point in spawnPoints) { if (point.position.x > winnerPos.x) candidatePoints.Add(point); }
            candidatePoints = candidatePoints.OrderBy(p => p.position.x).ToList();
        }
        else
        {
            foreach (var point in spawnPoints) { if (point.position.x < winnerPos.x) candidatePoints.Add(point); }
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
