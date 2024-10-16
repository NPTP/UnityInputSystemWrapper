using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Bindings
{
    internal static class BindingSaveLoad
    {
        internal static void LoadBindingsFromDiskForPlayer(InputPlayer inputPlayer)
        {
            string bindingsJson = PlayerPrefs.GetString(GetPlayerPrefsBindingsKey(inputPlayer.ID));
            if (!string.IsNullOrEmpty(bindingsJson))
                inputPlayer.Asset.LoadBindingOverridesFromJson(bindingsJson);
        }

        internal static void SaveBindingsToDiskForPlayer(InputPlayer inputPlayer)
        {
            string bindingsJson = inputPlayer.Asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(GetPlayerPrefsBindingsKey(inputPlayer.ID), bindingsJson);
        }
        
        private static string GetPlayerPrefsBindingsKey(PlayerID playerID) => $"{playerID}Bindings";
    }
}