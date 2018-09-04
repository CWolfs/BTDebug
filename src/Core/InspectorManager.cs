using UnityEngine;
using System;

using RuntimeInspectorNamespace;

using BTDebug.Utils;

namespace BTDebug {
  public class InspectorManager {
    private static InspectorManager instance;

    private GameObject inspectorPrefab;
    private GameObject inspectorGO;
    private bool debugMode = false;

    public bool IsInspectorShowing { get; private set; } = false;

    public static InspectorManager GetInstance() { 
      if (instance == null) instance = new InspectorManager();
      return instance;
    }

    public InspectorManager() {
      SetDebugMode(Main.Settings.inspectorDebugMode);
    }

    private void ApplySettings(GameObject go) {
      RuntimeInspector inspector = inspectorPrefab.GetComponentInChildren<RuntimeInspector>();
      inspector.DebugMode = debugMode;
    }

    public void ToggleDebugMode() {
      SetDebugMode(!debugMode);
    }

    public void SetDebugMode(bool flag) {
      Main.Logger.LogDebug($"[BTDebug] Setting debug mode to {flag}");
      debugMode = flag;
      if (inspectorPrefab) {
        RuntimeInspector inspector = inspectorPrefab.GetComponentInChildren<RuntimeInspector>();
        inspector.DebugMode = debugMode;
      }

      if (inspectorGO) {
        RuntimeInspector inspector = inspectorGO.GetComponentInChildren<RuntimeInspector>();
        inspector.DebugMode = debugMode;  
      }
    }

    public void ToggleInspector() {
      if (IsInspectorShowing) {
        HideInspector();
      } else {
        ShowInspector();
      }
    }

    public void ShowInspector() {
      Main.Logger.LogDebug($"[BTDebug] Showing inspector");
      if (!inspectorPrefab) {
        inspectorPrefab = Main.BTDebugBundle.LoadAsset("BTDebugInspector") as GameObject;
        ApplySettings(inspectorPrefab);
      }

      if (!inspectorGO) inspectorGO = MonoBehaviour.Instantiate(inspectorPrefab, Vector3.zero, Quaternion.identity);
      if (!inspectorGO.activeSelf) inspectorGO.SetActive(true);
      LayerTools.SetLayerRecursively(inspectorGO, 17);
      IsInspectorShowing = true;
    }

    public void HideInspector() {
      Main.Logger.LogDebug($"[BTDebug] Hiding inspector");
      if (inspectorGO) inspectorGO.SetActive(false);
      IsInspectorShowing = false;
    }

    public void RemoveInspector() {
      Main.Logger.LogDebug($"[BTDebug] Removing inspector");
      if (inspectorGO) MonoBehaviour.Destroy(inspectorGO);
      IsInspectorShowing = false;
    }
  }
}