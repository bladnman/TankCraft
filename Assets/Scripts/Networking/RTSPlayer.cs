using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour {
  [SerializeField] LayerMask buildingBlockLayer = new LayerMask();
  [SerializeField] Building[] avilableBuildings = new Building[0];
  [SerializeField] float buildingRangeLimit = 5f;

  public event Action<int> ClientOnResourcesChanged;
  [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
  int resources = 500;
  public int GetResources() { return resources; }


  List<Unit> units = new List<Unit>();
  public List<Unit> GetUnits() { return units; }

  List<Building> buildings = new List<Building>();
  public List<Building> GetBuildings() { return buildings; }

  Color teamColor = new Color();
  public Color GetTeamcolor() { return teamColor; }

  public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point) {

    // bail - over-lapping other buildings... not okay
    if (Physics.CheckBox(point + buildingCollider.center,
        buildingCollider.size / 2,
        Quaternion.identity,
        buildingBlockLayer
        )) return false;


    foreach (var building in buildings) {
      // is in range
      if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit) {
        return true;
      }
    }

    return false;
  }

  #region Server
  public override void OnStartServer() {
    Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
  }
  public override void OnStopServer() {
    Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
  }
  [Command]
  public void CmdTryPlaceBuilding(int buildingId, Vector3 point) {
    Building buildingToPlace = null;
    foreach (var building in avilableBuildings) {
      if (building.GetId() == buildingId) {
        buildingToPlace = building;
        break;
      }
    }

    // bail - no bulding to manage
    if (buildingToPlace == null) return;

    // bail - cannot affor this item
    if (resources < buildingToPlace.GetPrice()) return;

    // grab this building's collider
    var buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

    // bail - not in range
    if (!CanPlaceBuilding(buildingCollider, point)) return;

    // charge them for it
    SetResources(resources - buildingToPlace.GetPrice());

    // make the building
    var buildingInstance = Instantiate(buildingToPlace, point, buildingToPlace.transform.rotation);
    // tell the network
    NetworkServer.Spawn(buildingInstance.gameObject, connectionToClient);
  }
  [Server]
  public void SetResources(int newResources) {
    resources = newResources;
  }
  [Server]
  public void SetTeamColor(Color color) {
    teamColor = color;
  }
  [Server]
  void ServerHandleUnitSpawned(Unit unit) {
    // not owned by this player
    if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

    units.Add(unit);
  }
  [Server]
  void ServerHandleUnitDespawned(Unit unit) {
    // not owned by this player
    if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

    units.Remove(unit);
  }
  [Server]
  private void ServerHandleBuildingSpawned(Building building) {
    // not owned by this player
    if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

    buildings.Add(building);
  }
  [Server]
  private void ServerHandleBuildingDespawned(Building building) {
    // not owned by this player
    if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

    buildings.Remove(building);
  }
  #endregion

  #region Client
  public override void OnStartAuthority() {
    // do not do this on the server
    if (NetworkServer.active) return;
    Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
    Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
    Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
  }
  public override void OnStopClient() {
    if (!isClientOnly || !hasAuthority) return;
    Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
    Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
    Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
  }
  void AuthorityHandleUnitSpawned(Unit unit) {
    units.Add(unit);
  }
  void AuthorityHandleUnitDespawned(Unit unit) {
    units.Remove(unit);
  }
  void AuthorityHandleBuildingSpawned(Building building) {
    buildings.Add(building);
  }
  void AuthorityHandleBuildingDespawned(Building building) {
    buildings.Remove(building);
  }
  void ClientHandleResourcesUpdated(int _oldValue, int newValue) {
    ClientOnResourcesChanged?.Invoke(newValue);
  }
  #endregion
}
