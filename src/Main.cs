using System;
using System.Collections.Generic;
using HBS.Logging;
using Harmony;
using Newtonsoft.Json;
using System.Reflection;

using BTDebug.Utils;

namespace BTDebug {
    public class Main {
        public static ILog Logger;
        private static Settings settings;
        public static string Path { get; private set; }

        public static void InitLogger(string modDirectory) {
            Dictionary<string, LogLevel> logLevels = new Dictionary<string, LogLevel> {
                ["BTDebug"] = LogLevel.Debug
            };
            LogManager.Setup(modDirectory + "/output.log", logLevels);
            Logger = LogManager.GetLogger("BTDebug");
            Path = modDirectory;
        }

        // Entry point into the mod, specified in the `mod.json`
        public static void Init(string modDirectory, string modSettings) {
            try {
                InitLogger(modDirectory);

                Logger.Log("Loading BTDebug settings");
                settings = JsonConvert.DeserializeObject<Settings>(modSettings);
            } catch (Exception e) {
                Logger.LogError(e);
                Logger.Log("Error loading mod settings - using defaults.");
                settings = new Settings();
            }

            HarmonyInstance harmony = HarmonyInstance.Create("co.uk.cwolf.BTDebug");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}