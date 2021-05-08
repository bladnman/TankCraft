using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour {
  [SerializeField] Health health;
  [SerializeField] int resourcesPerInterval = 10;
  [SerializeField] float interval = 2f;

  float timer;
  RTSPlayer player;

  #region Server  -  -  -  -  -  -  -  -  -  
  public override void OnStartServer() {
    timer = interval;
    player = connectionToClient.identity.GetComponent<RTSPlayer>();

  }
  public override void OnStopServer() {
    health.ServerOnDie -= ServerHandleDie;
    GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
  }
  [ServerCallback]
  private void Update() {
    timer -= Time.deltaTime;
    if (timer <= 0) {

      player.SetResources(player.GetResources() + resourcesPerInterval);

      timer += interval;
    }
  }

  private void ServerHandleGameOver() {
    enabled = false;
  }

  private void ServerHandleDie() {
    NetworkServer.Destroy(gameObject);
  }
  #endregion

  #region Client  -  -  -  -  -  -  -  -  -  

  #endregion
}
