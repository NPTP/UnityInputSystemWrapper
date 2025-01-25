using System;
using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.ScriptContentBuilders;
using NPTP.InputSystemWrapper.Editor.Utilities;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class InputScriptGenerator
    {
        private enum ReadState
        {
            Normal = 0,
            WaitingForMarkerEnd
        }
        
        internal static void GenerateInputScriptCode()
        {
            OfflineInputData offlineInputData = Helper.OfflineInputData;
            if (offlineInputData.RuntimeInputData.InputActionAsset == null)
            {
                Debug.LogError($"Can't generate InputSystemWrapper code: You need to specify an InputActionAsset in the {nameof(RuntimeInputData)} asset first. Aborting...");
                return;
            }
            
            Helper.ClearFolderRecursive(Helper.GeneratedFolderSystemPath);
            GenerateActionClasses(offlineInputData.RuntimeInputData.InputActionAsset);
            
            ModifyExistingFile(Helper.ControlSchemeFileSystemPath, new ControlSchemeContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.InputContextFileSystemPath, new InputContextContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.PlayerIDFileSystemPath, new PlayerIDContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.InputPlayerFileSystemPath, new InputPlayerContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.InputManagerFileSystemPath, new InputManagerContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.InputUserChangeInfoFileSystemPath, new InputUserChangeInfoContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.RuntimeInputDataFileSystemPath, new RuntimeInputDataContentBuilder(offlineInputData));
            ModifyExistingFile(Helper.BindingChangerFileSystemPath, new BindingChangerContentBuilder(offlineInputData));
            
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void GenerateActionClasses(InputActionAsset asset)
        {
            foreach (InputActionMap map in asset.actionMaps)
            {
                GenerateFile(map,
                    Helper.ActionsTemplateFileSystemPath,
                    ActionsContentBuilder.AddContent,
                Helper.GeneratedActionsSystemPath + map.name.AsType() + "Actions.cs");
            }
        }

        private static void GenerateFile(InputActionMap map, string readPath,
            Action<string, InputActionMap, List<string>> addContentAction, string writePath)
        {
            List<string> newLines = new();

            try
            {
                using StreamReader sr = new(readPath);
                ReadState readState = ReadState.Normal;
                while (sr.ReadLine() is { } line)
                {
                    switch (readState)
                    {
                        case ReadState.Normal:
                            if (Helper.IsMarkerStart(line, out string markerName))
                            {
                                addContentAction(markerName, map, newLines);
                                readState = ReadState.WaitingForMarkerEnd;
                            }
                            else
                            {
                                newLines.Add(line);
                            }

                            break;
                        case ReadState.WaitingForMarkerEnd:
                            if (Helper.IsMarkerEnd(line)) readState = ReadState.Normal;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                ISWDebug.Log($"The file could not be read: {e.Message}");
                return;
            }

            Helper.WriteLinesToFile(newLines, writePath);
        }

        private static void ModifyExistingFile(string filePath, ContentBuilder contentBuilder)
        {
            List<string> newLines = new();

            try
            {
                using StreamReader sr = new(filePath);
                ReadState readState = ReadState.Normal;
                while (sr.ReadLine() is { } line)
                {
                    switch (readState)
                    {
                        case ReadState.Normal:
                            newLines.Add(line);
                            if (Helper.IsMarkerStart(line, out string markerName))
                            {
                                InputScriptGeneratorMarkerInfo info = new(markerName, line.GetLeadingWhitespace(), newLines);
                                contentBuilder.AddContent(info);
                                readState = ReadState.WaitingForMarkerEnd;
                            }
                            break;
                        case ReadState.WaitingForMarkerEnd:
                            if (Helper.IsMarkerEnd(line))
                            {
                                newLines.Add(line);
                                readState = ReadState.Normal;
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                ISWDebug.Log($"The file could not be read: {e.Message}");
                return;
            }

            Helper.WriteLinesToFile(newLines, filePath);
        }
    }
}