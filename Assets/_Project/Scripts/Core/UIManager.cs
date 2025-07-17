using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject joySticks;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
        SetActiveLoadingScreen(true);
        SetActiveOnScreenJoyStick(false);
    }

    public void SetActiveLoadingScreen(bool isShow)
    {
        loadingScreen.SetActive(isShow);
    }
    public void UpdateLoadingBar(float percent)
    {
        loadingBar.value = percent;
    }
    public void SetActiveOnScreenJoyStick(bool isShow)
    {
        joySticks.SetActive(isShow);
    }
}
