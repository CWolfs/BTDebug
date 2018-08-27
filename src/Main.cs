using System;
using System.Collections.Generic;
using HBS.Logging;
using Harmony;
using Newtonsoft.Json;
using System.Reflection;

using EncounterConverter.Utils;

namespace EncounterConverter {
    public class Main {
        public static ILog Logger;
        private static Settings settings;

        public static void InitLogger(string modDirectory) {
            Dictionary<string, LogLevel> logLevels = new Dictionary<string, LogLevel> {
                ["EncounterConverter"] = LogLevel.Debug
            };
            LogManager.Setup(modDirectory + "/output.log", logLevels);
            Logger = LogManager.GetLogger("EncounterConverter");
        }

        // Entry point into the mod, specified in the `mod.json`
        public static void Init(string modDirectory, string modSettings) {
            try {
                InitLogger(modDirectory);

                Logger.Log("Loading EncounterConverter settings");
                settings = JsonConvert.DeserializeObject<Settings>(modSettings);
            } catch (Exception e) {
                Logger.LogError(e);
                Logger.Log("Error loading mod settings - using defaults.");
                settings = new Settings();
            }

            HarmonyInstance harmony = HarmonyInstance.Create("co.uk.cwolf.EncounterConverter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}