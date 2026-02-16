using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState
    {
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
    public float playerScreenMargin = 2.5f;
    public Transform playerInitialSpawn;
    public Transform opponentInitialSpawn;
    private List<Transform> spawnPoints = new List<Transform>();
    public int spawnIndexOffset = 1;
    public TextMeshProUGUI countdownText;
    private GameObject playerInstance;
    private GameObject opponentInstance;
    private Vector3 lockedDuelPos;
    [Header("Victory Screen")]
    public GameObject victoryPanel;
    public TextMeshProUGUI winnerText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (var sp in spawnPointObjects) { spawnPoints.Add(sp.transform); }

        StartCoroutine(StartGameSequence());
    }

    void Update()
    {
        if (playerInstance == null || opponentInstance == null) return;

        float camHeight = virtualCamera.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        Vector3 targetPosition = cameraTarget.position;

        switch (currentState)
        {
            case GameState.Dueling:
                targetPosition = lockedDuelPos;

                if (!leftWall.gameObject.activeSelf) leftWall.gameObject.SetActive(true);
                if (!rightWall.gameObject.activeSelf) rightWall.gameObject.SetActive(true);
                break;

            case GameState.PlayerAdvancing:
                float p1TargetX = playerInstance.transform.position.x + camWidth - playerScreenMargin;
                targetPosition.x = Mathf.Max(targetPosition.x, p1TargetX);

                leftWall.gameObject.SetActive(true);
                rightWall.gameObject.SetActive(false);
                break;

            case GameState.EnemyAdvancing:
                float p2TargetX = opponentInstance.transform.position.x - camWidth + playerScreenMargin;
                targetPosition.x = Mathf.Min(targetPosition.x, p2TargetX);

                leftWall.gameObject.SetActive(false);
                rightWall.gameObject.SetActive(true);
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
        Vector3 camPos = Camera.main.transform.position;
        leftWall.position = new Vector3(camPos.x - camWidth, 0, 0);
        rightWall.position = new Vector3(camPos.x + camWidth, 0, 0);
    }

    IEnumerator StartGameSequence()
    {
        CreateCharacters(playerInitialSpawn.position, opponentInitialSpawn.position);
        float startMid = (playerInitialSpawn.position.x + opponentInitialSpawn.position.x) / 2f;
        lockedDuelPos = new Vector3(startMid, cameraTarget.position.y, 0);
        cameraTarget.position = lockedDuelPos;
        virtualCamera.ForceCameraPosition(lockedDuelPos, Quaternion.identity);
        playerInstance.GetComponent<PlayerMovement>().enabled = false;
        var p2Move = opponentInstance.GetComponent<PlayerMovement>();
        if (p2Move != null) p2Move.enabled = false;

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
        float midX = (playerInstance.transform.position.x + opponentInstance.transform.position.x) / 2f;
        lockedDuelPos = new Vector3(midX, cameraTarget.position.y, 0);

        var ai = opponentInstance.GetComponent<EnemyAI>();
        if (ai != null) ai.player = playerInstance.transform;

        SetGameState(GameState.Dueling);
    }

    void CreateCharacters(Vector3 playerPos, Vector3 opponentPos)
    {
        if (playerInstance == null) playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        else
        {
            playerInstance.transform.position = playerPos;
            playerInstance.SetActive(true);
        }

        if (opponentInstance == null) opponentInstance = Instantiate(opponentPrefab, opponentPos, Quaternion.identity);
        else
        {
            opponentInstance.transform.position = opponentPos;
            opponentInstance.SetActive(true);
        }

        playerInstance.GetComponentInChildren<WeaponManager>().EquipWeaponByIndex(0);
        opponentInstance.GetComponentInChildren<WeaponManager>().EquipWeaponByIndex(0);

        playerInstance.GetComponentInChildren<KnightControl>().idle();
        opponentInstance.GetComponentInChildren<KnightControl>().idle();

        playerInstance.GetComponent<DamageBox>().ResetCharacter();
        opponentInstance.GetComponent<DamageBox>().ResetCharacter();
    }

    void SetGameState(GameState newState)
    {
        currentState = newState;
        playerInstance.GetComponent<PlayerMovement>().enabled = (currentState != GameState.EnemyAdvancing);
        if (opponentInstance.GetComponent<PlayerMovement>() != null)
            opponentInstance.GetComponent<PlayerMovement>().enabled = (currentState != GameState.PlayerAdvancing);

        var ai = opponentInstance.GetComponent<EnemyAI>();
        if (ai != null)
        {
            if (currentState == GameState.Dueling) ai.StartDueling(playerInstance.transform);
            else if (currentState == GameState.EnemyAdvancing) ai.StartAdvancing();
            else ai.DeactivateAI();
        }
    }

    public void OnCharacterDied(GameObject character)
    {
        if (character.CompareTag("Player"))
        {
            SetGameState(GameState.EnemyAdvancing);
            StartCoroutine(Respawn(playerInstance, opponentInstance));
        }
        else
        {
            SetGameState(GameState.PlayerAdvancing);
            StartCoroutine(Respawn(opponentInstance, playerInstance));
        }
    }

    private IEnumerator Respawn(GameObject loser, GameObject winner)
    {
        yield return new WaitForSeconds(respawnDelay);

        loser.SetActive(false);

        var wm = loser.GetComponentInChildren<WeaponManager>();
        if (wm != null) wm.SwitchToNextWeapon();

        loser.GetComponent<DamageBox>().ResetCharacter();

        Transform respawnPoint = FindBestSpawnPoint(winner);
        if (respawnPoint != null)
        {
            loser.transform.position = respawnPoint.position;
        }
        else
        {
            float offset = loser.CompareTag("Player") ? -3f : 3f;
            loser.transform.position = new Vector3(cameraTarget.position.x + offset, loser.transform.position.y, 0);
        }

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

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (winnerTag == "Player" || winnerTag == "Player1")
            {
                winnerText.text = "PLAYER 1 WINS!";
                winnerText.color = Color.cyan;
            }
            else
            {
                winnerText.text = "PLAYER 2 WINS!";
                winnerText.color = Color.red;
            }
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
