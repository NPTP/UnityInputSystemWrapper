using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the ControlScheme and InputContext enums and the extension methods that convert them to the
    /// id types the package runtime works in. Enum values are emitted explicitly, in asset order, so that
    /// the cast between an enum value and an id index stays exact even if the asset is reordered.
    /// </summary>
    internal static class EnumEmitter
    {
        internal static GeneratableFile BuildControlSchemeFile(InputActionAsset asset)
        {
            GeneratableEnum controlScheme = SourceGen.NewEnum("ControlScheme").Public()
                .InNamespace(GeneratedNamespaces.ENUMS)
                .WithMember("None", -1);

            for (int i = 0; i < asset.controlSchemes.Count; i++)
            {
                controlScheme.WithMember(asset.controlSchemes[i].name, i);
            }

            GeneratableTypeDefinition extensions = SourceGen.NewClass("ControlSchemeExtensions").Public().Static()
                .InNamespace(GeneratedNamespaces.ENUMS)
                .WithDirective(GeneratedNamespaces.ROOT)
                .WithMethod(SourceGen.NewMethod("ToId")
                    .Internal()
                    .Returning("ControlSchemeId")
                    .Extending("ControlScheme", "controlScheme")
                    .Expression("InputRuntime.Current.GetControlSchemeId((int)controlScheme)"))
                .WithMethods(BuildDeviceFamilyMethods());

            return SourceGen.NewFile()
                .WithHeaderComment(ISWEditorHelper.GetGeneratorNoticeLines().ToArray())
                .Containing(controlScheme, extensions);
        }

        internal static GeneratableFile BuildInputContextFile(InputContextInfo[] inputContexts)
        {
            GeneratableEnum inputContext = SourceGen.NewEnum("InputContext").Public()
                .InNamespace(GeneratedNamespaces.ENUMS);

            List<string> headerComment = new(ISWEditorHelper.GetGeneratorNoticeLines());

            if (inputContexts == null || inputContexts.Length == 0)
            {
                headerComment.Add("// >>> WARNING: No InputContexts have been defined in your InputData asset.");
                headerComment.Add("// >>> Add at least 1 InputContext, then re-run input code generation.");
            }
            else
            {
                for (int i = 0; i < inputContexts.Length; i++)
                {
                    inputContext.WithMember(inputContexts[i].Name, i);
                }
            }

            GeneratableTypeDefinition extensions = SourceGen.NewClass("InputContextExtensions").Public().Static()
                .InNamespace(GeneratedNamespaces.ENUMS)
                .WithMethod(SourceGen.NewMethod("ToId")
                    .Internal()
                    .Returning("InputContextId")
                    .Extending("InputContext", "inputContext")
                    .Expression("new((int)inputContext)"));

            return SourceGen.NewFile()
                .WithHeaderComment(headerComment.ToArray())
                .Containing(inputContext, extensions);
        }

        /// <summary>
        /// One test per device family, so a call site can ask what a control scheme uses without naming
        /// the id struct behind it.
        /// </summary>
        private static GeneratableMethod[] BuildDeviceFamilyMethods()
        {
            string[] families =
            {
                "UsesPointer", "UsesGamepad", "UsesKeyboard",
                "UsesJoystick", "UsesSensor", "UsesTrackedDevice"
            };

            GeneratableMethod[] methods = new GeneratableMethod[families.Length];
            for (int i = 0; i < families.Length; i++)
            {
                methods[i] = SourceGen.NewMethod(families[i])
                    .Public()
                    .Returning<bool>()
                    .Extending("ControlScheme", "controlScheme")
                    .Expression($"controlScheme.ToId().{families[i]}");
            }

            return methods;
        }
    }
}
