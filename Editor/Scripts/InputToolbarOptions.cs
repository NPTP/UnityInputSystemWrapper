using UnityEditor;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Editor
{
    public static class InputToolbarOptions
    {
        private const string TOOLBAR_NAME = "Input";
        private const string REGENERATE_INPUT_CODE = TOOLBAR_NAME + "/Regenerate C# Input Manager Code";
        private const string OFFLINE_INPUT_DATA = TOOLBAR_NAME + "/Offline Input Data";
        private const string RUNTIME_INPUT_DATA = TOOLBAR_NAME + "/Runtime Input Data";

        [MenuItem(REGENERATE_INPUT_CODE)]
        private static void RegenerateInputCode()
        {
            InputScriptGenerator.GenerateInputScriptCode();
        }

        [MenuItem(OFFLINE_INPUT_DATA)]
        private static void OfflineInputData()
        {
            SelectAsset(Helper.OfflineInputData);
        }

        [MenuItem(RUNTIME_INPUT_DATA)]
        private static void RuntimeInputData()
        {
            SelectAsset(Helper.OfflineInputData.RuntimeInputData);
        }

        private static void SelectAsset(Object asset)
        {
            if (asset == null)
            {
                return;
            }

            Selection.activeObject = asset;
        }
    }
}