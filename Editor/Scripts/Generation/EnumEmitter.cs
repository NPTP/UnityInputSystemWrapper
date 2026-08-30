using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Enums;
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
            GeneratableEnum controlScheme = SourceGen.NewEnum("ControlScheme", AccessModifier.Public)
                .InNamespace(GeneratedNamespaces.ENUMS)
                .WithMember("None", -1);

            for (int i = 0; i < asset.controlSchemes.Count; i++)
            {
                controlScheme.WithMember(asset.controlSchemes[i].name.AsEnumMember(), i);
            }

            GeneratableTypeDefinition extensions = SourceGen.NewStaticClass("ControlSchemeExtensions", AccessModifier.Public)
                .InNamespace(GeneratedNamespaces.ENUMS)
                .WithDirective(GeneratedNamespaces.ROOT)
                .WithMethod(SourceGen.NewMethod("ToId")
                    .Internal()
                    .Returning("ControlSchemeId")
                    .Extending("ControlScheme", "controlScheme")
                    .Expression("InputRuntime.Current.GetControlSchemeId((int)controlScheme)"))
                .WithMethod(SourceGen.NewMethod("IsMouseBased")
                    .Public()
                    .Returning<bool>()
                    .Extending("ControlScheme", "controlScheme")
                    .Expression("controlScheme.ToId().IsMouseBased"))
                .WithMethod(SourceGen.NewMethod("IsGamepadBased")
                    .Public()
                    .Returning<bool>()
                    .Extending("ControlScheme", "controlScheme")
                    .Expression("controlScheme.ToId().IsGamepadBased"));

            return SourceGen.NewFile()
                .WithHeaderComment(Helper.GetGeneratorNoticeLines().ToArray())
                .Containing(controlScheme, extensions);
        }

        internal static GeneratableFile BuildInputContextFile(InputContextInfo[] inputContexts)
        {
            GeneratableEnum inputContext = SourceGen.NewEnum("InputContext", AccessModifier.Public)
                .InNamespace(GeneratedNamespaces.ENUMS);

            List<string> headerComment = new(Helper.GetGeneratorNoticeLines());

            if (inputContexts == null || inputContexts.Length == 0)
            {
                headerComment.Add("// >>> WARNING: No InputContexts have been defined in your OfflineInputData asset.");
                headerComment.Add("// >>> Add at least 1 InputContext, then re-run input code generation.");
            }
            else
            {
                for (int i = 0; i < inputContexts.Length; i++)
                {
                    inputContext.WithMember(inputContexts[i].Name.AsEnumMember(), i);
                }
            }

            GeneratableTypeDefinition extensions = SourceGen.NewStaticClass("InputContextExtensions", AccessModifier.Public)
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
    }
}
