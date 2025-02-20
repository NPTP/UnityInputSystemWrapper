using System.IO;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingSaveLoad
    {
        private const string FILE_TYPE = "json";
        private const string BINDING_FILE_NAME_PREFIX = "InputBindingOverrides_";
        
        private static string GetBindingFilePathForPlayer(PlayerID playerID)
        {
            return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{BINDING_FILE_NAME_PREFIX}{playerID.ToString()}.{FILE_TYPE}";
        }
        
        internal static void LoadBindingsFromDiskForPlayer(InputPlayer inputPlayer)
        {
            string filePath = GetBindingFilePathForPlayer(inputPlayer.ID);
            if (!FileReadWrite.TryReadLinesFromFile(filePath, out string fileContents))
            {
                ISWDebug.LogWarning($"Couldn't load binding overrides for {inputPlayer.ID.ToString()} at path: {filePath}. Aborting...");
                return;
            }
            
            inputPlayer.Asset.LoadBindingOverridesFromJson(fileContents);
        }

        internal static void SaveBindingsToDiskForPlayer(InputPlayer inputPlayer)
        {
            string fileContents = inputPlayer.Asset.SaveBindingOverridesAsJson();
            string filePath = GetBindingFilePathForPlayer(inputPlayer.ID);
            if (!FileReadWrite.TryWriteToFile(filePath, fileContents))
            {
                ISWDebug.LogWarning($"Couldn't write binding overrides for {inputPlayer.ID.ToString()} to path: {filePath}. Aborting...");
            }
        }
    }
}