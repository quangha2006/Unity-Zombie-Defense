using UI.Joystick;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviourSingleton<InputManager>
{
    [SerializeField] private Joystick moveJoystick;
    [SerializeField] private Joystick shootDirectionJoystick;

    private CharacterControlInputActions characterControlInput;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        characterControlInput = new CharacterControlInputActions();
    }
    public Vector2 GetMoveInput()
    {
        Vector2 joystick = moveJoystick.Direction;

        if (joystick.sqrMagnitude > 0.01f)
            return joystick;

        return characterControlInput.Gameplay.MoveDirection.ReadValue<Vector2>();
    }
    public Vector2 GetShootDirection()
    {
        Vector2 joystick = shootDirectionJoystick.Direction;

        if (joystick.sqrMagnitude > 0.01f)
            return joystick;

        return characterControlInput.Gameplay.ShootDirection.ReadValue<Vector2>();
    }

    public bool GetShootButton()
    {
        return shootDirectionJoystick.IsPressed;// || characterControlInput.Gameplay.Shoot.ReadValue<bool>();
    }

    void OnEnable()
    {
        characterControlInput.Gameplay.Enable();
    }

    void OnDisable()
    {
        characterControlInput?.Gameplay.Disable();
    }
}
