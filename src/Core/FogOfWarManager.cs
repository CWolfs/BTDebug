using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;

using BTDebug.Utils;

namespace BTDebug {
  public class FogOfWarManager {
    private static FogOfWarManager instance;
    private static string CombatGameCameraGOName = "GameCamera(Clone)";

    public bool IsFogOfWarOn { get; private set; } = true;
    public bool IsFogOfWarOverlayDisableable { get; private set; } = true;
    
    private FogScattering FogScatteringComponent { get; set; }

    public static FogOfWarManager GetInstance() { 
      if (instance == null) instance = new FogOfWarManager();
      return instance;
    }

    private FogOfWarManager() {
      SetOverlayMode(Main.Settings.fowDisableOverlay);
    }

    private void SetOverlayMode(bool flag) {
      IsFogOfWarOverlayDisableable = flag;
    }

    public void ToggleFogOfWar() {
      EnableFogOfWar(!IsFogOfWarOn);
    }

    public void EnableFogOfWar(bool flag) {
      if (flag) {
        Main.Logger.LogDebug($"[BTDebug] Turning Fog of War is ON");
        IsFogOfWarOn = true;
        EnablePilotableActorsFoW();
        if (IsFogOfWarOverlayDisableable) EnableOverlay();
      } else {
        Main.Logger.LogDebug($"[BTDebug] Turning Fog of War is OFF");
        IsFogOfWarOn = false;
        DisablePilotableActorsFoW();
        if (IsFogOfWarOverlayDisableable) DisableOverlay();
      }
    }

    private void EnablePilotableActorsFoW() {
      List<ICombatant> combatants = UnityGameInstance.BattleTechGame.Combat.GetAllCombatants();
      PilotableActorRepresentation[] pilotableActors = MonoBehaviour.FindObjectsOfType<PilotableActorRepresentation>();
      foreach (PilotableActorRepresentation pilotableActor in pilotableActors) {
        pilotableActor.ClearForcedPlayerVisibilityLevel(combatants);
      }
    }

    private void DisablePilotableActorsFoW() {
      PilotableActorRepresentation[] pilotableActors = MonoBehaviour.FindObjectsOfType<PilotableActorRepresentation>();
      foreach (PilotableActorRepresentation pilotableActor in pilotableActors) {
        pilotableActor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
      }
    }

    private void EnableOverlay() {
      if (SetupFogScatter()) FogScatteringComponent.enabled = true;
    }

    private void DisableOverlay() {
      if (SetupFogScatter()) FogScatteringComponent.enabled = false;
    }

    private bool SetupFogScatter() {
      if (!FogScatteringComponent) {
        GameObject gameCameraObject = GameObject.Find(CombatGameCameraGOName);
        if (!gameCameraObject) return false;
        
        FogScatteringComponent = gameCameraObject.GetComponentInChildren<FogScattering>();
        if (!FogScatteringComponent) return false;
      }
      return true;
    }
  }
}