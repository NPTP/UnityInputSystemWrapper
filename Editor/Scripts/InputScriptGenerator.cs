using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Generation;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class InputScriptGenerator
    {
        internal static void GenerateInputScriptCode()
        {
            GenerationReport.Begin();

            // The package may be read-only, so the assets the generator writes to live in the project.
            OfflineInputData offlineInputData = ProjectAssets.EnsureProjectAssets();
            if (offlineInputData == null)
            {
                GenerationReport.End();
                return;
            }

            InputActionAsset asset = offlineInputData.RuntimeInputData == null ? null : offlineInputData.RuntimeInputData.InputActionAsset;
            if (asset == null)
            {
                Debug.LogError($"Can't generate InputSystemWrapper code: You need to specify an InputActionAsset in the {nameof(RuntimeInputData)} asset first. Aborting...");
                GenerationReport.End();
                return;
            }

            // All generated code goes into its own assembly in the consuming project, so that the package
            // itself never has to be written to and can be installed read-only.
            string outputFolder = GeneratedAssembly.GetOrCreateFolderAssetPath();

            foreach (InputActionMap map in asset.actionMaps)
            {
                WriteType($"{map.name.AsType()}Actions", ActionsEmitter.Build(map), outputFolder);
            }

            WriteFile("ControlScheme", EnumEmitter.BuildControlSchemeFile(asset), outputFolder);
            WriteFile("InputContext", EnumEmitter.BuildInputContextFile(offlineInputData.InputContexts), outputFolder);
            WriteFile("InputPlayerExtensions", InputPlayerExtensionsEmitter.BuildFile(asset), outputFolder);
            WriteType("ISW", ISWEmitter.Build(asset, offlineInputData), outputFolder);
            WriteFile("BindingDataMenuItems", BindingDataMenuEmitter.BuildFile(), outputFolder);

            GeneratedAssembly.PruneStaleScripts(outputFolder);

            // Control scheme metadata, input contexts and rebinding paths are plain data, so they get written
            // into the RuntimeInputData asset rather than generated as C#.
            RuntimeInputDataSynchronizer.Synchronize(offlineInputData);

            GenerationReport.LogAndEnd("Input wrapper generation complete");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void WriteType(string fileName, GeneratableBase generatable, string outputFolder)
        {
            string assetPath = $"{outputFolder}/{fileName}.cs";
            GenerationReport.RecordWrite(assetPath, SourceGen.WriteToPath(assetPath, generatable));
        }

        private static void WriteFile(string fileName, GeneratableFile file, string outputFolder)
        {
            string assetPath = $"{outputFolder}/{fileName}.cs";
            GenerationReport.RecordWrite(assetPath, SourceGen.WriteToPath(assetPath, file));
        }
    }
}
