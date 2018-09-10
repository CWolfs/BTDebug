using UnityEngine;

namespace BTDebug.BTCamera {
  public class SimpleFreeformCamera : MonoBehaviour {
    private Vector3 angles;
    public float speed = 1.0f;
    public float fastSpeed = 2.0f;
    public float mouseSpeed = 4.0f;
  
    private void OnEnable() {
        angles = transform.eulerAngles;
    }
  
    private void Update() {
      if (CameraManager.GetInstance().IsFreeformCameraEnabled) {
        angles.x -= Input.GetAxis("Mouse Y") * mouseSpeed;
        angles.y += Input.GetAxis("Mouse X") * mouseSpeed;
        transform.eulerAngles = angles;
        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : speed;
        transform.position +=
            Input.GetAxis("Horizontal") * moveSpeed * transform.right +
            Input.GetAxis("Vertical") * moveSpeed * transform.forward;
      }
    }
  }
}