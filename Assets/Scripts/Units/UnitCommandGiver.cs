using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour {
  [SerializeField] LayerMask layerMask = new LayerMask();
  [SerializeField] UnitSelectionHandler unitSelectionHandler;

  Camera mainCamera;

  void Start() {
    mainCamera = Camera.main;
    GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
  }
  private void OnDestroy() {
    GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
  }
  private void Update() {
    // WAS BUTTON PRESSED?
    if (!Mouse.current.rightButton.wasPressedThisFrame) return;

    // WHAT WAS PRESSED?
    if (!MUtils.WasAHit(out RaycastHit hit, layerMask)) return;

    // CLICKED ON SOMETHING TARGETABLE
    if (hit.collider.TryGetComponent<Targetable>(out Targetable target)) {
      // ATTACK - not our own
      if (!target.hasAuthority) {
        TryTarget(target);
        return;
      }
    }

    // TRY TO MOVE
    TryMove(hit.point);
  }

  private void TryMove(Vector3 point) {
    foreach (var unit in unitSelectionHandler.SelectedUnits) {
      unit.GetUnitMovement().CmdMove(point);
    }
  }
  private void TryTarget(Targetable target) {
    foreach (var unit in unitSelectionHandler.SelectedUnits) {
      unit.GetTargeter().CmdSetTarget(target.gameObject);
    }
  }
  private void ClientHandleGameOver(string winnerName) {
    enabled = false; // no more Update()
  }


}
