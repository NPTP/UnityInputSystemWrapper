using System;
using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Generation;
using NPTP.InputSystemWrapper.Editor.ScriptContentBuilders;
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
            InputActionAsset asset = offlineInputData.RuntimeInputData == null ? null : offlineInputData.RuntimeInputData.InputActionAsset;
            if (asset == null)
            {
                Debug.LogError($"Can't generate InputSystemWrapper code: You need to specify an InputActionAsset in the {nameof(RuntimeInputData)} asset first. Aborting...");
                return;
            }

            // All generated code goes into its own assembly in the consuming project, so that the package
            // itself never has to be written to and can be installed read-only.
            string outputFolder = GeneratedAssembly.GetOrCreateFolderSystemPath();
            Helper.ClearGeneratedScripts(outputFolder);

            GenerateActionClasses(asset, outputFolder);
            Helper.WriteLinesToFile(EnumEmitter.BuildControlSchemeLines(asset), outputFolder + "ControlScheme.cs");
            Helper.WriteLinesToFile(EnumEmitter.BuildInputContextLines(offlineInputData.InputContexts), outputFolder + "InputContext.cs");
            Helper.WriteLinesToFile(InputPlayerExtensionsEmitter.BuildLines(asset), outputFolder + "InputPlayerExtensions.cs");
            Helper.WriteLinesToFile(ISWEmitter.BuildLines(asset, offlineInputData), outputFolder + "ISW.cs");

            // Control scheme metadata, input contexts and rebinding paths are plain data, so they get written
            // into the RuntimeInputData asset rather than generated as C#.
            RuntimeInputDataSynchronizer.Synchronize(offlineInputData);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void GenerateActionClasses(InputActionAsset asset, string outputFolder)
        {
            foreach (InputActionMap map in asset.actionMaps)
            {
                GenerateFile(map,
                    Helper.ActionsTemplateFileSystemPath,
                    ActionsContentBuilder.AddContent,
                    outputFolder + map.name.AsType() + "Actions.cs");
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
    }
}
