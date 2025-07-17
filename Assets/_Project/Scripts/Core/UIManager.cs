using UnityEditor.ShaderGraph;
using UnityEngine;

public class UIManager : MonoBehaviourSingleton<UIManager>
{
    [SerializeField] private InputManager inputManager;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(Instance);
    }
    private void Update()
    {
        Vector2 moveInput = inputManager.GetMoveInput();
        //if (moveInput != Vector2.zero)
        //{
        //    Debug.Log("Input: " + moveInput);
        //}
    }
}
