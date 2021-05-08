using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour {
  [SerializeField] Renderer[] colorRenderers = new Renderer[0];

  [SyncVar(hook = nameof(HandleTeamColorUpdated))]
  Color teamColor = new Color();

  #region Server  -  -  -  -  -  -  -  -  -  
  public override void OnStartServer() {
    var player = connectionToClient.identity.GetComponent<RTSPlayer>();
    teamColor = player.GetTeamcolor();
  }
  #endregion

  #region Client  -  -  -  -  -  -  -  -  -  
  void HandleTeamColorUpdated(Color oldColor, Color newColor) {
    foreach (var renderer in colorRenderers) {
      renderer.material.SetColor("_BaseColor", newColor);
    }
  }
  #endregion
}
