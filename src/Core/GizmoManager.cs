using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;
using BattleTech.Designed;

using BTDebug.Utils;

namespace BTDebug {
  public class GizmoManager {
    private enum SpawnType { PLAYER_MECH, ENEMY_MECH, ENEMY_TURRET };
    private static GizmoManager instance;

    public bool IsGizmoModeActive { get; private set; } = false;
    
    private GameObject encounterLayerParentGO;
    private HexGrid hexGrid;

    private Material regionMaterial;
    private List<GameObject> regionPointRepresentations = new List<GameObject>();

    private Material spawnAreaMaterial;
    private List<GameObject> spawnerRepresentations = new List<GameObject>();

    private Material playerMechSpawnMaterial;
    private List<GameObject> playerMechSpawnRepresentations = new List<GameObject>();

    private GameObject activeEncounter;
    private GameObject chunkPlayerLance;
    private GameObject spawnerPlayerLance;
    private List<GameObject> playerLanceSpawnPoints = new List<GameObject>();
    private GameObject boundary;

    public static GizmoManager GetInstance() { 
      if (instance == null) instance = new GizmoManager();
      return instance;
    }

    private GizmoManager() {
      regionMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      regionMaterial.color = Color.magenta;

      spawnAreaMaterial = new Material(Shader.Find("Unlit/BT-Stars"));

      playerMechSpawnMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      playerMechSpawnMaterial.color = Color.blue;
    }

    public void ToggleGizmos() {
      if (!encounterLayerParentGO) encounterLayerParentGO = GameObject.Find("EncounterLayerParent");
      if (hexGrid == null) hexGrid = ReflectionHelper.GetPrivateStaticField(typeof(WorldPointGameLogic), "hexGrid") as HexGrid;

      if (IsGizmoModeActive) {
        DisableRegions();
        DisableSpawns();
        DisableBoundary();
        IsGizmoModeActive = false;
      } else {
        EnableRegions();
        EnableSpawns();
        EnableBoundary();
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
      DisablePlayerLanceSpawn();
    }

    private void EnablePlayerLanceSpawn() {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      placeholderPoint.transform.parent = spawnerPlayerLance.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = spawnerPlayerLance.transform.position;
      Vector3 hexPosition = hexGrid.GetClosestPointOnGrid(position);
      placeholderPoint.transform.position = hexPosition;

      placeholderPoint.transform.localScale = new Vector3(100, 100, 100);
      placeholderPoint.GetComponent<Renderer>().sharedMaterial = spawnAreaMaterial;

      spawnerRepresentations.Add(placeholderPoint);

      foreach (Transform t in spawnerPlayerLance.transform) {
        GameObject mechSpawn = t.gameObject;
        if (mechSpawn.name.Contains("SpawnPoint")) {
          GameObject gizmo = EnableSpawn(mechSpawn, SpawnType.PLAYER_MECH);
          playerMechSpawnRepresentations.Add(gizmo);
        }
      }
    }

    private void DisablePlayerLanceSpawn() {
      foreach (GameObject spawnerRepresentations in spawnerRepresentations) {
        MonoBehaviour.Destroy(spawnerRepresentations);
      }

      foreach(GameObject playerMechSpawnRepresentation in playerMechSpawnRepresentations) {
        MonoBehaviour.Destroy(playerMechSpawnRepresentation);
      }
    }

    private GameObject EnableSpawn(GameObject target, SpawnType type) {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      placeholderPoint.transform.parent = target.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = target.transform.position;
      Vector3 hexPosition = hexGrid.GetClosestPointOnGrid(position);
      placeholderPoint.transform.position = hexPosition;
      placeholderPoint.transform.localScale = new Vector3(10, 10, 10);

      if (type == SpawnType.PLAYER_MECH) {
        placeholderPoint.GetComponent<Renderer>().sharedMaterial = playerMechSpawnMaterial;
      }

      return placeholderPoint;
    }

    private void EnableBoundary() {
      GameObject chunkBoundaryRect = activeEncounter.transform.Find("Chunk_EncounterBoundary").gameObject;
      GameObject boundary = chunkBoundaryRect.transform.Find("EncounterBoundaryRect").gameObject;
      EncounterBoundaryChunkGameLogic chunkBoundaryLogic = chunkBoundaryRect.GetComponent<EncounterBoundaryChunkGameLogic>();
      EncounterBoundaryRectGameLogic boundaryLogic = boundary.GetComponent<EncounterBoundaryRectGameLogic>();
      Rect boundaryRec = chunkBoundaryLogic.GetEncounterBoundaryRectBounds();

      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
      placeholderPoint.transform.parent = boundary.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;
      placeholderPoint.transform.localScale = new Vector3(boundaryRec.width, boundaryRec.height, boundaryRec.width);

      boundary = placeholderPoint;
    }

    private void DisableBoundary() {
      MonoBehaviour.Destroy(boundary);
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