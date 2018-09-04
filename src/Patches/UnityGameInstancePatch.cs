using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony;

using BattleTech;

using BTDebug.Utils;

// this.Sim.DialogPanel.Show
namespace BTDebug {
  [HarmonyPatch(typeof(UnityGameInstance), "Update")]
  public class UnityGameInstancePatch {
    private static bool LoadedAssembly { get; set; } = false;
    private static Assembly DebugAssembly { get; set; } 
    private static AssetBundle Bundle { get; set; }

    static void Postfix(UnityGameInstance __instance) {
      if (Input.GetKeyDown(KeyCode.P)) {
        string indentation = "";
        Main.Logger.LogDebug($"[BTDebug] Outting all game objects and components");
        GameObject[] rootGos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in rootGos) {
          RecursivePrintGameObject(go, indentation);
        }
      }

      if (Input.GetKeyDown(KeyCode.I)) {
        AssetBundleTest();
      }
    }

    static void AssetBundleTest() {
      Main.Logger.LogDebug($"[BTDebug] Loading test asset bundle");
      
      if (!LoadedAssembly) {
        DebugAssembly = Assembly.LoadFile($"{Main.Path}/bundles/BTDebug-Library.dll");
        Bundle = AssetBundle.LoadFromFile($"{Main.Path}/bundles/btdebug-bundle");
        LoadedAssembly = true;
      }

      GameObject prefab = Bundle.LoadAsset("DebugInspector") as GameObject;
      GameObject inspector = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.identity);
      LayerTools.SetLayerRecursively(inspector, 17);

      Main.Logger.LogDebug($"[BTDebug] Finished loading test asset bundle");
    }

    static void RecursivePrintGameObject(GameObject go, string indentation) {
      if (go.activeInHierarchy) {
        Main.Logger.LogDebug($"{indentation}- [GO] {go.name} - {go.tag} - {go.layer}");
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