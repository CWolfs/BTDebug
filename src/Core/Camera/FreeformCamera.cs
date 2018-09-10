using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BTDebug.BTCamera {
  public class FreeFormCamera : MonoBehaviour {
    private float zoomAmount = 0f;
    private float maxClamp = 10f;
    private float rotSpeed = 10f;
    private float dragSpeed = 3f;

    private Transform mTransform;
    private float mouseAxis = 0f;

    // freeform
    private float sensitivityX = 5f;
    private float sensitivityY = 5f;
    
    private float minimumX = -360f;
    private float maximumX = 360f;
    
    private float minimumY = -360f;
    private float maximumY = 360f;
    
    private float rotationX = 0F;
    private float rotationY = 0F;
    
    private Quaternion originalRotation;

    // private float mainSpeed = 10.0f;  // Regular speed
    private float mainSpeed = 1;
    private float shiftAdd = 20.0f;   // Multiplied by how long shift is held.  Basically running
    private float maxShift = 100.0f;  // Maximum speed when holding shift
    private float totalRun = 1.0f;
    // end freeform

    private Vector3 camLastPos = Vector3.one;

    void OnEnable() {
      // mTransform = transform;
      // originalRotation = Quaternion.LookRotation(Vector3.forward);
      //originalRotation = Quaternion.LookRotation(transform.forward);
    }
    
    // Update is called once per frame
    void Update() {
      if (CameraManager.GetInstance().IsFreeformCameraEnabled) {
        MouseLook();
        // XYMiddleMouseMovement();
        // Zoom();
        // camLastPos = transform.localPosition;
      }
    }

    private void MouseLook() {
      if (Input.GetMouseButton(1)) {
        // Read the mouse input axis
        // rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        // rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        
        // rotationX = ClampAngle(rotationX, minimumX, maximumX);
        // rotationY = ClampAngle(rotationY, minimumY, maximumY);
        
        // Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        // Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
        // mTransform.localRotation = originalRotation * xQuaternion * yQuaternion;
        Vector3 angles = transform.eulerAngles;
        float angleX = Input.GetAxis("Mouse Y") * sensitivityY;
        angleX = ClampAngle(angleX, minimumX, maximumX);
        angles.x -= angleX;

        float angleY = Input.GetAxis("Mouse X") * sensitivityX;
        angleY = ClampAngle(angleY, minimumY, maximumY);
        angles.y += angleY;

        transform.localEulerAngles = angles;

        float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? mainSpeed * 2 : mainSpeed;
        transform.localPosition +=
            Input.GetAxis("Horizontal") * moveSpeed * transform.right +
            Input.GetAxis("Vertical") * moveSpeed * transform.forward;
        
        // Keyboard commands
        /*
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift)) {
          totalRun += Time.deltaTime;
          p  = p * totalRun * shiftAdd;
          p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
          p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
          p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        } else {
          totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
          p = p * mainSpeed;
        }
        
        p = p * Time.deltaTime;
        Vector3 newPosition = mTransform.position;
        if (Input.GetKey(KeyCode.Space)){ // If player wants to move on X and Z axis only
          mTransform.Translate(p);
          newPosition.x = mTransform.position.x;
          newPosition.z = mTransform.position.z;
          mTransform.position = newPosition;
        } else {
          mTransform.Translate(p);
        }
        */
      }
    }

    private Vector3 GetBaseInput() { // Returns the basic values, if it's 0 than it's not active.
      Vector3 p_Velocity = new Vector3();
      if (Input.GetKey(KeyCode.W)){
        p_Velocity += new Vector3(0, 0 , 1);
      }
      if (Input.GetKey(KeyCode.S)){
        p_Velocity += new Vector3(0, 0, -1);
      }
      if (Input.GetKey(KeyCode.A)){
        p_Velocity += new Vector3(-1, 0, 0);
      }
      if (Input.GetKey(KeyCode.D)){
        p_Velocity += new Vector3(1, 0, 0);
      }
      return p_Velocity;
    }
    
    public static float ClampAngle(float angle, float min, float max) {
      if (angle < -360F) angle += 360F;
      if (angle > 360F) angle -= 360F;
      return Mathf.Clamp (angle, min, max);
    }

    private void XYMiddleMouseMovement() {
      if (Input.GetMouseButton(2)) {	// middle mouse button
        mTransform.Translate(-(new Vector3(Input.GetAxis("Mouse X") * dragSpeed, Input.GetAxis("Mouse Y") * dragSpeed, 0)));
      }
    }

    private void Zoom() {
      mouseAxis = Input.GetAxis("Mouse ScrollWheel");
      if ((mouseAxis < 0) || (mouseAxis > 0)) {
        zoomAmount += mouseAxis;
        zoomAmount = Mathf.Clamp(zoomAmount, -maxClamp, maxClamp);
        float translate = Mathf.Min(Mathf.Abs(mouseAxis), maxClamp  - Mathf.Abs(zoomAmount));
        mTransform.Translate(0, 0, (translate * rotSpeed  * Mathf.Sign(mouseAxis)) * 2f);
      }
    }
  }
}