using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private string levelName;
    private float currentProgress = 0.2f;
    private void Awake()
    {
        UIManager.Instance?.UpdateLoadingBar(0.1f);
    }
    void Start()
    {
        UIManager.Instance.UpdateLoadingBar(0.2f);
        startGameButton.onClick.AddListener(OnGameStartPressed);
    }
    private void Update()
    {
        if (currentProgress <= 1f)
        {
            currentProgress += Time.deltaTime;
        }
        UIManager.Instance.UpdateLoadingBar(currentProgress);
        if (currentProgress >= 1f)
        {
            UIManager.Instance.SetActiveLoadingScreen(false);
        }
    }
    private void OnGameStartPressed()
    {
        GameManager.Instance.StartBattle(levelName);
    }
}
