using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Langrisser1_2_PersonalMod.Patches
{
    [HarmonyPatch(typeof(Resources))]
    internal class CSVDataManager
    {
        // Hook Resources.Load(string, Type) - matching Unity's 'systemTypeInstance' parameter name
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Resources.Load), new Type[] { typeof(string), typeof(Type) })]
        public static void ResourcesLoadWithTypePostfix(string path, Type systemTypeInstance, ref UnityEngine.Object __result)
        {
            ProcessLoadedAsset(path, ref __result);
        }

        // Hook Resources.Load(string)
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Resources.Load), new Type[] { typeof(string) })]
        public static void ResourcesLoadSinglePostfix(string path, ref UnityEngine.Object __result)
        {
            ProcessLoadedAsset(path, ref __result);
        }

        private static void ProcessLoadedAsset(string path, ref UnityEngine.Object result)
        {
            if (string.IsNullOrEmpty(path)) return;

            // Normalize path separators and check case-insensitively for "csv/"
            string normalizedPath = path.Replace('\\', '/');
            if (!normalizedPath.StartsWith("csv/", StringComparison.OrdinalIgnoreCase)) return;

            if (result is TextAsset vanillaTextAsset)
            {
                string relativeFilePath = normalizedPath + ".csv";
                string dumpFilePath = Path.Combine(MainPlugin.DumpDir, relativeFilePath);
                string modFilePath = Path.Combine(MainPlugin.ModDir, relativeFilePath);

                // 1. Dump vanilla file
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dumpFilePath));
                    File.WriteAllText(dumpFilePath, vanillaTextAsset.text, Encoding.UTF8);
                    MainPlugin.Log.LogInfo($"Dumped CSV: {path}");
                }
                catch (Exception ex)
                {
                    MainPlugin.Log.LogError($"Failed to dump CSV file '{path}': {ex.Message}");
                }

                // 2. Load and merge modded CSV if available
                if (File.Exists(modFilePath))
                {
                    try
                    {
                        string modText = File.ReadAllText(modFilePath, Encoding.UTF8);
                        string mergedText = MergeCsvData(vanillaTextAsset.text, modText);

                        result = new TextAsset(mergedText) { name = vanillaTextAsset.name };
                        MainPlugin.Log.LogInfo($"Successfully merged modded CSV: {path}");
                    }
                    catch (Exception ex)
                    {
                        MainPlugin.Log.LogError($"Failed to merge modded CSV '{path}': {ex.Message}");
                    }
                }
            }
        }

        public static string MergeCsvData(string vanillaText, string modText)
        {
            char[] lineSeparators = new char[] { '\r', '\n' };
            string[] vanillaLines = vanillaText.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
            string[] modLines = modText.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (vanillaLines.Length == 0) return modText;
            if (modLines.Length == 0) return vanillaText;

            int vanillaStartIdx = -1;
            int vanillaEndIdx = -1;

            for (int i = 0; i < vanillaLines.Length; i++)
            {
                string line = vanillaLines[i].Trim();
                string[] parts = line.Split(',');
                if (parts.Length > 0)
                {
                    string tag = parts[0].Trim();
                    if (tag.Equals("START", StringComparison.OrdinalIgnoreCase) && vanillaStartIdx == -1)
                        vanillaStartIdx = i;
                    else if (tag.Equals("END", StringComparison.OrdinalIgnoreCase))
                        vanillaEndIdx = i;
                }
            }

            List<string> modDataRows = new List<string>();
            bool inModDataBlock = false;

            foreach (var rawLine in modLines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = line.Split(',');
                string tag = parts[0].Trim();

                if (tag.Equals("START", StringComparison.OrdinalIgnoreCase))
                {
                    inModDataBlock = true;
                    continue;
                }
                if (tag.Equals("END", StringComparison.OrdinalIgnoreCase))
                {
                    inModDataBlock = false;
                    continue;
                }

                if (inModDataBlock || (!modText.ToUpper().Contains("START") && !tag.Equals("ENUMTOP", StringComparison.OrdinalIgnoreCase) && !tag.Equals("ENUM", StringComparison.OrdinalIgnoreCase)))
                {
                    modDataRows.Add(rawLine);
                }
            }

            Dictionary<string, string> modRowMap = new Dictionary<string, string>();
            List<string> newModRows = new List<string>();

            foreach (var modRow in modDataRows)
            {
                string key = GetRowKey(modRow);
                if (!string.IsNullOrEmpty(key))
                {
                    if (!modRowMap.ContainsKey(key))
                        modRowMap.Add(key, modRow);
                }
                else
                {
                    newModRows.Add(modRow);
                }
            }

            List<string> resultLines = new List<string>();
            HashSet<string> overwrittenKeys = new HashSet<string>();

            for (int i = 0; i < vanillaLines.Length; i++)
            {
                string currentLine = vanillaLines[i];

                if (vanillaStartIdx != -1 && i > vanillaStartIdx && (vanillaEndIdx == -1 || i < vanillaEndIdx))
                {
                    string key = GetRowKey(currentLine);
                    if (!string.IsNullOrEmpty(key) && modRowMap.ContainsKey(key))
                    {
                        resultLines.Add(modRowMap[key]);
                        overwrittenKeys.Add(key);
                        continue;
                    }
                }

                if (i == vanillaEndIdx)
                {
                    foreach (var kvp in modRowMap)
                    {
                        if (!overwrittenKeys.Contains(kvp.Key))
                            resultLines.Add(kvp.Value);
                    }
                    foreach (var newRow in newModRows)
                    {
                        resultLines.Add(newRow);
                    }
                }

                resultLines.Add(currentLine);
            }

            if (vanillaEndIdx == -1)
            {
                foreach (var kvp in modRowMap)
                {
                    if (!overwrittenKeys.Contains(kvp.Key))
                        resultLines.Add(kvp.Value);
                }
                foreach (var newRow in newModRows)
                {
                    resultLines.Add(newRow);
                }
            }

            return string.Join("\n", resultLines.ToArray());
        }

        private static string GetRowKey(string csvLine)
        {
            string[] cols = csvLine.Split(',');
            if (cols.Length == 0) return null;

            for (int i = 0; i < Math.Min(cols.Length, 3); i++)
            {
                string val = cols[i].Trim();
                if (!string.IsNullOrEmpty(val) && !val.Equals("START", StringComparison.OrdinalIgnoreCase) && !val.Equals("END", StringComparison.OrdinalIgnoreCase))
                {
                    return val;
                }
            }
            return null;
        }
    }
}