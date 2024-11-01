using System;
using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Editor.ScriptContentBuilders;
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
            InputActionAsset asset = Helper.InputActionAsset;
            
            Helper.ClearFolder(Helper.GeneratedFolderSystemPath);
            GenerateActionClasses(asset);
            
            ModifyExistingFile(asset, Helper.ControlSchemeFileSystemPath, ControlSchemeContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.InputContextFileSystemPath, InputContextContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.PlayerIDFileSystemPath, PlayerIDContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.InputPlayerFileSystemPath, InputPlayerContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.InputManagerFileSystemPath, InputManagerContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.InputUserChangeInfoFileSystemPath, InputUserChangeInfoContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.RuntimeInputDataFileSystemPath, RuntimeInputDataContentBuilder.AddContent);
            ModifyExistingFile(asset, Helper.BindingChangerFileSystemPath, BindingChangerContentBuilder.AddContent);
            
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
                NPTPDebug.Log($"The file could not be read: {e.Message}");
                return;
            }

            Helper.WriteLinesToFile(newLines, writePath);
        }

        private static void ModifyExistingFile(InputActionAsset asset, string filePath, Action<InputActionAsset, string, List<string>> markerSectionAction)
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
                                markerSectionAction?.Invoke(asset, markerName, newLines);
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
                NPTPDebug.Log($"The file could not be read: {e.Message}");
                return;
            }

            Helper.WriteLinesToFile(newLines, filePath);
        }
    }
}