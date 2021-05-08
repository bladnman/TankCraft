using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour {

  public static event Action ServerOnGameOver;
  public static event Action<string> ClientOnGameOver;

  [SerializeField] List<UnitBase> bases = new List<UnitBase>();

  #region Server  -  -  -  -  -  -  -  -  -  
  public override void OnStartServer() {
    UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
    UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
  }
  public override void OnStopServer() {
    UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
    UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
  }
  [Server]
  private void ServerHandleBaseSpawned(UnitBase unitBase) {
    bases.Add(unitBase);
  }
  [Server]
  private void ServerHandleBaseDespawned(UnitBase unitBase) {
    bases.Remove(unitBase);

    // LAST PLAYER STANDING
    if (bases.Count == 1) {
      Debug.Log($"M@ [{GetType()}] WINNER WINNER!");   // M@: 
      int winnerId = bases[0].connectionToClient.connectionId;
      RpcGameOver($"Player {winnerId}");

      ServerOnGameOver?.Invoke();
    }
  }
  #endregion

  #region Client  -  -  -  -  -  -  -  -  -  

  [ClientRpc]
  void RpcGameOver(string winnerName) {
    ClientOnGameOver?.Invoke(winnerName);
  }
  #endregion
}
