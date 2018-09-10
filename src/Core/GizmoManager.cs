using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;

using BTDebug.Utils;

namespace BTDebug {
  public class GizmoManager {
    private static GizmoManager instance;

    public bool IsGizmoModeActive { get; private set; } = false;

    private Material regionMaterial;
    private List<GameObject> regionPointRepresentations = new List<GameObject>();

    public static GizmoManager GetInstance() { 
      if (instance == null) instance = new GizmoManager();
      return instance;
    }

    private GizmoManager() {
      regionMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      regionMaterial.color = Color.magenta;
    }

    public void ToggleGizmos() {
      if (IsGizmoModeActive) {
        DisableRegions();
      } else {
        EnableRegions();
      }
    }

    private void EnableRegions() {
      RegionPointGameLogic[] regionPoints = MonoBehaviour.FindObjectsOfType<RegionPointGameLogic>();
      foreach (RegionPointGameLogic regionPoint in regionPoints) {
        GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholderPoint.transform.parent = regionPoint.transform;
        placeholderPoint.transform.position = regionPoint.Position;
        placeholderPoint.transform.localScale = new Vector3(5, 5, 5);
        Material mat = placeholderPoint.GetComponent<Renderer>().material;
        placeholderPoint.GetComponent<Renderer>().sharedMaterial = regionMaterial;
        regionPointRepresentations.Add(placeholderPoint);
      }
    }

    private void DisableRegions() {
      foreach (GameObject regionPointRepresentation in regionPointRepresentations) {
        MonoBehaviour.Destroy(regionPointRepresentation);
      }
    }
  }
}