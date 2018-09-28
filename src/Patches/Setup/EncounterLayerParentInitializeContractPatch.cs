using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;

using BattleTech;
using BattleTech.Framework;

/*
  This patch is the start of this mod.
  It allows for the planning on what should be done for this particular encounter / contract.
  Once all the tasks are queued up they will be executed at the correct patch points
*/
namespace BTDebug {
  [HarmonyPatch(typeof(EncounterLayerParent), "InitializeContract")]
  public class EncounterLayerParentInitializeContractPatch {
    static void Prefix(EncounterLayerParent __instance, MessageCenterMessage message) {
      Main.Logger.Log($"[EncounterLayerParentInitializeContractPatch Prefix] Patching InitializeContract");
      InitializeContractMessage initializeContractMessage = message as InitializeContractMessage;
      CombatGameState combat = initializeContractMessage.combat;
      Contract activeContract = combat.ActiveContract;
      GizmoManager.GetInstance().SetContract(activeContract);
    }
  }
}