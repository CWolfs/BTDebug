using Harmony;

using BattleTech;

namespace BTDebug {
  [HarmonyPatch(typeof(CameraControl), "Update")]
  public class CameraControlUpdatePatch {
    static void Postfix(CameraControl __instance, DebugFlyCameraControl ___debugFlyCameraControl) {
      if (CameraManager.GetInstance().IsFreeformCameraEnabled) {
        ___debugFlyCameraControl.enabled = false;
      }
    }
  }
}