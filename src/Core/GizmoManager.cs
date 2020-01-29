using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;
using BattleTech.Designed;
using BattleTech.Framework;

using BTDebug.Utils;

namespace BTDebug {
  public class GizmoManager {
    private enum SpawnType { PLAYER_MECH, ENEMY_MECH, ENEMY_TURRET, NEUTRAL };
    private static GizmoManager instance;

    public bool IsGizmoModeActive { get; private set; } = false;
    public bool IsGizmoRegionModeActive { get; private set; } = false;

    public Contract Contract { get; private set; }

    private GameObject encounterLayerParentGO;
    private HexGrid hexGrid;

    private Material regionMaterial;
    private List<GameObject> regionPointRepresentations = new List<GameObject>();

    private Material spawnAreaMaterial;
    private List<GameObject> spawnerRepresentations = new List<GameObject>();

    private Material playerMechSpawnMaterial;
    private Material enemyMechSpawnMaterial;
    private Material neutralMechSpawnMaterial;
    private List<GameObject> playerMechSpawnRepresentations = new List<GameObject>();

    private Material plotMaterial;
    private List<GameObject> plotCentreRepresentations = new List<GameObject>();

    private GameObject activeEncounter;
    private GameObject chunkPlayerLance;
    private GameObject spawnerPlayerLance;
    private List<GameObject> playerLanceSpawnPoints = new List<GameObject>();

    private GameObject boundaryRepresentation;
    private Material boundaryMaterial;

    private GameObject mapBoundaryRepresentation;
    private Material mapBoundaryMaterial;

    private Material routeMaterial;
    private List<GameObject> routePointRepresentations = new List<GameObject>();

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
      enemyMechSpawnMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      enemyMechSpawnMaterial.color = Color.red;
      neutralMechSpawnMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      neutralMechSpawnMaterial.color = Color.green;

      plotMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      plotMaterial.color = Color.white;

      boundaryMaterial = new Material(Shader.Find("Unlit/BT-LaserUI"));
      boundaryMaterial.name = "BTDebug Boundary";
      boundaryMaterial.color = new Color(255f / 255f, 100f / 255f, 100f / 255f, 140f / 255f);

      mapBoundaryMaterial = new Material(Shader.Find("Unlit/BT-LaserUI"));
      mapBoundaryMaterial.name = "BTDebug Boundary";
      mapBoundaryMaterial.color = new Color(47f / 255f, 190f / 255f, 255f / 255f, 140f / 255f);

      routeMaterial = new Material(Shader.Find("UI/DefaultBackground"));
      routeMaterial.color = Color.yellow;
    }

    public void SetContract(Contract contract) {
      Contract = contract;
    }

    public void UpdateBoundaryColour() {
      if (FogOfWarManager.GetInstance().IsFogOfWarOn) {
        boundaryMaterial.color = new Color(255f / 255f, 100f / 255f, 100f / 255f, 140f / 255f);
        mapBoundaryMaterial.color = new Color(47f / 255f, 190f / 255f, 255f / 255f, 140f / 255f);
      } else {
        boundaryMaterial.color = new Color(255f / 255f, 100f / 255f, 100f / 255f, 80f / 255f);
        mapBoundaryMaterial.color = new Color(47f / 255f, 190f / 255f, 255f / 255f, 80f / 255f);
      }
    }

    public void ToggleGizmos() {
      if (!encounterLayerParentGO) encounterLayerParentGO = GameObject.Find("EncounterLayerParent");
      if (hexGrid == null) hexGrid = ReflectionHelper.GetPrivateStaticField(typeof(WorldPointGameLogic), "_hexGrid") as HexGrid;

      if (IsGizmoModeActive) {
        DisableRegions();
        DisableSpawns();
        DisablePlotCentres();
        DisableBoundary();
        DisableMapBoundary();
        DisableRoutes();
        IsGizmoModeActive = false;
      } else {
        EnableRegions();
        EnableSpawns();
        EnablePlotCentres();
        EnableBoundary();
        EnableMapBoundary();
        EnableRoutes();
        IsGizmoModeActive = true;
      }
    }

    public void ToggleGizmoRegionMode() {
      IsGizmoRegionModeActive = !IsGizmoRegionModeActive;
    }

    private void EnableRegions() {
      RegionPointGameLogic[] regionPoints = MonoBehaviour.FindObjectsOfType<RegionPointGameLogic>();
      foreach (RegionPointGameLogic regionPoint in regionPoints) {
        GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholderPoint.name = "RegionPointGizmo";
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
        chunkPlayerLance = GetActiveEncounterGameObject().transform.Find(GetPlayerLanceChunkName()).gameObject;
        spawnerPlayerLance = chunkPlayerLance.transform.Find(GetPlayerLanceSpawnerName()).gameObject;
        if (spawnerPlayerLance == null) {
          Main.Logger.LogError("[GizmoManager] No active encounters found");
          return;
        }
      }

      EnablePlayerLanceSpawn();
      EnableEnemyLanceSpawns();
      EnableNeutralLanceSpawns();
    }

