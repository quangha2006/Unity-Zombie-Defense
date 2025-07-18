using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    [SerializeField] private float firstLoadingTime;
    private bool isLoadToLobby = false;
    private float loadingTimer;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
    }
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
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
            SceneManager.LoadScene("LobbyScene");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        
    }
    public void StartBattle(string levelname)
    {
        UIManager.Instance.UpdateLoadingBar(0f);
        UIManager.Instance.SetActiveLoadingScreen(true);
        SceneManager.LoadScene(levelname);
    }
}
