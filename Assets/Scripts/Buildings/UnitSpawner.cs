using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler {
  [SerializeField] Health health;
  [SerializeField] Unit unitPrefab;
  [SerializeField] Transform unitSpawnPoint;
  [SerializeField] TMP_Text remainingUnitsText;
  [SerializeField] Image unitProgressImage;
  [SerializeField] int maxUnitQueue = 5;
  [SerializeField] float spawnMoveRange = 7f;
  [SerializeField] float unitSpawnDuration = 5f;

  [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
  int queuedUnits;

  [SyncVar]
  float unitTimer;

  float progressImageVelocity;

  private void Update() {
    if (isServer) {
      ServerUpdateUnitProduction();
    }
    if (isClient) {
      ClientUpdateTimerDisplay();
    }
  }


  #region Server
  public override void OnStartServer() {
    health.ServerOnDie += ServerHandleDie;
  }
  public override void OnStopServer() {
    health.ServerOnDie -= ServerHandleDie;
  }
  [Server]
  private void ServerHandleDie() {
    NetworkServer.Destroy(gameObject);
  }
  [Command]
  void CmdSpawnUnit() {
    if (queuedUnits == maxUnitQueue) return;

    var player = connectionToClient.identity.GetComponent<RTSPlayer>();
    int resources = player.GetResources();

    if (resources < unitPrefab.GetResourceCost()) return;

    queuedUnits++;
    player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
  }
  [Server]
  private void ServerUpdateUnitProduction() {
    // nothing being produced
    if (queuedUnits <= 0) return;

    // add time to timer
    unitTimer += Time.deltaTime;

    // not ready yet
    if (unitTimer < unitSpawnDuration) return;

    ServerProduceAUnit();

    // restart production queue
    queuedUnits--;
    unitTimer = 0;
  }
  [Server]
  private void ServerProduceAUnit() {

    GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
    NetworkServer.Spawn(unitInstance, connectionToClient);

    // random vector in a sphere
    Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
    // don't want to move up/down... keep that as it was
    spawnOffset.y = unitSpawnPoint.position.y;

    // move our unit a touch (so they don't all stack up)
    UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
    unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

  }
  #endregion

  #region Client

  public void OnPointerClick(PointerEventData eventData) {

    // must be left mouse button
    if (eventData.button != PointerEventData.InputButton.Left) return;

    if (!hasAuthority) return;

    CmdSpawnUnit();
  }
  void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits) {
    remainingUnitsText.text = $"{newUnits}";
  }
  private void ClientUpdateTimerDisplay() {
    float newProgress = unitTimer / unitSpawnDuration;

    // if it wrapped around snap to new value
    if (newProgress < unitProgressImage.fillAmount) {
      unitProgressImage.fillAmount = newProgress;
    } else {
      unitProgressImage.fillAmount = Mathf.SmoothDamp(
        unitProgressImage.fillAmount,
        newProgress,
        ref progressImageVelocity,
        0.1f
      );
    }
  }

  #endregion
}