    private void DisableSpawns() {
      DisableLanceSpawns();
    }

    private void EnablePlayerLanceSpawn() {
      EnableLance(spawnerPlayerLance, SpawnType.PLAYER_MECH);
    }

    private void EnableEnemyLanceSpawns() {
      List<GameObject> lanceSpawners = activeEncounter.FindAllContainsRecursive(
        "Lance_Enemy",
        "Lance_OpposingForce",

        // mapStory_StoryEncounter1b_vHigh - Story_1B_Retreat,
        "TraitorLance",
        "Fight2Lance",
        "03_DestroyLanceSpawner",
        "TargetLance",
        "05_MainHostileLanceSpawner",
        "LanceSpawner",

        // ArenaSkirmish
        "Player2LanceSpawner"
      );

      foreach (GameObject lanceSpawn in lanceSpawners) {
        EnableLance(lanceSpawn, SpawnType.ENEMY_MECH);
      }
    }

    private void EnableNeutralLanceSpawns() {
      List<GameObject> lanceSpawners = activeEncounter.FindAllContainsRecursive(
        "Lance_Neutral",
        "Lance_Escort",
        "Lance_Ally",         // Mission Control

        // mapStory_StoryEncounter1b_vHigh - Story_1B_Retreat
        "AranoFriendlyLance",
        "NeutralLance",

        // FireMission
        "Lance_Employer"
      );

      foreach (GameObject lanceSpawn in lanceSpawners) {
        EnableLance(lanceSpawn, SpawnType.NEUTRAL);
      }
    }

    private void EnableLance(GameObject spawner, SpawnType type) {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      placeholderPoint.name = "LanceSpawnGizmo";
      placeholderPoint.transform.parent = spawner.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = spawner.transform.position;
      Vector3 hexPosition = hexGrid.GetClosestPointOnGrid(position);
      placeholderPoint.transform.position = hexPosition;

      placeholderPoint.transform.localScale = new Vector3(100, 100, 100);
      placeholderPoint.GetComponent<Renderer>().sharedMaterial = spawnAreaMaterial;

      spawnerRepresentations.Add(placeholderPoint);

      foreach (Transform t in spawner.transform) {
        GameObject mechSpawn = t.gameObject;
        if (mechSpawn.name.Contains("SpawnPoint")) {
          GameObject gizmo = EnableSpawn(mechSpawn, type);
          playerMechSpawnRepresentations.Add(gizmo);
        }
      }
    }

    private void DisableLanceSpawns() {
      foreach (GameObject spawnerRepresentations in spawnerRepresentations) {
        MonoBehaviour.Destroy(spawnerRepresentations);
      }

      foreach (GameObject playerMechSpawnRepresentation in playerMechSpawnRepresentations) {
        MonoBehaviour.Destroy(playerMechSpawnRepresentation);
      }
    }

    private GameObject EnableSpawn(GameObject target, SpawnType type) {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      placeholderPoint.name = "MechSpawnGizmo";
      placeholderPoint.transform.parent = target.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = target.transform.position;
      Vector3 hexPosition = hexGrid.GetClosestPointOnGrid(position);
      placeholderPoint.transform.position = hexPosition;
      placeholderPoint.transform.localScale = new Vector3(10, 10, 10);

      if (type == SpawnType.PLAYER_MECH) {
        placeholderPoint.GetComponent<Renderer>().sharedMaterial = playerMechSpawnMaterial;
      } else if (type == SpawnType.ENEMY_MECH) {
        placeholderPoint.GetComponent<Renderer>().sharedMaterial = enemyMechSpawnMaterial;
      } else if (type == SpawnType.NEUTRAL) {
        placeholderPoint.GetComponent<Renderer>().sharedMaterial = neutralMechSpawnMaterial;
      }

      return placeholderPoint;
    }

    private void EnableBoundary() {
      GameObject chunkBoundaryRect = activeEncounter.transform.Find("Chunk_EncounterBoundary").gameObject;
      GameObject boundary = chunkBoundaryRect.transform.Find("EncounterBoundaryRect").gameObject;
      EncounterBoundaryChunkGameLogic chunkBoundaryLogic = chunkBoundaryRect.GetComponent<EncounterBoundaryChunkGameLogic>();
      EncounterBoundaryRectGameLogic boundaryLogic = boundary.GetComponent<EncounterBoundaryRectGameLogic>();

      Rect boundaryRec = boundaryLogic.GetRect();
      Rect usableBoundary = boundaryRec.GenerateUsableBoundary();

      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
      placeholderPoint.name = "BoundaryGizmo";
      placeholderPoint.transform.parent = boundary.transform;

      Vector3 centre = (Vector3)ReflectionHelper.GetPrivateField(boundaryLogic, "rectCenter");
      placeholderPoint.transform.position = new Vector3(centre.x + ((boundaryRec.width - usableBoundary.width) / 2f), centre.y, centre.z - ((boundaryRec.height - usableBoundary.height) / 2f));
      placeholderPoint.transform.localScale = new Vector3(usableBoundary.width, boundaryRec.height, usableBoundary.height);

      placeholderPoint.GetComponent<Renderer>().sharedMaterial = boundaryMaterial;

      boundaryRepresentation = placeholderPoint;
    }

