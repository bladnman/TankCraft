using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targetable : NetworkBehaviour {

  [SerializeField] Transform aimAtPoint;
  public Transform GetAimAtPoint() { return aimAtPoint; }


}
