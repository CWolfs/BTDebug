using UnityEngine;
using System;

using BTDebug.Utils;

namespace BTDebug {
  public class InspectorManager {
    private static InspectorManager instance;

    private GameObject inspectorPrefab;
    private GameObject inspectorGO;
    public bool IsInspectorShowing { get; private set; } = false;

    public static InspectorManager GetInstance() { 
      if (instance == null) instance = new InspectorManager();
      return instance;
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
      if (!inspectorPrefab) inspectorPrefab = Main.BTDebugBundle.LoadAsset("BTDebugInspector") as GameObject;
      if (!inspectorGO) inspectorGO = MonoBehaviour.Instantiate(inspectorPrefab, Vector3.zero, Quaternion.identity);
      if (!inspectorGO.activeSelf) inspectorGO.SetActive(true);
      LayerTools.SetLayerRecursively(inspectorGO, 17);
      IsInspectorShowing = true;
    }

    public void HideInspector() {
      Main.Logger.LogDebug($"[BTDebug] Hiding inspector");
      inspectorGO.SetActive(false);
      IsInspectorShowing = false;  
    }

    public void RemoveInspector() {
      Main.Logger.LogDebug($"[BTDebug] Removing inspector");
      MonoBehaviour.Destroy(inspectorGO);
      IsInspectorShowing = false;
    }
  }
}