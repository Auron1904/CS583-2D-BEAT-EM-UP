using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private Health playerHealth;
    [SerializeField] private string nextSceneName = "Scene2";
    [SerializeField] private float nextSceneDelay = 1.5f;
    [SerializeField] private GameObject gameOverPanel;   // added in Phase 5
    [SerializeField] private AudioClip clearSfx;
    [SerializeField] private AudioClip gameOverSfx;

    private int enemiesAlive;
    private int enemiesPending;
    private bool levelOver;

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    void Start()
    {
        playerHealth.Died += OnPlayerDied;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Enemy tracking ----------

    public void ExpectEnemies(int count)
    {
        enemiesPending += count;
    }

    public void RegisterEnemy(Health enemy)
    {
        if (enemiesPending > 0) enemiesPending--;
        enemiesAlive++;
        enemy.Died += OnEnemyDied;
    }

    private void OnEnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"Enemy down. Alive: {enemiesAlive}, still coming: {enemiesPending}");

        if (enemiesAlive <= 0 && enemiesPending <= 0 && !levelOver)
        {
            levelOver = true;
            SFX.Play(clearSfx);
            StartCoroutine(LoadAfterDelay(nextSceneName));
        }
    }

    // ---------- Player death ----------

    private void OnPlayerDied()
    {
        if (levelOver) return;
        levelOver = true;
        SFX.Play(gameOverSfx);

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        else StartCoroutine(LoadAfterDelay(SceneManager.GetActiveScene().name));  // no UI yet: auto-restart
    }

    void Update()
    {
        // Waiting on the Game Over screen for R or Enter.
        if (gameOverPanel == null || !gameOverPanel.activeSelf) return;
        Keyboard kb = Keyboard.current;
        if (kb != null && (kb.rKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
            RestartScene();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator LoadAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(nextSceneDelay);
        SceneManager.LoadScene(sceneName);
    }
}