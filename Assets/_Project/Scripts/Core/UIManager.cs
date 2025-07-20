using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject joySticks;
    [SerializeField] private PopupBase commonPopup;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
        SetActiveLoadingScreen(true);
        SetActiveOnScreenJoyStick(false);
        commonPopup.gameObject.SetActive(false);
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
    public void ShowCommonPopup(string title, string content, string leftbtnText, string rightBtnText,  Action onLeftButtonPressed, Action onRightButtonPressed, bool autoHide = true)
    {
        commonPopup.SetData(title, content, leftbtnText, rightBtnText, onLeftButtonPressed, onRightButtonPressed, autoHide);
        commonPopup.gameObject.SetActive(true);
    }
    public void ResetUI()
    {
        commonPopup.gameObject.SetActive(false);
        SetActiveOnScreenJoyStick(false);
        UpdateLoadingBar(0f);
    }
}
