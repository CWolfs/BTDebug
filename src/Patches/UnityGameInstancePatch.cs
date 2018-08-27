using UnityEngine;
using System;
using System.IO;
using Harmony;

using BattleTech;

// this.Sim.DialogPanel.Show
namespace EncounterConverter {
  [HarmonyPatch(typeof(UnityGameInstance), "Update")]
  public class UnityGameInstancePatch {
    static void Postfix(UnityGameInstance __instance) {
      if (Input.GetKeyDown(KeyCode.P)) {
        string indentation = "";
        Main.Logger.LogDebug($"[EncounterConverter] Outting all game objects and components");
        GameObject[] rootGos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in rootGos) {
          RecursivePrintGameObject(go, indentation);
        }
      }
    }

    static void RecursivePrintGameObject(GameObject go, string indentation) {
      if (go.activeInHierarchy) {
        Main.Logger.LogDebug($"{indentation}- [GO] {go.name}");
        Component[] components = go.GetComponents<Component>();
        indentation += "  ";
        foreach (Component component in components) {
          if (component is MonoBehaviour) {
            MonoBehaviour mb = (MonoBehaviour)component;
            if (!mb.enabled) continue;
          }

          if (component is Collider) {
            Collider col = (Collider)component;
            if (!col.enabled) continue;
          }

          /*
          if (component is Rigidbody) {
            Rigidbody rb = (Rigidbody)component;
            if (rb.collisionDetectionMode == CollisionDetectionMode.Discrete || rb.isKinematic) continue;
          }
          */

          Main.Logger.LogDebug($"{indentation}- [Component] {component.GetType().Name}");
        }
        
        foreach(Transform t in go.transform) {
          RecursivePrintGameObject(t.gameObject, indentation);
          indentation.Remove(indentation.Length - 2);
        }
      }
    }
  }
}