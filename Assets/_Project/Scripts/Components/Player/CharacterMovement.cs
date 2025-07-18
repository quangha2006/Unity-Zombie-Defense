using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float rotationSpeed = 10f;
    [SerializeField] public PlayerController playerController;
    [SerializeField] private bool isInLobby = false;

    private CharacterController controller;
    private Transform cameraTransform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        Vector3 localMoveDirection = Vector3.zero;
        if (playerController.gameReady)
        {
            Vector2 moveInput = InputManager.Instance.GetMoveInput();
            Vector2 shootInput = InputManager.Instance.GetShootDirection();

            Vector3 moveInputDirection = new Vector3(moveInput.x, 0, moveInput.y);
            Vector3 shootInputDirection = new Vector3(shootInput.x, 0, shootInput.y);

            var hasShootDirection = shootInput.sqrMagnitude > 0.01f;
            var hasMoveInput = moveInputDirection.sqrMagnitude > 0.01f;

            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();



            if (hasMoveInput)
            {
                Vector3 moveDirection = (cameraForward * moveInputDirection.z) + (cameraRight * moveInputDirection.x);

                moveDirection.Normalize();

                controller.SimpleMove(moveDirection * moveSpeed);

                if (!hasShootDirection)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                localMoveDirection = transform.InverseTransformDirection(moveDirection);

            }
            else
            {
                controller.SimpleMove(Vector3.zero);
            }
            if (hasShootDirection)
            {
                Vector3 lookDirection = (cameraForward * shootInputDirection.z) + (cameraRight * shootInputDirection.x);
                lookDirection.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        if (!isInLobby)
        {
            animator.SetFloat("MoveForward", localMoveDirection.z);
            animator.SetFloat("MoveRight", localMoveDirection.x);
        }
    }
}
