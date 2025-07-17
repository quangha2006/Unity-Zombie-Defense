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
            UIManager.Instance.UpdateLoadingBar(1f - (loadingTimer / firstLoadingTime));
        }

        if (loadingTimer < 0.5f && !isLoadToLobby)
        {
            isLoadToLobby = true;
            SceneManager.LoadScene("Level-01");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        
    }
}
