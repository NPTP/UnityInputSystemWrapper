using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NPTP.InputSystemWrapper.Actions;
using NPTP.InputSystemWrapper.Bindings;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class Helper
    {
        private const string MARKER = "// MARKER";
        private const string START = "Start";
        private const string END = "End";
        
        // Assets
        internal static InputActionAsset InputActionAsset => EditorAssetGetter.GetFirst<RuntimeInputData>().InputActionAsset;
        internal static OfflineInputData OfflineInputData => EditorAssetGetter.GetFirst<OfflineInputData>();
        internal static string InputNamespace => GetNamespace(InputManagerFileSystemPath);
        
        // Existing script paths
        internal static string InputManagerFileSystemPath => EditorScriptGetter.GetSystemFilePath(typeof(Input));
        internal static string InputPlayerFileSystemPath => EditorScriptGetter.GetSystemFilePath<InputPlayer>();
        internal static string ControlSchemeFileSystemPath => EditorScriptGetter.GetSystemFilePath<ControlScheme>();
        internal static string InputContextFileSystemPath => EditorScriptGetter.GetSystemFilePath<InputContext>();
        internal static string PlayerIDFileSystemPath => EditorScriptGetter.GetSystemFilePath<PlayerID>();
        internal static string InputUserChangeInfoFileSystemPath => EditorScriptGetter.GetSystemFilePath<InputUserChangeInfo>();
        internal static string RuntimeInputDataFileSystemPath => EditorScriptGetter.GetSystemFilePath<RuntimeInputData>();
        internal static string BindingChangerFileSystemPath => EditorScriptGetter.GetSystemFilePath(typeof(BindingChanger));
        private static string InputManagerFolderSystemPath => EditorScriptGetter.GetSystemFolderPath(typeof(Input));
        
        // Template paths
        internal static string ActionsTemplateFileSystemPath => EditorAssetGetter.GetSystemFilePath(OfflineInputData.ActionsTemplateFile);
        
        // Generated script paths
        internal const string GENERATED = "Generated";
        internal const string ACTIONS = "Actions";
        internal static string GeneratedFolderSystemPath => InputManagerFolderSystemPath + GENERATED + Sep;
        internal static string GeneratedActionsSystemPath => GeneratedFolderSystemPath + Sep + ACTIONS + Sep;
        private static char Sep => Path.DirectorySeparatorChar;
        
        // String extensions for code generation
        internal static string AsField(this string s) => s.AlphaNumericCharactersOnly().RemoveFirstCharacterIfNumber().AllWhitespaceTrimmed().LowercaseFirst();
        internal static string AsProperty(this string s) => s.AllWhitespaceTrimmed().CapitalizeFirst();
        internal static string AsType(this string s) => s.AllWhitespaceTrimmed().CapitalizeFirst();
        internal static string AsEnumMember(this string s) => s.AlphaNumericCharactersOnly().RemoveFirstCharacterIfNumber();
        internal static string AsInspectorLabel(this string s) => s.SpaceBetweenWords().CapitalizeFirst();
        
        internal static void ClearFolder(string folderSystemPath)
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

        internal static void WriteLinesToFile(List<string> newLines, string filePath)
        {
            try
            {
                int sepIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
                if (sepIndex >= 0)
                {
                    string directoryPath = filePath.Remove(sepIndex);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }

                using (StreamWriter sw = new(filePath))
                {
                    foreach (string line in newLines)
                    {
                        sw.WriteLine(line);
                    }
                }

                NPTPDebug.Log($"{filePath} written successfully!");
            }
            catch (Exception e)
            {
                NPTPDebug.Log($"File could not be written: {e.Message}");
            }
        }

        internal static List<string> GetGeneratorNoticeLines()
        {
            return new List<string>
            {
                $"// ------------------------------------------------------------------------------------------",
                $"// This file was automatically generated by {nameof(InputScriptGenerator)}. Do not modify it manually.",
                $"// ------------------------------------------------------------------------------------------"
            };
        }

        internal static IEnumerable<string> GetMapNames(InputActionAsset asset)
        {
            return asset.actionMaps.Select(map => map.name);
        }
        
        internal static bool IsMarkerStart(string line, out string markerName)
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

        internal static bool IsMarkerEnd(string line)
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
                NPTPDebug.LogError($"The file could not be read: {e.Message}");
            }

            return string.Empty;
        }
    }
}