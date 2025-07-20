using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupBase : MonoBehaviour
{
    [SerializeField] private TMP_Text titlePopup;
    [SerializeField] private TMP_Text contentPopup;
    [SerializeField] private TMP_Text leftButtonText;
    [SerializeField] private TMP_Text rightButtonText;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private Action onLeftPressed;
    private Action onRightPressed;
    private bool isHideAfterPress = true;
    private void Start()
    {
        leftButton.onClick.AddListener(OnLeftButtonPressed);
        rightButton.onClick.AddListener(OnRightButtonPressed);
    }

    public void SetData(string title, string content, string leftbtnText, string rightBtnText, Action onLeftButtonPressed, Action onRightButtonPressed, bool autoHide = true)
    {
        titlePopup.text = title;
        contentPopup.text = content;
        leftButtonText.text = leftbtnText;
        rightButtonText.text = rightBtnText;

        onLeftPressed = onLeftButtonPressed;
        onRightPressed = onRightButtonPressed;
        isHideAfterPress = autoHide;
    }

    private void OnLeftButtonPressed()
    {
        onLeftPressed?.Invoke();
        if (isHideAfterPress)
            gameObject.SetActive(false);
    }
    private void OnRightButtonPressed()
    {
        onRightPressed?.Invoke();
        if (isHideAfterPress)
            gameObject.SetActive(false);
    }
}
