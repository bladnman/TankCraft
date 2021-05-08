using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour {

  [SerializeField] LayerMask layerMask = new LayerMask();
  [SerializeField] RectTransform unitSelectionArea;

  Camera mainCamera;
  RTSPlayer player;
  Vector2 startPosition;

  public List<Unit> SelectedUnits { get; } = new List<Unit>();

  private void Start() {
    mainCamera = Camera.main;
    Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
  }
  private void OnDestroy() {
    Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
  }
  private void Update() {
    if (player == null) {
      // todo: this causes a known error which will be addressed in a later lesson
      player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    // first press -- START NEW SELECTION
    if (Mouse.current.leftButton.wasPressedThisFrame) {
      StartSelectionArea();
    }

    // drag -- MOVING MOUSE
    else if (Mouse.current.leftButton.isPressed) {
      UpdateSelectionArea();
    }

    // release -- COMPLETE SELECTION
    else if (Mouse.current.leftButton.wasReleasedThisFrame) {
      CompleteSelectionArea();
    }
  }
  private void StartSelectionArea() {
    // CLEAR PREVIOUS SELECTION
    // unless LEFT-SHIFT is down
    if (!Keyboard.current.leftShiftKey.isPressed) {
      foreach (var unit in SelectedUnits) {
        unit.Deselect();
      }
      SelectedUnits.Clear();
    }

    unitSelectionArea.gameObject.SetActive(true);
    startPosition = Mouse.current.position.ReadValue();
    UpdateSelectionArea();
  }
  private void UpdateSelectionArea() {
    var mousePosition = Mouse.current.position.ReadValue();
    var width = mousePosition.x - startPosition.x;
    var height = mousePosition.y - startPosition.y;
    unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
    unitSelectionArea.anchoredPosition = startPosition + new Vector2(width / 2, height / 2);
  }
  private void CompleteSelectionArea() {
    unitSelectionArea.gameObject.SetActive(false);

    // SINGLE SELECT - no drag box size
    if (unitSelectionArea.sizeDelta.magnitude == 0) {
      CompleteSelection_Single();
    }

    // MULTISELECTION
    else {
      CompleteSelection_Multi();
    }

    unitSelectionArea.sizeDelta = new Vector2(0f, 0f);

  }
  void CompleteSelection_Single() {
    // WHAT WAS PRESSED?
    if (!MUtils.WasAHit(out RaycastHit hit, layerMask)) return;
    // was this a Unit?
    if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;
    // do we own this item?
    if (!unit.hasAuthority) return;

    // selected
    SelectedUnits.Add(unit);

    foreach (var selectedUnit in SelectedUnits) {
      selectedUnit.Select();
    }
  }
  void CompleteSelection_Multi() {
    Vector2 minVect = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
    Vector2 maxVect = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

    foreach (var unit in player.GetUnits()) {
      // aleady selected?
      if (SelectedUnits.Contains(unit)) continue;

      Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
      if (screenPosition.x > minVect.x &&
          screenPosition.x < maxVect.x &&
          screenPosition.y > minVect.y &&
          screenPosition.y < maxVect.y) {
        SelectedUnits.Add(unit);
        unit.Select();
      }
    }
  }
  private void AuthorityHandleUnitDespawned(Unit unit) {
    // when a unit despawns let's just make sure it is removed from
    // any selection list
    SelectedUnits.Remove(unit);
  }
  private void ClientHandleGameOver(string winnerName) {
    enabled = false; // will prevent Update() from being called
  }

}