    private void DisableBoundary() {
      MonoBehaviour.Destroy(boundaryRepresentation);
    }

    private void EnableMapBoundary() {
      float mapBorderSize = 50f;
      float mapSize = 2048f;
      Rect mapBoundary = new Rect(0, 0, mapSize - (mapBorderSize * 2), mapSize - (mapBorderSize * 2));

      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
      placeholderPoint.name = "MapBoundaryGizmo";
      placeholderPoint.transform.position = new Vector3(mapBoundary.x, 100f, mapBoundary.y);
      placeholderPoint.transform.localScale = new Vector3(mapBoundary.width, mapBoundary.height, mapBoundary.height);

      placeholderPoint.GetComponent<Renderer>().sharedMaterial = mapBoundaryMaterial;

      mapBoundaryRepresentation = placeholderPoint;
    }

    private void DisableMapBoundary() {
      MonoBehaviour.Destroy(mapBoundaryRepresentation);
    }

    private void EnableRoutes() {
      RoutePointGameLogic[] routePoints = MonoBehaviour.FindObjectsOfType<RoutePointGameLogic>();
      foreach (RoutePointGameLogic routePoint in routePoints) {
        GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placeholderPoint.name = "RoutePointGizmo";
        placeholderPoint.transform.parent = routePoint.transform;
        placeholderPoint.transform.position = routePoint.Position;
        placeholderPoint.transform.localScale = new Vector3(5, 5, 5);

        placeholderPoint.GetComponent<Renderer>().sharedMaterial = routeMaterial;
        routePointRepresentations.Add(placeholderPoint);
      }
    }

    private void DisableRoutes() {
      foreach (GameObject routePointRepresentation in routePointRepresentations) {
        MonoBehaviour.Destroy(routePointRepresentation);
      }
    }

    private void EnablePlotCentres() {
      GameObject plotsParentGo = GameObject.Find("PlotParent");
      foreach (Transform t in plotsParentGo.transform) {
        Vector3 plotPosition = t.position;
        if (IsPlotValidForEncounter(t)) {
          GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
          placeholderPoint.name = "PlotCentreGizmo";
          placeholderPoint.transform.parent = t;
          placeholderPoint.transform.position = t.position;
          placeholderPoint.transform.localScale = new Vector3(5, 5, 5);

          placeholderPoint.GetComponent<Renderer>().sharedMaterial = plotMaterial;
          plotCentreRepresentations.Add(placeholderPoint);
        }
      }
    }

    private void DisablePlotCentres() {
      foreach (GameObject plotCentreRepresentation in plotCentreRepresentations) {
        MonoBehaviour.Destroy(plotCentreRepresentation);
      }
    }

    private bool IsPlotValidForEncounter(Transform plotTransform) {
      Transform selectedPlotTransform = plotTransform.FindIgnoreCaseStartsWith("PlotVariant");

      if (selectedPlotTransform == null) {
        return false;
      }

      GameObject selectedPlotGo = selectedPlotTransform.gameObject;
      if (selectedPlotGo.activeSelf) return true;

      return false;
    }

    private GameObject GetActiveEncounterGameObject() {
      if (activeEncounter) return activeEncounter;

      foreach (Transform t in encounterLayerParentGO.transform) {
        GameObject child = t.gameObject;
        if (child.activeSelf) {
          if (t.GetComponent<EncounterLayerData>()) {
            activeEncounter = t.gameObject;
            return activeEncounter;
          }
        }
      }
      return null;
    }

    public string GetPlayerLanceChunkName() {
      string type = Contract.ContractTypeValue.Name;

      if (type == "ArenaSkirmish") {
        return "MultiPlayerSkirmishChunk";
      } else if (type == "Story_1B_Retreat") {
        return "Gen_PlayerLance";
      } else if (type == "Story_5_ServedCold_Default") {
        return "01_InitialSetup/Chunk_PlayerLance";
      }

      return "Chunk_PlayerLance";
    }

    public string GetPlayerLanceSpawnerName() {
      string type = Contract.ContractTypeValue.Name;

      if (type == "ArenaSkirmish") {
        return "Player1LanceSpawner";
      } else if ((type == "Story_1B_Retreat") || (type == "FireMission") || (type == "AttackDefend")) {
        return "PlayerLanceSpawner";
      } else if (type == "ThreeWayBattle") {
        return "PlayerLanceSpawner_Battle+";
      }

      return "Spawner_PlayerLance";
    }
  }
}