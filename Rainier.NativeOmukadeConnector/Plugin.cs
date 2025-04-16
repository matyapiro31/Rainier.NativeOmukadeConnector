/*************************************************************************
* Rainier Native Omukade Connector
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/
#nullable disable

using BepInEx;
using BepInEx.Logging;
using ClientNetworking;
using Newtonsoft.Json;
using Rainier.NativeOmukadeConnector.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting;

namespace Rainier.NativeOmukadeConnector
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string VERSION_STRING = "Native Omukade Connector \"NOC\" 2.1.2 (\"Auditioning Audino Rev2\")";
        internal const string OMUKADE_VERSION = "Omukade Cheyenne-EX";
        internal const string CONFIG_FILENAME = "BepInEx\\config\\config-noc.json";

        internal static ManualLogSource SharedLogger;
        internal static ConfigurationSettings Settings;
        internal static DateTime LastUpdateTime = DateTime.Now;

        private void Awake()
        {
            SharedLogger = Logger;
            
            if (!Environment.GetCommandLineArgs().Contains("--enable-omukade"))
            {
                SharedLogger.LogWarning("Omukade not enabled by command-line; goodbye");
                return;
            }
            SharedLogger.LogWarning($"CMD Line Args is: {string.Join(" ", Environment.GetCommandLineArgs())}");
            SharedLogger.LogWarning($"CMD Line is: {Environment.CommandLine}");

            if (File.Exists(CONFIG_FILENAME))
            {
                SharedLogger.LogMessage("Found config file");
                Settings = JsonConvert.DeserializeObject<ConfigurationSettings>(File.ReadAllText(CONFIG_FILENAME));
            }
            else
            {
                SharedLogger.LogWarning("No Config File Found! Using Dev defaults...");
                Settings = new ConfigurationSettings();
            }

            SharedLogger.LogMessage($"Omukade endpoint set to {Settings.OmukadeEndpoint}");

            if (Plugin.Settings.UseProxyApi)
            {
                // remove https:// or http:// from the endpoint url
                string subdomain = "localhost:7166";
                bool isHttps = false;
                if (Settings.ProxyApiEndpoint.StartsWith("https://"))
                {
                    subdomain = Settings.ProxyApiEndpoint.Substring(8);
                    isHttps = true;
                }
                else if (Settings.ProxyApiEndpoint.StartsWith("http://"))
                {
                    subdomain = Settings.ProxyApiEndpoint.Substring(7);
                    isHttps = false;
                }
                // override readonly field Stages.PROD
                var field = typeof(Stages).GetField("PROD", BindingFlags.Static | BindingFlags.Public);
                if (isHttps)
                {
                    field.SetValue(null, new OmukadeSecureStage(subdomain));
                }
                else
                {
                    field.SetValue(null, new OmukadeStage(subdomain));
                }
                SharedLogger.LogMessage($"API proxy endpoint set to {Settings.ProxyApiEndpoint}");
            }
            if (Plugin.Settings.OverrideCardDefinition)
            {
                // Load Card Definitions from CardDefinitionDirectory.
                try
                {
                    if (Directory.Exists(Plugin.Settings.CardDefinitionDirectory))
                    {
                        // Remove .json from the directory path
                        OmukadeDeckUtils.overrideCardIDs = Directory.GetFileSystemEntries(Plugin.Settings.CardDefinitionDirectory, "*.json").Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
                        OmukadeDeckUtils.cardDefinitionOverrides = new List<string>();
                        List<string> overridesPaths = Directory.GetFiles(Plugin.Settings.CardDefinitionDirectory).Where(f => f.EndsWith(".json")).ToList();
                        foreach (var overridePath in overridesPaths)
                        {
                            string overrideJson = File.ReadAllText(overridePath);
                            OmukadeDeckUtils.cardDefinitionOverrides.Add(overrideJson);
                        }
                        LastUpdateTime = DateTime.Now;
                        SharedLogger.LogMessage($"Card Source Definitions loaded from {Plugin.Settings.CardDefinitionDirectory}");
                    }
                    else
                    {
                        SharedLogger.LogMessage($"Card Source Definitions Not found.");
                    }
                }
                catch (Exception e)
                {
                    SharedLogger.LogError($"Error loading card source definitions: {e}");
                }
            }
            // Plugin startup logic
            SharedLogger.LogInfo($"Performing patches for NOC...");

            new HarmonyLib.Harmony(nameof(Rainier.NativeOmukadeConnector)).PatchAll();

            SharedLogger.LogInfo($"Applied Patches");
        }
    }
}
