using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour {


  [SerializeField] Targetable target;
  public Targetable GetTarget() { return target; }

  public override void OnStartServer() {
    GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
  }
  public override void OnStopServer() {
    GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
  }

  [Command]
  public void CmdSetTarget(GameObject targetGameObject) {
    if (!targetGameObject.TryGetComponent<Targetable>(out Targetable target)) return;

    Debug.Log($"M@ [{GetType()}] SETTING TARGET");   // M@: 
    this.target = target;
  }
  [Server]
  public void ClearTarget() {
    Debug.Log($"M@ [{GetType()}] CLEARING TARGET");   // M@: 
    this.target = null;
  }
  [Server]
  private void ServerHandleGameOver() {
    ClearTarget();
  }
}
