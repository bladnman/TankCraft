using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour {
  [SerializeField] int resourceCost = 10;
  [SerializeField] Health health;
  [SerializeField] UnitMovement unitMovement;
  [SerializeField] Targeter targeter;
  [SerializeField] UnityEvent onSelected;
  [SerializeField] UnityEvent onDeselected;

  public static event Action<Unit> ServerOnUnitSpawned;
  public static event Action<Unit> ServerOnUnitDespawned;
  public static event Action<Unit> AuthorityOnUnitSpawned;
  public static event Action<Unit> AuthorityOnUnitDespawned;

  public UnitMovement GetUnitMovement() { return unitMovement; }
  public Targeter GetTargeter() { return targeter; }
  public int GetResourceCost() { return resourceCost; }

  #region Server
  public override void OnStartServer() {
    health.ServerOnDie += ServerHandleDie;
    ServerOnUnitSpawned?.Invoke(this);
  }
  public override void OnStopServer() {
    health.ServerOnDie -= ServerHandleDie;
    ServerOnUnitDespawned?.Invoke(this);
  }
  [Server]
  private void ServerHandleDie() {
    NetworkServer.Destroy(gameObject);
  }
  #endregion


  #region Client
  public override void OnStartAuthority() {
    AuthorityOnUnitSpawned?.Invoke(this);
  }
  public override void OnStopClient() {
    if (!hasAuthority) return;

    AuthorityOnUnitDespawned?.Invoke(this);
  }
  [Client]
  public void Select() {
    if (!hasAuthority) return;
    onSelected?.Invoke();
  }
  [Client]
  public void Deselect() {
    if (!hasAuthority) return;
    onDeselected?.Invoke();
  }
  #endregion
}
