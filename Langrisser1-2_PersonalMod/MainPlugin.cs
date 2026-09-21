using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Langrisser1_2_PersonalMod
{

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class MainPlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.Taba.Langrisser1_2_PersonalMod";
        private const string PluginName = "Langrisser1_2_PersonalMod";
        private const string VersionString = "1.0.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);
        public static string DumpDir;
        public static string ModDir;

        /// <summary>
        /// Initialise the configuration settings and patch methods
        /// </summary>
        private void Awake()
        {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            
            Log = Logger;

            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DumpDir = Path.Combine(pluginFolder, "Dump");
            ModDir = Path.Combine(pluginFolder, "Mod");

            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");

        }
    }
}
