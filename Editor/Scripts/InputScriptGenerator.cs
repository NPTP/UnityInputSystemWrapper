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
            InputData inputData = ProjectAssets.EnsureProjectAssets();
            if (inputData == null)
            {
                GenerationReport.End();
                return;
            }

            InputActionAsset asset = inputData.InputActionAsset;
            if (asset == null)
            {
                Debug.LogError($"Can't generate InputSystemWrapper code: You need to specify an InputActionAsset in the {nameof(InputData)} asset first. Aborting...");
                GenerationReport.End();
                return;
            }

            // All generated code goes into its own assembly in the consuming project, so that the package
            // itself never has to be written to and can be installed read-only.
            string outputFolder = GeneratedAssembly.GetOrCreateFolderAssetPath();

            string actionsFolder = $"{outputFolder}/{GeneratedAssembly.ACTIONS_SUBFOLDER}";
            foreach (InputActionMap map in asset.actionMaps)
            {
                WriteType($"{map.name.AsType()}Actions", ActionsEmitter.Build(map), actionsFolder);
            }

            WriteFile("ControlScheme", EnumEmitter.BuildControlSchemeFile(asset), outputFolder);
            WriteFile("InputContext", EnumEmitter.BuildInputContextFile(inputData.AuthoredContexts), outputFolder);
            WriteFile("Extensions", ExtensionsEmitter.BuildFile(), outputFolder);
            WriteType(InputPlayerRefEmitter.TYPE_NAME, InputPlayerRefEmitter.Build(asset, inputData), outputFolder);
            WriteType("ISW", ISWEmitter.Build(asset, inputData), outputFolder);
            WriteFile("BindingDataMenuItems", BindingDataMenuEmitter.BuildFile(), outputFolder);

            GeneratedAssembly.PruneStaleScripts(outputFolder);

            BindingAudit.Run(asset);

            // Control scheme metadata, input contexts and rebinding paths are data, so they are written into
            // the InputData asset.
            InputDataSynchronizer.Synchronize(inputData);

            GenerationReport.LogAndEnd("Input wrapper generation complete");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
        private static void WriteType(string fileName, GeneratableDefinition generatable, string outputFolder)
        {
            GeneratableFile file = SourceGen.NewFile()
                .WithHeaderComment(ISWEditorHelper.GetGeneratorNoticeLines().ToArray())
                .Containing(generatable);

            WriteFile(fileName, file, outputFolder);
        }

        private static void WriteFile(string fileName, GeneratableFile file, string outputFolder)
        {
            string assetPath = $"{outputFolder}/{fileName}.cs";
            GenerationReport.RecordWrite(assetPath, SourceGen.WriteToPath(assetPath, file));
        }
    }
}
