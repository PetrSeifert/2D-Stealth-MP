using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLook : MonoBehaviour
{
    private Camera mainCamera;
    private Cinemachine.CinemachineBrain cinemachineBrain;
    private PlayerInput playerInput;
    private Vector2 tempCameraPosition;

    private void Awake()
    {
        mainCamera = Camera.main;
        cinemachineBrain = mainCamera.GetComponent<Cinemachine.CinemachineBrain>();
        playerInput = GetComponent<PlayerInput>();
    }

    public void ToggleFreeLook()
    {
        cinemachineBrain.enabled = !cinemachineBrain.enabled;
        if (cinemachineBrain.enabled)
        {
            playerInput.SwitchCurrentActionMap("Player");
            mainCamera.transform.position = tempCameraPosition;
        } 
        else
        {
            playerInput.SwitchCurrentActionMap("FreeLook");
            tempCameraPosition = mainCamera.transform.position;
        } 
    }

    public void MoveCamera(Vector3 moveVector)
    {
        mainCamera.transform.position += moveVector/3;
    }
}
