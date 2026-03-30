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

    public TextMeshProUGUI countdownText;
    private GameObject playerInstance;
    private GameObject opponentInstance;
    private Vector3 lockedDuelPos;

    public GameObject victoryPanel;
    public TextMeshProUGUI winnerText;

    public GameObject p1Arrow;
    public GameObject p2Arrow;

    private List<GameObject> deadPlayers = new List<GameObject>();
    private Coroutine respawnCoroutine;
    public float wallSafeMargin = 1.5f;

    public bool isTrainingMode = false;

    public GameObject progressBar;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); }
        else { Instance = this; }
    }

    void Start()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (var sp in spawnPointObjects) { spawnPoints.Add(sp.transform); }
        ProgressBar pb = FindFirstObjectByType<ProgressBar>();
        if (pb != null)
        {
            pb.cameraTarget = this.cameraTarget;
            pb.leftLimit = GameObject.Find("OpponentGoal").transform;
            pb.rightLimit = GameObject.Find("PlayerGoal").transform;
        }
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
        PlayerMovement p2Move = opponentInstance.GetComponent<PlayerMovement>();
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

        EnemyAI ai = opponentInstance.GetComponent<EnemyAI>();
        if (ai != null) ai.player = playerInstance.transform;

        SetGameState(GameState.Dueling);
    }

    void CreateCharacters(Vector3 playerPos, Vector3 opponentPos)
    {
        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, playerPos, Quaternion.identity);
            DamageBox playerDb = playerInstance.GetComponent<DamageBox>();
            if (playerDb != null)
            {
                playerDb.OnDeath += () => OnCharacterDied(playerInstance);
            }
        }
        else
        {
            playerInstance.transform.position = playerPos;
            playerInstance.SetActive(true);
        }

        if (opponentInstance == null)
        {
            opponentInstance = Instantiate(opponentPrefab, opponentPos, Quaternion.identity);
            DamageBox enemyDb = opponentInstance.GetComponent<DamageBox>();
            if (enemyDb != null)
            {
                enemyDb.OnDeath += () => OnCharacterDied(opponentInstance);
            }
        }
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

        if (p1Arrow != null && p2Arrow != null)
        {
            p1Arrow.SetActive(false);
            p2Arrow.SetActive(false);

            if (currentState == GameState.PlayerAdvancing)
            {
                p1Arrow.SetActive(true);
            }
            else if (currentState == GameState.EnemyAdvancing)
            {
                p2Arrow.SetActive(true);
            }
        }

        EnemyAI ai = opponentInstance.GetComponent<EnemyAI>();
        if (ai != null)
        {
            if (currentState == GameState.Dueling) ai.StartDueling(playerInstance.transform);
            else if (currentState == GameState.EnemyAdvancing) ai.StartAdvancing();
            else ai.DeactivateAI();
        }
    }

    public void OnCharacterDied(GameObject character)
    {
        if (!deadPlayers.Contains(character))
        {
            deadPlayers.Add(character);
        }

        if (deadPlayers.Count == 1)
        {
            if (character.CompareTag("Player"))
                SetGameState(GameState.EnemyAdvancing);
            else
                SetGameState(GameState.PlayerAdvancing);

            respawnCoroutine = StartCoroutine(RespawnSequence());
        }
        else if (deadPlayers.Count == 2)
        {
            SetGameState(GameState.Dueling);
        }
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (deadPlayers.Count == 2)
        {
            HandleDrawRespawn();
        }
        else if (deadPlayers.Count == 1)
        {
            HandleSingleRespawn(deadPlayers[0]);
        }

        deadPlayers.Clear();
        StartDuel();
    }

    private void HandleSingleRespawn(GameObject loser)
    {
        bool isPlayer1 = loser.CompareTag("Player");
        GameObject winner = isPlayer1 ? opponentInstance : playerInstance;

        ResetPlayerState(loser);

        float camHeight = virtualCamera.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        float camX = Camera.main.transform.position.x;
        float safeLeftBound = (camX - camWidth) + wallSafeMargin;
        float safeRightBound = (camX + camWidth) - wallSafeMargin;
        List<Transform> visibleSpawns = spawnPoints.Where(sp => sp.position.x >= safeLeftBound && sp.position.x <= safeRightBound).ToList();

        Transform selectedSpawn = null;

        if (visibleSpawns.Count > 0)
        {
            float zoneWidth = (safeRightBound - safeLeftBound) / 3f;
            float minX, maxX;

            if (isPlayer1)
            {
                minX = safeLeftBound;
                maxX = safeLeftBound + zoneWidth;
            }
            else
            {
                minX = safeRightBound - zoneWidth;
                maxX = safeRightBound;
            }

            List<Transform> preferredSpawns = visibleSpawns.Where(sp => sp.position.x >= minX && sp.position.x <= maxX).ToList();
            if (preferredSpawns.Count > 0)
            {
                selectedSpawn = preferredSpawns[Random.Range(0, preferredSpawns.Count)];
            }
            else
            {
                if (isPlayer1)
                {
                    selectedSpawn = visibleSpawns.OrderBy(sp => sp.position.x).First();
                }
                else
                {
                    selectedSpawn = visibleSpawns.OrderByDescending(sp => sp.position.x).First();
                }
            }
        }

        if (selectedSpawn != null)
        {
            loser.transform.position = selectedSpawn.position;
            loser.SetActive(true);
        }
    }

    private void HandleDrawRespawn()
    {
        float camHeight = virtualCamera.Lens.OrthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        float camX = Camera.main.transform.position.x;
        float safeLeftBound = (camX - camWidth) + wallSafeMargin;
        float safeRightBound = (camX + camWidth) - wallSafeMargin;
        List<Transform> visibleSpawns = spawnPoints.Where(sp => sp.position.x >= safeLeftBound && sp.position.x <= safeRightBound).OrderBy(sp => sp.position.x).ToList();

        ResetPlayerState(playerInstance);
        ResetPlayerState(opponentInstance);

        if (visibleSpawns.Count >= 2)
        {
            playerInstance.transform.position = visibleSpawns.First().position;
            opponentInstance.transform.position = visibleSpawns.Last().position;

            playerInstance.SetActive(true);
            opponentInstance.SetActive(true);
        }
        else
        {
            playerInstance.transform.position = new Vector3(safeLeftBound, playerInstance.transform.position.y, 0);
            opponentInstance.transform.position = new Vector3(safeRightBound, opponentInstance.transform.position.y, 0);

            playerInstance.SetActive(true);
            opponentInstance.SetActive(true);
        }
    }

    private void ResetPlayerState(GameObject character)
    {
        character.SetActive(false);
        WeaponManager wm = character.GetComponentInChildren<WeaponManager>();
        if (wm != null) wm.SwitchToNextWeapon();

        DamageBox damageBox = character.GetComponent<DamageBox>();
        if (damageBox != null) damageBox.ResetCharacter();

        CharacterBase charBase = character.GetComponent<CharacterBase>();
        if (charBase != null) charBase.ResetState();
    }

    public void EndGame(string winnerTag)
    {
        if (isTrainingMode)
        {
            RestartGame();
            return;
        }

        Time.timeScale = 0f;
        if (p1Arrow != null) p1Arrow.SetActive(false);
        if (p2Arrow != null) p2Arrow.SetActive(false);
        if (progressBar != null) progressBar.SetActive(false);

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

            StartCoroutine(FadeInVictoryPanel());
        }
    }

    private IEnumerator FadeInVictoryPanel()
    {
        CanvasGroup cg = victoryPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        float fadeDuration = 2.0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);

            yield return null;
        }
        cg.alpha = 1f;
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
