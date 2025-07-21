using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    private const string LobbySceneName = "LobbyScene";

    [SerializeField] private float firstLoadingTime;
    [SerializeField] private string[] levelName;

    private bool isLoadToLobby = false;
    private float loadingTimer;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        loadingTimer = firstLoadingTime;
    }
    private void Update()
    {
        if (loadingTimer >= 0f)
        {
            loadingTimer -= Time.deltaTime;
            UIManager.Instance.UpdateLoadingBar((1f - (loadingTimer / firstLoadingTime))/10f);
        }

        if (loadingTimer <= 0f && !isLoadToLobby)
        {
            isLoadToLobby = true;
            SceneManager.LoadScene(LobbySceneName);
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        
    }

    private void OnSceneUnloaded(Scene scene)
    {
        UIManager.Instance.ResetUI();
    }

    public bool LoadMatchLevel(int level, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (level <= 0 || level > levelName.Length)
        {
            errorMessage = "Level not found.";
            return false;
        }
        var sceneName = levelName[level - 1];
        UIManager.Instance.UpdateLoadingBar(0f);
        UIManager.Instance.SetActiveLoadingScreen(true);
        SceneManager.LoadScene(sceneName);
        return true;
    }

    public void LoadLobbyScene()
    {
        UIManager.Instance.UpdateLoadingBar(0f);
        UIManager.Instance.SetActiveLoadingScreen(true);
        SceneManager.LoadScene(LobbySceneName);
    }
}
