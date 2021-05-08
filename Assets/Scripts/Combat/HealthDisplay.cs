using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour {
  [SerializeField] Health health;
  [SerializeField] GameObject healthBarParent;
  [SerializeField] Image healthBarImage;

  private void Awake() {
    health.ClientOnHealthUpdated += HandleHealthUpdated;
  }
  private void OnDestroy() {
    health.ClientOnHealthUpdated -= HandleHealthUpdated;
  }

  void OnMouseEnter() {
    healthBarParent.SetActive(true);
  }
  void OnMouseExit() {
    healthBarParent.SetActive(false);
  }

  void HandleHealthUpdated(int currentHealth, int maxHealth) {
    healthBarImage.fillAmount = (float)currentHealth / maxHealth;
  }
}
