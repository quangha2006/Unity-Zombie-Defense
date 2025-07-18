using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoaderEnforcer : MonoBehaviour
{
    [SerializeField] private string bootLoaderSceneName = "BootLoaderScene";

    void Awake()
    {
        if (GameManager.Instance == null && SceneManager.GetActiveScene().name != bootLoaderSceneName)
        {
            Debug.LogWarning("Force loading BootLoaderScene...");
            SceneManager.LoadScene(bootLoaderSceneName);
        }
    }
}
