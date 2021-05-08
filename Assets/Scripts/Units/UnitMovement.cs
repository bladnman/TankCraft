using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour {

  [SerializeField] NavMeshAgent agent;
  [SerializeField] Targeter targeter;
  [SerializeField] float chaseRange = 10f;

  #region Server
  public override void OnStartServer() {
    GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
  }
  public override void OnStopServer() {
    GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
  }
  [ServerCallback]
  private void Update() {
    Targetable myTarget = targeter.GetTarget();

    // I HAVE A TARGET
    if (myTarget != null) {
      var toTargetVector = myTarget.transform.position - transform.position;
      // chase if too far away
      if (toTargetVector.sqrMagnitude > chaseRange * chaseRange) {
        agent.SetDestination(myTarget.transform.position);
      }
      // stop once close enough
      else if (agent.hasPath) {
        agent.ResetPath();
      }
    }

    // NO TARGET
    else {
      if (!agent.hasPath) return;
      // unit still trying to go where it was told to go
      if (agent.remainingDistance > agent.stoppingDistance) return;
      // once it reaches we clear path
      // so that it can be pushed away for others and not keep trying to
      // come back
      agent.ResetPath();
    }

  }
  [Command]
  public void CmdMove(Vector3 position) {
    ServerMove(position);
  }
  [Server]
  public void ServerMove(Vector3 position) {
    Debug.Log($"M@ [{GetType()}] SERVER MOVE");   // M@: 
    targeter.ClearTarget();

    // make sure this is a valid position
    if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

    agent.SetDestination(hit.position);
  }
  [Server]
  private void ServerClearPath() {
    agent.ResetPath(); // stop walking any current path
  }
  [Server]
  private void ServerHandleGameOver() {
    ServerClearPath();
  }
  #endregion

}
