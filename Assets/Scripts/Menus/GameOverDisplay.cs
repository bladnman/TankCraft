using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour {
  [SerializeField] GameObject gameOverDisplayParent;
  [SerializeField] TMP_Text winnerNameText;
  void Start() {
    GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
  }
  void OnDestroy() {
    GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
  }
  public void LeaveGame() {
    // STOP HOSTING
    if (NetworkServer.active && NetworkClient.isConnected) {
      NetworkManager.singleton.StopHost();
    }

    // STOP CLIENT
    else {
      NetworkManager.singleton.StopClient();
    }
  }
  private void ClientHandleGameOver(string winnerName) {
    winnerNameText.text = $"{winnerName} has Won!";
    gameOverDisplayParent.SetActive(true);
  }
}
