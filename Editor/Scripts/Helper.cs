﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InputSystemWrapper.Utilities.Editor;
using InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Data;

namespace UnityInputSystemWrapper.Editor
{
    public static class Helper
    {
        private const string MARKER = "// MARKER";
        private const string START = "Start";
        private const string END = "End";
        
        // Assets
        public static InputActionAsset InputActionAsset => EditorAssetGetter.GetFirst<RuntimeInputData>().InputActionAsset;
        public static OfflineInputData OfflineInputData => EditorAssetGetter.GetFirst<OfflineInputData>();
        public static string InputNamespace => GetNamespace(InputManagerFileSystemPath);
        
        // Existing script paths
        public static string InputManagerFileSystemPath => EditorScriptGetter.GetSystemFilePath(typeof(Input));
        public static string InputPlayerFileSystemPath => EditorScriptGetter.GetSystemFilePath<InputPlayer>();
        public static string ControlSchemeFileSystemPath => EditorScriptGetter.GetSystemFilePath<ControlScheme>();
        public static string InputContextFileSystemPath => EditorScriptGetter.GetSystemFilePath<InputContext>();
        public static string PlayerIDFileSystemPath => EditorScriptGetter.GetSystemFilePath<PlayerID>();
        public static string DeviceControlInfoFileSystemPath => EditorScriptGetter.GetSystemFilePath<DeviceControlInfo>();
        private static string InputManagerFolderSystemPath => EditorScriptGetter.GetSystemFolderPath(typeof(Input));
        
        // Template paths
        public static string MapActionsTemplateFileSystemPath => EditorAssetGetter.GetSystemFilePath(OfflineInputData.MapActionsTemplateFile);
        public static string MapCacheTemplateFileSystemPath => EditorAssetGetter.GetSystemFilePath(OfflineInputData.MapCacheTemplateFile);
        
        // Generated script paths
        public const string GENERATED = "Generated";
        public const string MAP_ACTIONS = "MapActions";
        public const string MAP_CACHES = "MapCaches";
        public static string GeneratedFolderSystemPath => InputManagerFolderSystemPath + GENERATED + Sep;
        public static string GeneratedMapActionsSystemPath => GeneratedFolderSystemPath + MAP_ACTIONS + Sep;
        public static string GeneratedMapCacheSystemPath => GeneratedFolderSystemPath + MAP_CACHES + Sep;
        private static char Sep => Path.DirectorySeparatorChar;
        
        // String extensions for code generation
        public static string AsField(this string s) => s.AllWhitespaceTrimmed().LowercaseFirst();
        public static string AsType(this string s) => s.AllWhitespaceTrimmed().CapitalizeFirst();
        public static string AsEnumMember(this string s) => s.AlphaNumericCharactersOnly();
        
        public static void ClearFolder(string folderSystemPath)
        {
            if (!Directory.Exists(folderSystemPath))
            {
                Directory.CreateDirectory(folderSystemPath);
            }
            else
            {
                string[] filePaths = Directory.GetFiles(folderSystemPath);

                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
            }
        }

        public static void WriteLinesToFile(List<string> newLines, string filePath)
        {
            try
            {
                using (StreamWriter sw = new(filePath))
                {
                    foreach (string line in newLines)
                    {
                        sw.WriteLine(line);
                    }
                }

                Debug.Log($"{filePath} written successfully!");
            }
            catch (Exception e)
            {
                Debug.Log($"File could not be written: {e.Message}");
            }
        }

        public static List<string> GetGeneratorNoticeLines()
        {
            return new List<string>
            {
                $"// ------------------------------------------------------------------------",
                $"// This file was automatically generated by {nameof(InputScriptGenerator)}.",
                $"// ------------------------------------------------------------------------"
            };
        }

        public static IEnumerable<string> GetMapNames(InputActionAsset asset)
        {
            return asset.actionMaps.Select(map => map.name);
        }
        
        public static bool IsMarkerStart(string line, out string markerName)
        {
            string trimmedLine = line.Trim();
            bool isMarkerStart = trimmedLine.StartsWith(MARKER) && trimmedLine.EndsWith(START);

            if (!isMarkerStart)
            {
                markerName = string.Empty;
                return false;
            }
            
            StringBuilder sb = new();
            int periodCount = 0;
            foreach (char c in trimmedLine)
            {
                if (c == '.')
                {
                    periodCount++;
                    if (periodCount == 2)
                    {
                        break;
                    }
                    continue;
                }

                if (periodCount == 1)
                {
                    sb.Append(c);
                }
            }

            markerName = sb.ToString();
            return true;
        }

        public static bool IsMarkerEnd(string line)
        {
            string trimmedLine = line.Trim();
            return trimmedLine.StartsWith(MARKER) && trimmedLine.EndsWith(END);
        }
        
        private static string GetNamespace(string filePath)
        {
            const string namespaceString = "namespace";
            
            try
            {
                using StreamReader sr = new(filePath);
                while (sr.ReadLine() is { } line)
                {
                    if (line.StartsWith(namespaceString))
                    {
                        return line.Replace(namespaceString, string.Empty).Trim();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"The file could not be read: {e.Message}");
            }

            return string.Empty;
        }
    }
}