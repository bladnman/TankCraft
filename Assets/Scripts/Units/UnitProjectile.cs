using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public class UnitProjectile : NetworkBehaviour {
  [SerializeField] Rigidbody rb;
  [SerializeField] float lifespan = 5f;
  [SerializeField] float launchForce = 10f;
  [SerializeField] int damageToDeal = 20;

  private void Start() {
    rb.velocity = transform.forward * launchForce;
  }

  public override void OnStartServer() {
    Invoke(nameof(DestroySelf), lifespan);
  }

  [ServerCallback]
  private void OnTriggerEnter(Collider other) {
    // we entered something
    if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity)) {
      // same connection means the owner was hit... bail
      if (networkIdentity.connectionToClient == connectionToClient) return;
    }

    // the thing hit has health .. DAMAGE
    if (other.TryGetComponent<Health>(out Health health)) {
      health.DealDamange(damageToDeal);
    }

    // hitting anything destroys us
    DestroySelf();
  }

  [Server]
  void DestroySelf() {
    NetworkServer.Destroy(gameObject);
  }
}
