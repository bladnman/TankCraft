using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour {
  [SerializeField] Transform playerCameraTransform;
  [SerializeField] float speed = 20f;
  [SerializeField] float screenBoarderThickness = 10f;
  [SerializeField] Vector2 screenXLimits = Vector2.zero;
  [SerializeField] Vector2 screenZLimits = Vector2.zero;
  [SerializeField] bool enableMouseMoves = false;

  Controls controls;
  Vector2 previousInput;

  #region Server  -  -  -  -  -  -  -  -  -  

  #endregion

  #region Client  -  -  -  -  -  -  -  -  -  

  public override void OnStartAuthority() {
    playerCameraTransform.gameObject.SetActive(true);

    controls = new Controls();

    controls.Player.MoveCamera.performed += SetPreviousInput;
    controls.Player.MoveCamera.canceled += SetPreviousInput;

    controls.Enable();
  }
  [ClientCallback]
  private void Update() {
    if (!hasAuthority) return;
    if (!Application.isFocused) return;

    UpdateCameraPosition();
  }

  private void UpdateCameraPosition() {
    Vector3 pos = playerCameraTransform.position;

    // NO KEYBOARD - TRY MOUSE INPUT
    if (previousInput == Vector2.zero) {
      if (!enableMouseMoves) return;
      var cursorMovement = Vector3.zero;
      var cursorPosition = Mouse.current.position.ReadValue();

      if (cursorPosition.y >= Screen.height - screenBoarderThickness) {
        cursorMovement.z += 1;
      } else if (cursorPosition.y <= screenBoarderThickness) {
        cursorMovement.z -= 1;
      }

      if (cursorPosition.x >= Screen.width - screenBoarderThickness) {
        cursorMovement.x += 1;
      } else if (cursorPosition.x <= screenBoarderThickness) {
        cursorMovement.x -= 1;
      }

      pos += cursorMovement.normalized * speed * Time.deltaTime;
    }

    // KEYBOARD
    else {
      var kInput = new Vector3(previousInput.x, 0f, previousInput.y);
      pos += kInput * speed * Time.deltaTime;
    }

    // keep in-bounds
    pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
    pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

    playerCameraTransform.position = pos;

  }

  void SetPreviousInput(InputAction.CallbackContext ctx) {
    previousInput = ctx.ReadValue<Vector2>();
  }

  #endregion
}
