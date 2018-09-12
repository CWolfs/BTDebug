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
    
    private GameObject encounterLayerParentGO;

    private Material regionMaterial;
    private List<GameObject> regionPointRepresentations = new List<GameObject>();

    private GameObject activeEncounter;
    private GameObject chunkPlayerLance;
    private GameObject spawnerPlayerLance;
    private List<GameObject> playerLanceSpawnPoints = new List<GameObject>();

    public static GizmoManager GetInstance() { 
      if (instance == null) instance = new GizmoManager();
      return instance;
    }

    private GizmoManager() {
      regionMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      regionMaterial.color = Color.magenta;
    }

    public void ToggleGizmos() {
      if (!encounterLayerParentGO) encounterLayerParentGO = GameObject.Find("EncounterLayerParent");

      if (IsGizmoModeActive) {
        DisableRegions();
        DisableSpawns();
        IsGizmoModeActive = false;
      } else {
        EnableRegions();
        EnableSpawns();
        IsGizmoModeActive = true;
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

    private void EnableSpawns() {
      if (!spawnerPlayerLance) {
        chunkPlayerLance = GetActiveEncounterGameObject().transform.Find("Chunk_PlayerLance").gameObject;
        spawnerPlayerLance = chunkPlayerLance.transform.Find("Spawner_PlayerLance").gameObject;
        if (spawnerPlayerLance == null) { 
          Main.Logger.LogError("[GizmoManager] No active encounters found");
          return;
        }
      }

      EnablePlayerLanceSpawn();
    }

    private void DisableSpawns() {

    }

    private void EnablePlayerLanceSpawn() {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      placeholderPoint.transform.parent = spawnerPlayerLance.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = spawnerPlayerLance.transform.position;
      HexGrid hexGrid = ReflectionHelper.GetPrivateStaticField(typeof(WorldPointGameLogic), "hexGrid") as HexGrid;
      Vector3 hexPosition = hexGrid.GetClosestPointOnGrid(position);
      placeholderPoint.transform.position = hexPosition;

      placeholderPoint.transform.localScale = new Vector3(100, 100, 100);
      Material mat = placeholderPoint.GetComponent<Renderer>().material;
      placeholderPoint.GetComponent<Renderer>().sharedMaterial = regionMaterial;
    }

    private GameObject GetActiveEncounterGameObject() {
      if (activeEncounter) return activeEncounter;

      foreach (Transform t in encounterLayerParentGO.transform) {
        GameObject child = t.gameObject;
        if (child.activeSelf) {
          activeEncounter = t.gameObject;
          return activeEncounter;
        }
      }
      return null;
    }
  }
}