using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.EditorWindows;
using UnityEditor;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace NPTP.InputSystemWrapper.Editor
{
    internal static class InputToolbarOptions
    {
        private const string TOOLBAR_NAME = "Input";
        private const string REGENERATE_INPUT_CODE_ASSETS = TOOLBAR_NAME + "/Regenerate Input Wrapper Code and Assets";
        private const string INPUT_DATA = TOOLBAR_NAME + "/Input Data";
        private const string INPUT_ACTIONS_ASSET = TOOLBAR_NAME + "/Input Actions Asset";
        private const string OPEN_DEBUGGER_WINDOW = TOOLBAR_NAME + "/Input Wrapper Debugger Window";

        [MenuItem(REGENERATE_INPUT_CODE_ASSETS, isValidateFunction: false, 0)]
        private static void RegenerateInputCode()
        {
            InputScriptGenerator.GenerateInputScriptCode();
        }

        [MenuItem(INPUT_DATA, isValidateFunction: false, 100)]
        private static void InputData()
        {
            SelectAsset(ISWEditorHelper.InputData);
        }

        [MenuItem(INPUT_ACTIONS_ASSET, isValidateFunction: true, 101)]
        private static bool ValidateInputActionsAsset()
        {
            return TryGetInputActionAsset(out _);
        }

        [MenuItem(INPUT_ACTIONS_ASSET, isValidateFunction: false, 101)]
        private static void InputActionsAsset()
        {
            if (TryGetInputActionAsset(out InputActionAsset asset))
            {
                AssetDatabase.OpenAsset(asset);
            }
        }

        /// <summary>The action asset the input data names, or false when it names none.</summary>
        private static bool TryGetInputActionAsset(out InputActionAsset asset)
        {
            InputData inputData = ISWEditorHelper.InputData;
            asset = inputData == null ? null : inputData.InputActionAsset;
            return asset != null;
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