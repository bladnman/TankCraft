using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

  [SerializeField] Building building;
  [SerializeField] Image iconImage;
  [SerializeField] TMP_Text priceText;
  [SerializeField] LayerMask floorMask = new LayerMask();

  Camera mainCamera;
  RTSPlayer player;
  BoxCollider buildingCollider;
  GameObject buildingPreviewInstance;
  Renderer buildingRendererInstance;


  void Start() {
    mainCamera = Camera.main;

    iconImage.sprite = building.GetIcon();
    priceText.text = building.GetPrice().ToString();

    buildingCollider = building.GetComponent<BoxCollider>();

  }

  void Update() {
    if (player == null) {
      // todo: this causes a known error which will be addressed in a later lesson
      player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }

    UpdateBuildingPreview();
  }

  public void OnPointerDown(PointerEventData eventData) {
    // we are waiting on the left button, thanks
    if (eventData.button != PointerEventData.InputButton.Left) return;

    // no enough money
    if (player.GetResources() < building.GetPrice()) return;

    buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
    buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

    buildingPreviewInstance.SetActive(false);
  }
  public void OnPointerUp(PointerEventData eventData) {
    // not dragging
    if (buildingPreviewInstance == null) return;

    // WHAT WAS PRESSED?
    if (!MUtils.WasAHit(out RaycastHit hit, floorMask)) return;

    // PLACE BUILDING
    player.CmdTryPlaceBuilding(building.GetId(), hit.point);

    Destroy(buildingPreviewInstance);
  }
  void UpdateBuildingPreview() {
    // not dragging
    if (buildingPreviewInstance == null) return;

    // Make sure the mouse is over the floor (hits the floor)
    if (!MUtils.WasAHit(out RaycastHit hit, floorMask)) return;

    // move to the hit
    buildingPreviewInstance.transform.position = hit.point;

    // if the preview is not active...
    if (!buildingPreviewInstance.activeSelf) {
      buildingPreviewInstance.SetActive(true);
    }

    var color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;

    // update color depending on "placeability"
    buildingRendererInstance.material.SetColor("_BaseColor", color);
  }
}
