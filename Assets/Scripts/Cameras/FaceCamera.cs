using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {

  Transform mainCameraTransform;

  void Start() {
    mainCameraTransform = Camera.main.transform;
  }
  // lateupdate is called "after update"
  // this will allow the camera to move (update) first
  void LateUpdate() {
    // face the camera... trust me
    transform.LookAt(
      transform.position + mainCameraTransform.rotation * Vector3.forward,
      mainCameraTransform.rotation * Vector3.up
    );
  }
}
