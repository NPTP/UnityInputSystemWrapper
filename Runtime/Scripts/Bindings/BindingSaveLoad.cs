using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingSaveLoad
    {
        public static void LoadBindingsFromDiskForPlayer(InputPlayer inputPlayer)
        {
            // TODO: to JSON with other settings
            string bindingsJson = PlayerPrefs.GetString(GetPlayerPrefsBindingsKey(inputPlayer.ID));
            if (!string.IsNullOrEmpty(bindingsJson))
                inputPlayer.Asset.LoadBindingOverridesFromJson(bindingsJson);
        }

        public static void SaveBindingsToDiskForPlayer(InputPlayer inputPlayer)
        {
            // TODO: to JSON with other settings
            string bindingsJson = inputPlayer.Asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(GetPlayerPrefsBindingsKey(inputPlayer.ID), bindingsJson);
        }

        private static string GetPlayerPrefsBindingsKey(PlayerID playerID) => $"{playerID}Bindings";
    }
}