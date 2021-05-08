using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour {
  [SerializeField] Targeter targeter;
  [SerializeField] GameObject projectilePrefab;
  [SerializeField] Transform projectileSpawnPoint;
  [SerializeField] float fireRange = 5f;
  [SerializeField] float fireRate = 1f;
  [SerializeField] float rotationSpeed = 180f;


  float lastFireTime;

  [ServerCallback]
  private void Update() {

    if (!CanFireAtTarget()) return;

    // face our target
    var target = targeter.GetTarget();

    Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    // fire
    if (Time.time > (1 / fireRate) + lastFireTime) {
      Quaternion projecitleRotation = Quaternion.LookRotation(
        target.GetAimAtPoint().position - projectileSpawnPoint.position);

      // this creates on server
      GameObject projectileInstance = Instantiate(projectilePrefab,
        projectileSpawnPoint.position, projecitleRotation);

      // we also need it on the network
      NetworkServer.Spawn(projectileInstance, connectionToClient);

      lastFireTime = Time.time;
    }
  }

  [Server]
  bool CanFireAtTarget() {
    var target = targeter.GetTarget();
    if (target == null) return false;
    var toTargetVector = target.transform.position - transform.position;
    return toTargetVector.sqrMagnitude <= fireRange * fireRange;
  }
}
