using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private PlayerInput playerInput;
    [SerializeField] private PauseMenuController pauseMenuController;
    private Character character;
    private FreeLook freeLook;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        character = GetComponent<Character>();
        freeLook = GetComponent<FreeLook>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnInteract()
    {
        Debug.Log("interact");
        character.Interact();
    }

    private void OnAttack()
    {
        Debug.Log("Attack");
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;
        character.Attack(mousePosition);
    }

    private void OnSwitchWeapon()
    {
        character.SwitchWeapon();
    }

    private void OnMove(InputValue value)
    {
        character.SetMoveDirection(value.Get<float>());
    }

    private void OnJump()
    {
        character.Jump();
    }

    private void OnCrouch(InputValue value)
    {

    }

    private void OnClimb(InputValue value)
    {
        character.SetClimbDirection(value.Get<float>());
    }

    private void OnPauseGame()
    {
        playerInput.SwitchCurrentActionMap("UI");
        pauseMenuController.PauseGame();
    }

    private void OnToggleFreeLook()
    {
        Debug.Log("ToggleFreeLook");
        freeLook.ToggleFreeLook();
    }

    private void OnMoveCamera(InputValue value)
    {
        freeLook.MoveCamera(-value.Get<Vector2>());
    }
}
