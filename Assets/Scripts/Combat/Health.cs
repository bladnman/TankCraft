using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
  [SerializeField] int maxHealth = 100;

  [SyncVar(hook = nameof(HandleHealthUpdated))]
  [SerializeField] int currentHealth;
  bool isAlive = true;

  public event Action ServerOnDie;
  public event Action<int, int> ClientOnHealthUpdated;

  #region Server
  public override void OnStartServer() {
    currentHealth = maxHealth;
    UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
  }
  public override void OnStopServer() {
    UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
  }

  [Server]
  public void DealDamange(int damageAmount) {
    // take the damange
    currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

    // DIE
    if (currentHealth <= 0) {
      Die();
    }
  }
  [Server]
  public void Die() {
    if (!isAlive) return;
    isAlive = false;
    currentHealth = 0;

    ServerOnDie?.Invoke();
  }
  [Server]
  private void ServerHandlePlayerDie(int playerCnxId) {
    // was this our player?
    if (connectionToClient.connectionId != playerCnxId) return;

    Die();
  }
  #endregion


  #region Client

  void HandleHealthUpdated(int oldHealth, int newHealth) {
    ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
  }

  #endregion
}
