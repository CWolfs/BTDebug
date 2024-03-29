using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;

using BTDebug.Utils;
using BTDebug.BTCamera;

namespace BTDebug {
  public class CameraManager {
    private static CameraManager instance;
    private static string CombatGameCameraGOName = "GameCamera(Clone)";
    private static string SimGameCameraGOName = "Camera_SimGame";
    private static string SpaceCameraGOName = "Camera_Space";

    public bool IsFreeformCameraEnabled { get; private set; } = false;
    public bool IsUiEnabled { get; private set; } = true;
    public bool IsInSimGame { get; private set; } = false;

    private GameObject GameCameraObject { get; set; }
    private CameraControl CameraControl { get; set; }
    private DebugFlyCameraControl DebugFlyCameraControl { get; set; }
    private Camera Camera { get; set; }
    private FreeFormCamera FreeFormCamera { get; set; }

    private float originalCameraFoV = 0;
    private float originalCameraFarClipPlane = 0;

    private GameObject UiManagerGo { get; set; }

    public static CameraManager GetInstance() {
      if (instance == null) instance = new CameraManager();
      return instance;
    }

    private CameraManager() {

    }

    public void ToggleFreeformCamera() {
      if (SetupCamera()) {
        EnableFreeformCamera(!IsFreeformCameraEnabled);
      }
    }

    public void ToggleUi() {
      EnableUi(!IsUiEnabled);
    }

    public void EnableFreeformCamera(bool flag) {
      if (flag) {
        Main.Logger.LogDebug($"[BTDebug] Turning Freeform Camera is ON");
        originalCameraFoV = Camera.fieldOfView;
        originalCameraFarClipPlane = Camera.farClipPlane;
        Camera.fieldOfView = 60;
        Camera.farClipPlane = 9999;
        if (!IsInSimGame) CameraControl.DEBUG_TakeCompleteControl = true;

        if (!FreeFormCamera) {
          FreeFormCamera = GameCameraObject.AddComponent<FreeFormCamera>();
        }

        FreeFormCamera.enabled = true;
        IsFreeformCameraEnabled = true;
      } else {
        Main.Logger.LogDebug($"[BTDebug] Turning Freeform Camera is OFF");

        if (!IsInSimGame) CameraControl.DEBUG_TakeCompleteControl = false;
        Camera.fieldOfView = originalCameraFoV;
        Camera.farClipPlane = originalCameraFarClipPlane;
        if (FreeFormCamera) {
          FreeFormCamera.enabled = false;
          MonoBehaviour.Destroy(FreeFormCamera);
        }

        IsFreeformCameraEnabled = false;

        // Reset GOs
        GameCameraObject = null;
        CameraControl = null;
        Camera = null;
        DebugFlyCameraControl = null;
        FreeFormCamera = null;
      }
    }

    public void EnableUi(bool flag) {
      if (UiManagerGo == null) UiManagerGo = GameObject.Find("UIManager");

      if (flag) {
        GameObject uiRoot = UiManagerGo.transform.Find("UICam").gameObject;
        uiRoot.SetActive(true);

        List<GameObject> inworldUiElements = UiManagerGo.FindAllContainsRecursive("uixPrfIndc_");
        foreach (GameObject inworldUiElement in inworldUiElements) {
          inworldUiElement.SetActive(true);
        }

        IsUiEnabled = true;
      } else {
        GameObject uiRoot = UiManagerGo.transform.Find("UICam").gameObject;
        uiRoot.SetActive(false);

        List<GameObject> inworldUiElements = UiManagerGo.FindAllContainsRecursive("uixPrfIndc_");
        foreach (GameObject inworldUiElement in inworldUiElements) {
          inworldUiElement.SetActive(false);
        }

        IsUiEnabled = false;
      }
    }

    private bool SetupCamera() {
      if (!GameCameraObject) {
        GameCameraObject = GameObject.Find(CombatGameCameraGOName);
        if (GameCameraObject == null) {
          IsInSimGame = true;
          if (UnityGameInstance.BattleTechGame.Simulation.CurRoomState == DropshipLocation.SHIP) {
            GameCameraObject = GameObject.Find(SpaceCameraGOName);
          } else {
            GameCameraObject = GameObject.Find(SimGameCameraGOName);
          }
        } else {
          IsInSimGame = false;
        }

        if (!GameCameraObject) return false;

        if (!IsInSimGame) {
          CameraControl = GameCameraObject.GetComponent<CameraControl>();
          if (!CameraControl) return false;

          DebugFlyCameraControl = GameCameraObject.GetComponent<DebugFlyCameraControl>();
          if (!DebugFlyCameraControl) return false;
        }

        Camera = GameCameraObject.GetComponentInChildren<Camera>();
        if (!Camera) return false;
      }
      return true;
    }
  }
}