using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using RuntimeInspectorNamespace;

using BattleTech;
using BattleTech.Rendering;
using BattleTech.Designed;
using BattleTech.Framework;

using BTDebug.Utils;

namespace BTDebug {
  public class GizmoManager {
    public const string PLAYER_TEAM_ID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public const string PLAYER_2_TEAM_ID = "757173dd-b4e1-4bb5-9bee-d78e623cc867";
    public const string EMPLOYER_TEAM_ID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
    public const string TARGET_TEAM_ID = "be77cadd-e245-4240-a93e-b99cc98902a5";
    public const string TARGETS_ALLY_TEAM_ID = "31151ed6-cfc2-467e-98c4-9ae5bea784cf";
    public const string NEUTRAL_TO_ALL_TEAM_ID = "61612bb3-abf9-4586-952a-0559fa9dcd75";
    public const string HOSTILE_TO_ALL_TEAM_ID = "3c9f3a20-ab03-4bcb-8ab6-b1ef0442bbf0";

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
        chunkPlayerLance = GetPlayerLanceChunk().gameObject;
        spawnerPlayerLance = GetPlayerLanceSpawner(chunkPlayerLance).gameObject;
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
      LanceSpawnerGameLogic[] lanceSpawnersArray = activeEncounter.GetComponentsInChildren<LanceSpawnerGameLogic>();
      List<LanceSpawnerGameLogic> lanceSpawners = lanceSpawnersArray.Where(
        lanceSpawner =>
          (lanceSpawner.teamDefinitionGuid == TARGET_TEAM_ID) ||
          (lanceSpawner.teamDefinitionGuid == TARGETS_ALLY_TEAM_ID)
      ).ToList();

      foreach (LanceSpawnerGameLogic lanceSpawn in lanceSpawners) {
        EnableLance(lanceSpawn.gameObject, SpawnType.ENEMY_MECH);
      }
    }

    private void EnableNeutralLanceSpawns() {
      LanceSpawnerGameLogic[] lanceSpawnersArray = activeEncounter.GetComponentsInChildren<LanceSpawnerGameLogic>();
      List<LanceSpawnerGameLogic> lanceSpawners = lanceSpawnersArray.Where(lanceSpawner => (lanceSpawner.teamDefinitionGuid == EMPLOYER_TEAM_ID || lanceSpawner.teamDefinitionGuid == NEUTRAL_TO_ALL_TEAM_ID)).ToList();

      foreach (LanceSpawnerGameLogic lanceSpawn in lanceSpawners) {
        EnableLance(lanceSpawn.gameObject, SpawnType.NEUTRAL);
      }
    }

    private void EnableLance(GameObject spawner, SpawnType type) {
      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      placeholderPoint.name = "LanceSpawnGizmo";
      placeholderPoint.transform.parent = spawner.transform;
      placeholderPoint.transform.localPosition = Vector3.zero;

      Vector3 position = spawner.transform.position;
      placeholderPoint.transform.position = position;

      placeholderPoint.transform.localScale = new Vector3(100, 100, 100);
      placeholderPoint.GetComponent<Renderer>().sharedMaterial = spawnAreaMaterial;

      spawnerRepresentations.Add(placeholderPoint);

      UnitSpawnPointGameLogic[] unitSpawnArray = spawner.GetComponentsInChildren<UnitSpawnPointGameLogic>();
      foreach (UnitSpawnPointGameLogic unitSpawnPoint in unitSpawnArray) {
        GameObject gizmo = EnableSpawn(unitSpawnPoint.gameObject, type);
        playerMechSpawnRepresentations.Add(gizmo);
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
      placeholderPoint.transform.position = position;
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

    private EncounterBoundaryChunkGameLogic GetBoundaryChunk() {
      GameObject encounterGo = GetActiveEncounterGameObject();
      return encounterGo.GetComponentInChildren<EncounterBoundaryChunkGameLogic>();
    }

    private void EnableBoundary() {
      GameObject chunkBoundaryRect = GetBoundaryChunk().gameObject;
      EncounterBoundaryChunkGameLogic chunkBoundaryLogic = chunkBoundaryRect.GetComponent<EncounterBoundaryChunkGameLogic>();
      EncounterBoundaryRectGameLogic boundaryLogic = chunkBoundaryRect.GetComponentInChildren<EncounterBoundaryRectGameLogic>();
      GameObject boundary = boundaryLogic.gameObject;

      Rect boundaryRec = boundaryLogic.GetRect();
      Rect usableBoundary = boundaryRec.GenerateUsableBoundary();

      GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
      placeholderPoint.name = "BoundaryGizmo";
      placeholderPoint.transform.parent = boundary.transform;

      Vector3 centre = (Vector3)ReflectionHelper.GetPrivateField(boundaryLogic, "rectCenter");
      // usableBoundary is in corner coordinates (0,0 = bottom-left of full map), convert to world/center coordinates
      // World coordinates are centered on the full 2048x2048 map (offset = 2048/2 = 1024)
      Vector3 usableBoundaryCentre = new Vector3(
        (usableBoundary.x + usableBoundary.width / 2f) - 1024f,
        centre.y,
        (usableBoundary.y + usableBoundary.height / 2f) - 1024f
      );
      placeholderPoint.transform.position = usableBoundaryCentre;
      placeholderPoint.transform.localScale = new Vector3(usableBoundary.width, usableBoundary.height, usableBoundary.height);

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
      foreach (Transform plot in plotsParentGo.transform) {
        Vector3 plotPosition = plot.position;

        foreach (Transform variant in plot) {
          if (IsPlotValidForEncounter(variant)) {
            GameObject placeholderPoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholderPoint.name = "PlotCentreGizmo";
            placeholderPoint.transform.parent = variant.transform;
            placeholderPoint.transform.position = variant.transform.position;
            placeholderPoint.transform.localScale = new Vector3(5, 200, 5);

            placeholderPoint.GetComponent<Renderer>().sharedMaterial = plotMaterial;
            plotCentreRepresentations.Add(placeholderPoint);
          }
        }
      }
    }

    private void DisablePlotCentres() {
      foreach (GameObject plotCentreRepresentation in plotCentreRepresentations) {
        MonoBehaviour.Destroy(plotCentreRepresentation);
      }
    }

    private bool IsPlotValidForEncounter(Transform plotVariantTransform) {
      if (plotVariantTransform.gameObject.activeSelf) {
        return plotVariantTransform.gameObject.name.ToLower().StartsWith("plotvariant");
      }
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

    public PlayerLanceChunkGameLogic GetPlayerLanceChunk() {
      string type = Contract.ContractTypeValue.Name;
      GameObject encounterGo = GetActiveEncounterGameObject();
      return encounterGo.GetComponentInChildren<PlayerLanceChunkGameLogic>();
    }

    public PlayerLanceSpawnerGameLogic GetPlayerLanceSpawner(GameObject playerChunk) {
      string type = Contract.ContractTypeValue.Name;
      return playerChunk.GetComponentInChildren<PlayerLanceSpawnerGameLogic>();
    }
  }
}