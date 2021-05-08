using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour {
  [SerializeField] TMP_Text resourcesText;
  RTSPlayer player;


  private void Update() {
    if (player == null) {
      // todo: this causes a known error which will be addressed in a later lesson
      player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

      if (player != null) {
        ClientHandleResourcesUpdated(player.GetResources());
        player.ClientOnResourcesChanged += ClientHandleResourcesUpdated;
      }
    }

  }
  private void OnDestroy() {
    if (player != null) {
      player.ClientOnResourcesChanged -= ClientHandleResourcesUpdated;
    }
  }
  void ClientHandleResourcesUpdated(int resources) {
    resourcesText.text = $"Resources: {resources}";
  }
}

