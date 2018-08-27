using UnityEngine;
using System;
using System.IO;
using Harmony;

using BattleTech;
using BattleTech.Data;
using BattleTech.Serialization;

using Newtonsoft;
using Newtonsoft.Json;

// this.Sim.DialogPanel.Show
namespace EncounterConverter {
  [HarmonyPatch(typeof(MapMetaDataExporter), "LoadEncounterLayerDataV2")]
  public class MapMetaDataExporterPatch {
    static void Prefix(MapMetaDataExporter __instance, EncounterLayerIdentifier encounterLayerIdentifier, DataManager dataManager) {
      Main.Logger.LogDebug($"Prefix running for MapMetaDataExporter with method LoadEncounterLayerDataV2");

      EncounterLayerParent component = __instance.GetComponent<EncounterLayerParent>();
			string text = encounterLayerIdentifier.path;
			string encounterLayerDataName = MapMetaDataExporter.GetEncounterLayerDataName(encounterLayerIdentifier.name);
			VersionManifestEntry versionManifestEntry = dataManager.ResourceLocator.EntryByID(encounterLayerDataName, BattleTechResourceType.LayerData);
			text = versionManifestEntry.FilePath;
			byte[] data = File.ReadAllBytes(text);
			EncounterLayerData layerByGuid = component.GetLayerByGuid(encounterLayerIdentifier.encounterLayerGuid);
			EncounterLayerData encounterLayerData = Serializer.Deserialize<EncounterLayerData>(data, SerializationTarget.Exported, TargetMaskOperation.HAS_ANY, layerByGuid);
			layerByGuid.ReattachReferences();

      try {
        /* 
        File.WriteAllText(Path.ChangeExtension(text, ".json"), JsonConvert.SerializeObject(encounterLayerData, Formatting.Indented, new JsonSerializerSettings {
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
          NullValueHandling = NullValueHandling.Include,
        }));
        */
        // File.WriteAllText(Path.ChangeExtension(text, ".json"), encounterLayerData.ToJSON());
      } catch (Exception e) {
        Main.Logger.LogDebug(e);
      }

      Main.Logger.LogDebug($"Finished Prefix");
    }
  }
}