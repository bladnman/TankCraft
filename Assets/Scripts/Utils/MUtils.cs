using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MUtils {
  static public bool WasAHit(out RaycastHit hit, LayerMask layerMask) {
    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    var wasAHit = Physics.Raycast(ray, out RaycastHit anyHit, Mathf.Infinity, layerMask);
    hit = anyHit;
    return wasAHit;
  }
}
