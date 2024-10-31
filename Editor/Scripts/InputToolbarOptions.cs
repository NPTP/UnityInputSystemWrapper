using NPTP.InputSystemWrapper.Editor.EditorWindows;
using UnityEditor;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class InputToolbarOptions
    {
        private const string TOOLBAR_NAME = "Input";
        private const string REGENERATE_INPUT_CODE = TOOLBAR_NAME + "/Regenerate Input Wrapper Code";
        private const string OFFLINE_INPUT_DATA = TOOLBAR_NAME + "/Offline Input Data";
        private const string RUNTIME_INPUT_DATA = TOOLBAR_NAME + "/Runtime Input Data";
        private const string OPEN_DEBUGGER_WINDOW = TOOLBAR_NAME + "/Open Input Wrapper Debugger Window";

        [MenuItem(REGENERATE_INPUT_CODE, isValidateFunction: false, 0)]
        private static void RegenerateInputCode()
        {
            InputScriptGenerator.GenerateInputScriptCode();
        }

        [MenuItem(OFFLINE_INPUT_DATA, isValidateFunction: false, 100)]
        private static void OfflineInputData()
        {
            SelectAsset(Helper.OfflineInputData);
        }

        [MenuItem(RUNTIME_INPUT_DATA, isValidateFunction: false, 100)]
        private static void RuntimeInputData()
        {
            SelectAsset(Helper.OfflineInputData.RuntimeInputData);
        }

        [MenuItem(OPEN_DEBUGGER_WINDOW, isValidateFunction: false, 200)]
        private static void OpenDebuggerWindow()
        {
            EditorWindow.GetWindow(typeof(InputWrapperDebuggerWindow));
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