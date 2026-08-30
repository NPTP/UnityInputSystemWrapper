using System.Linq;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEditor;

namespace NPTP.InputSystemWrapper.Editor
{
    internal class InputAssetsPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            Generation.ProjectAssets.TryFindProjectAsset("InputData", out InputData inputData);
            if (inputData == null || inputData == null || inputData.InputActionAsset == null)
            {
                return;
            }

            if (importedAssets.Any(importedAsset => importedAsset.EndsWith($"{inputData.InputActionAsset.name}.inputactions") ||
                                                    importedAsset.EndsWith($"{inputData.name}.asset")))
            {
                InputScriptGenerator.GenerateInputScriptCode();
            }
        }
    }
}