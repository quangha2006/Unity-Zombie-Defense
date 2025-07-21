using UnityEngine;


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
        var localMoveDirection = Vector3.zero;
        if (playerController.canMove)
        {
            var moveInput = InputManager.Instance.GetMoveInput();
            var shootInput = InputManager.Instance.GetShootDirection();

            var moveInputDirection = new Vector3(moveInput.x, 0, moveInput.y);
            var shootInputDirection = new Vector3(shootInput.x, 0, shootInput.y);

            var hasShootDirection = shootInput.sqrMagnitude > 0.01f;
            var hasMoveInput = moveInputDirection.sqrMagnitude > 0.01f;

            var cameraForward = cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            var cameraRight = cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();



            if (hasMoveInput)
            {
                var moveDirection = (cameraForward * moveInputDirection.z) + (cameraRight * moveInputDirection.x);

                moveDirection.Normalize();

                controller.SimpleMove(moveDirection * moveSpeed);

                if (!hasShootDirection && !playerController.forceKeepRotation)
                {
                    var targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                localMoveDirection = transform.InverseTransformDirection(moveDirection);

            }
            else
            {
                controller.SimpleMove(Vector3.zero);
            }
            if (hasShootDirection && !playerController.forceKeepRotation)
            {
                var lookDirection = (cameraForward * shootInputDirection.z) + (cameraRight * shootInputDirection.x);
                lookDirection.Normalize();
                var targetRotation = Quaternion.LookRotation(lookDirection);
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
