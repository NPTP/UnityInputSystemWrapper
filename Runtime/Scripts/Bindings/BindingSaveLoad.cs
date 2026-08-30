using System;
using System.Collections.Generic;
using System.IO;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Player;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingSaveLoad
    {
        private const string FILE_TYPE = "json";
        private const string BINDING_FILE_NAME_PREFIX = "InputBindingOverrides_";

        private static string GetBindingFilePathForPlayer(int playerID)
        {
            return $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}{BINDING_FILE_NAME_PREFIX}PlayerID{playerID}.{FILE_TYPE}";
        }

        /// <summary>
        /// Load a player's saved bindings from wherever the mode says they live. With both sources in
        /// play the project's own storage wins, since it is the one that can outlive a reinstall.
        /// </summary>
        internal static void LoadBindingsForPlayer(InputPlayer inputPlayer, BindingSerializationMode mode)
        {
            string json = null;

            if (mode.UsesEvent())
            {
                BindingsLoadRequest request = new(inputPlayer.ID);
                InputRuntime.Current.BroadcastBindingsLoadRequested(request);
                json = request.json;
            }

            if (string.IsNullOrEmpty(json) && mode.UsesFile() && !TryReadFile(inputPlayer, out json))
            {
                return;
            }

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            inputPlayer.Asset.LoadBindingOverridesFromJson(DropOverridesWithNoBinding(inputPlayer, json));
        }

        /// <summary>Serialize a player's bindings out to everything the mode names.</summary>
        internal static void SaveBindingsForPlayer(InputPlayer inputPlayer, BindingSerializationMode mode)
        {
            string json = inputPlayer.Asset.SaveBindingOverridesAsJson();

            if (mode.UsesFile())
            {
                string filePath = GetBindingFilePathForPlayer(inputPlayer.ID);
                if (!FileReadWrite.TryWriteToFile(filePath, json))
                {
                    ISWDebug.LogWarning($"Couldn't write binding overrides for {inputPlayer.ID.ToString()} to path: {filePath}. Aborting...");
                }
            }

            if (mode.UsesEvent())
            {
                InputRuntime.Current.BroadcastBindingsSaveRequested(new BindingsSaveRequest(inputPlayer.ID, json));
            }
        }

        private static bool TryReadFile(InputPlayer inputPlayer, out string fileContents)
        {
            string filePath = GetBindingFilePathForPlayer(inputPlayer.ID);
            if (FileReadWrite.TryReadLinesFromFile(filePath, out fileContents))
            {
                return true;
            }

            ISWDebug.LogWarning($"Couldn't load binding overrides for {inputPlayer.ID.ToString()} at path: {filePath}. Aborting...");
            return false;
        }

        /// <summary>
        /// Saved overrides name the binding they belong to by id, so one whose binding has since been
        /// removed from the input action asset can no longer be applied. Those are dropped here and
        /// reported once, rather than left for the input system to warn about one at a time.
        /// </summary>
        private static string DropOverridesWithNoBinding(InputPlayer inputPlayer, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            OverrideListJson overrideList;
            try
            {
                overrideList = JsonUtility.FromJson<OverrideListJson>(json);
            }
            catch (Exception e)
            {
                ISWDebug.LogWarning($"Saved bindings for player {inputPlayer.ID.ToString()} could not be read ({e.Message}). Loading them as they are.");
                return json;
            }

            if (overrideList?.bindings == null)
            {
                return json;
            }

            HashSet<string> bindingIds = new();
            foreach (InputBinding binding in inputPlayer.Asset.bindings)
            {
                bindingIds.Add(binding.id.ToString());
            }

            List<OverrideJson> applicable = new();
            int droppedCount = 0;
            foreach (OverrideJson bindingOverride in overrideList.bindings)
            {
                if (bindingIds.Contains(bindingOverride.id))
                {
                    applicable.Add(bindingOverride);
                }
                else
                {
                    droppedCount++;
                }
            }

            if (droppedCount == 0)
            {
                return json;
            }

            ISWDebug.LogWarning($"{droppedCount.ToString()} saved binding override(s) for player {inputPlayer.ID.ToString()} " +
                                $"refer to bindings no longer in {inputPlayer.Asset.name} and were skipped. The rest were loaded.");

            overrideList.bindings = applicable;
            return applicable.Count == 0 ? string.Empty : JsonUtility.ToJson(overrideList);
        }

        /// <summary>
        /// Mirrors the input system's own override format, which is internal to it. Field names have to
        /// match exactly for the round trip through JsonUtility to hold.
        /// </summary>
        [Serializable]
        private class OverrideListJson
        {
            public List<OverrideJson> bindings;
        }

        [Serializable]
        private class OverrideJson
        {
            public string action;
            public string id;
            public string path;
            public string interactions;
            public string processors;
        }
    }
}
