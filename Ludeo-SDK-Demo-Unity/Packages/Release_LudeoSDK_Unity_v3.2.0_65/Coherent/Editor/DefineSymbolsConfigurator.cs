using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

namespace cohtml
{
    public class DefineSymbolsConfigurator
    {
        [Obsolete("This define symbol is obsolete. Use RenderPipelineDefineSymbol instead.")]
        private const string HDRPDefineSymbol = "COHERENT_SUPPORT_HDRP";

        // Define rendering pipelines.
        private const string RenderPipelineDefineSymbol = "COHERENT_RENDERING_PIPELINES";

        private const string DefineSymbol32Bit = "COHERENT_CPU_32BIT";
        private const string PackagePipelines = "Packages/com.unity.render-pipelines";
        private const string HDRPPackage = PackagePipelines + ".high-definition";
        private const string URPPackage = PackagePipelines + ".universal";

        private static string s_HDRPDirectory;
        private static string s_URPDirectory;
        private static bool s_LastRPConfiguration;

        private static bool s_LastEditorArchConfiguration;

        private static bool s_IsLastBuildArch32Bit;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.update += OnApplicationUpdate;
        }

        private static void OnApplicationUpdate()
        {
            SetupRPDefine();
            AddEditorArchitectureDefine();
            AddBuildArchitectureDefine();
        }

        public static void SetupRPDefine()
        {
            bool RPExistsAndAssigned = GraphicsSettings.renderPipelineAsset != null;
            if (s_LastRPConfiguration == RPExistsAndAssigned)
            {
                return;
            }

            s_LastRPConfiguration = RPExistsAndAssigned;
            if (RPExistsAndAssigned)
            {
                AddDefineSymbols(RenderPipelineDefineSymbol);
                LogHandler.Log("HDRP/URP mode enabled.");
            }
            else
            {
                RemoveDefineSymbols(RenderPipelineDefineSymbol);
            }
        }

        private static void AddEditorArchitectureDefine()
        {
#if UNITY_EDITOR
            if (s_LastEditorArchConfiguration == Environment.Is64BitProcess)
            {
                return;
            }

            s_LastEditorArchConfiguration = Environment.Is64BitProcess;
            if (Environment.Is64BitProcess)
            {
                RemoveDefineSymbols(DefineSymbol32Bit);
            }
            else
            {
                AddDefineSymbols(DefineSymbol32Bit);
            }
#endif
        }

        private static void AddBuildArchitectureDefine()
        {
            bool isAndroid32Bit = PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARMv7
#if UNITY_2020_1_OR_NEWER
                || PlayerSettings.Android.targetArchitectures == AndroidArchitecture.X86
#endif
                ;

            bool isActiveBuildWindowsOrAndroid32Bit =
                (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows && !Environment.Is64BitProcess) ||
                (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && isAndroid32Bit);

            if (s_IsLastBuildArch32Bit == isActiveBuildWindowsOrAndroid32Bit)
            {
                return;
            }

            s_IsLastBuildArch32Bit = isActiveBuildWindowsOrAndroid32Bit;
            if (isActiveBuildWindowsOrAndroid32Bit)
            {
                AddDefineSymbols(DefineSymbol32Bit);
            }
            else
            {
                RemoveDefineSymbols(DefineSymbol32Bit);
            }
        }

        public static void AddDefineSymbols(params string[] symbolsToAdd)
        {
            List<string> allDefines = GetDefinesAsCollection();
            IEnumerable<string> notExistInDefines = symbolsToAdd.Except(allDefines);
            if (notExistInDefines.Any())
            {
                allDefines.AddRange(notExistInDefines);
                ApplyDefineSymbols(allDefines);
            }
        }

        public static void RemoveDefineSymbols(params string[] symbolsToRemove)
        {
            List<string> allDefines = GetDefinesAsCollection();
            List<string> clearedDefines = allDefines.Except(symbolsToRemove).ToList();
            if (!allDefines.All(clearedDefines.Contains))
            {
                ApplyDefineSymbols(clearedDefines);
            }
        }

        public static List<string> GetDefinesAsCollection()
        {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';'));
        }

        private static void ApplyDefineSymbols(List<string> symbols)
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", symbols));
            }
        }
    }
}