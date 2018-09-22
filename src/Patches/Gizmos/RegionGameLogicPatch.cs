using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;

using BattleTech;

namespace BTDebug {
  [HarmonyPatch(typeof(RegionGameLogic), "Update")]
  public class RegionGameLogicPatch {
    static void Postfix(RegionGameLogic __instance) {
      if (GizmoManager.GetInstance().IsGizmoModeActive && GizmoManager.GetInstance().IsGizmoRegionModeActive) {
        MeshRenderer component = __instance.GetComponent<MeshRenderer>();
        if (component != null) {
					component.enabled = true;
				}
      }
    }
  }
}