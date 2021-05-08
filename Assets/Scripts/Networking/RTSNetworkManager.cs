using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager {
  [SerializeField] GameObject unitSpawnerPrefab;
  [SerializeField] GameOverHandler gameOverHandlerPrefab;

  public override void OnServerAddPlayer(NetworkConnection conn) {
    base.OnServerAddPlayer(conn);

    var player = conn.identity.GetComponent<RTSPlayer>();
    player.SetTeamColor(new Color(
      UnityEngine.Random.Range(0f, 1f),
      UnityEngine.Random.Range(0f, 1f),
      UnityEngine.Random.Range(0f, 1f)
    ));

    var spawner = Instantiate(unitSpawnerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);
    NetworkServer.Spawn(spawner, conn);
  }
  public override void OnServerSceneChanged(string sceneName) {
    if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) {
      GameOverHandler instance = Instantiate(gameOverHandlerPrefab);
      NetworkServer.Spawn(instance.gameObject);
    }
  }
}
