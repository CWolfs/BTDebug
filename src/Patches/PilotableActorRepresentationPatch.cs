using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;

using BattleTech;

namespace BTDebug {
  [HarmonyPatch(typeof(PilotableActorRepresentation), "OnPlayerVisibilityChanged")]
  public class PilotableActorRepresentationPatch {
    static void Prefix(PilotableActorRepresentationPatch __instance, ref VisibilityLevel newLevel) {
      if (!FogOfWarManager.GetInstance().IsFogOfWarOn) {
        Main.Logger.LogDebug($"[BTDebug] Running Prefix for Fog of War");
        newLevel = VisibilityLevel.LOSFull;
      }
    }
  }
}