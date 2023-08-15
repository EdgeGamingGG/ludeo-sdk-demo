using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace cohtml
{
    public class ScriptingBackendAdjuster : Editor
    {
        private const string DllExtension = ".dll";
        private const string BundleExtension = ".bundle";
        private const string DllExtensionLinux = ".so";
        private const string DotNet = "cohtml.Net";
        private const string DotNetRestricted = "cohtml.Restricted";

        private const string Plugin = "cohtml_Unity3DPlugin";
        private const string PluginAndroid = "libcohtml_Unity3DPlugin";

        private const string Il2CppSuffix = ".IL2CPP";
        private const string x64 = "x86_64";
        private const string x86 = "x86";

        public static bool m_MonoEnabled;
        private static List<string> s_DefinesCollection;

        private const string AndroidRelativePath = "Android/";
        private const string CoherentEnableIl2Cpp = "COHERENT_ENABLE_IL2CPP";

        private static readonly string PREF_KEY_SCRIPTING_BACKEND = $"CohtmlPreviousScriptingBackend_{EditorUserBuildSettings.selectedBuildTargetGroup}";        

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.update -= OnApplicationUpdate;
            EditorApplication.update += OnApplicationUpdate;
        }

        private static ScriptingImplementation? GetPreviousScriptingBackend()
        {
            int previousSet = EditorPrefs.GetInt(PREF_KEY_SCRIPTING_BACKEND, int.MinValue);
            if (previousSet == int.MinValue)
                return null;
            else
                return (ScriptingImplementation)previousSet;
        }

        private static void OnApplicationUpdate()
        {
            UpdateCohtmlScriptingBackend();
        }

        public static void UpdateCohtmlScriptingBackend()
        {
            ScriptingImplementation? previousScriptingBackend = GetPreviousScriptingBackend();
            ScriptingImplementation selectedScriptingBackend = PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup);

            if ((previousScriptingBackend != null) && (selectedScriptingBackend == previousScriptingBackend))
                return;

            EditorPrefs.SetInt(PREF_KEY_SCRIPTING_BACKEND, (int)selectedScriptingBackend);

            m_MonoEnabled = selectedScriptingBackend == ScriptingImplementation.Mono2x;            

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    UpdateCohtmlScriptingBackendStandalone();
                    break;
                case BuildTarget.Android:
                    UpdateCohtmlScriptingBackendAndroid();
                    break;
            }

            AddIl2CppDefine();
            AssetDatabase.Refresh();
            Debug.Log($"[Cohtml] Switch Scripting backend to {selectedScriptingBackend}");
        }

        private static void AddIl2CppDefine()
        {
            if (m_MonoEnabled)
            {
                DefineSymbolsConfigurator.RemoveDefineSymbols(CoherentEnableIl2Cpp);
            }
            else
            {
                DefineSymbolsConfigurator.AddDefineSymbols(CoherentEnableIl2Cpp);
            }
        }

        private static void UpdateCohtmlScriptingBackendStandalone()
        {
            ChangeStandaloneWindowsAsset(DotNet, m_MonoEnabled);
            ChangeStandaloneWindowsAsset(DotNet + Il2CppSuffix, !m_MonoEnabled);

            ChangeStandaloneWindowsAsset(DotNetRestricted, m_MonoEnabled);
            ChangeStandaloneWindowsAsset(DotNetRestricted + Il2CppSuffix, !m_MonoEnabled);

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    ChangeStandaloneWindowsAsset(Plugin, m_MonoEnabled);
                    ChangeStandaloneWindowsAsset(Plugin + Il2CppSuffix, !m_MonoEnabled);
                    break;
                case BuildTarget.StandaloneOSX:
                    ChangeStandaloneMacAsset(Plugin, m_MonoEnabled);
                    ChangeStandaloneMacAsset(Plugin + Il2CppSuffix, !m_MonoEnabled);
                    break;
            }
        }

        private static void UpdateCohtmlScriptingBackendAndroid()
        {
            ChangeStandaloneWindowsAsset(DotNet, m_MonoEnabled);
            ChangeStandaloneWindowsAsset(DotNet + Il2CppSuffix, !m_MonoEnabled);

            ChangeStandaloneWindowsAsset(DotNetRestricted, m_MonoEnabled);
            ChangeStandaloneWindowsAsset(DotNetRestricted + Il2CppSuffix, !m_MonoEnabled);

            ChangeAndroidAsset(PluginAndroid, m_MonoEnabled);
            ChangeAndroidAsset(PluginAndroid + Il2CppSuffix, !m_MonoEnabled);
        }

        private static void ChangeStandaloneWindowsAsset(string assetName, bool enable)
        {
            PluginImporter[] importers = GetAssets(assetName)
                .Where(x => x.assetPath.Contains(assetName + DllExtension))
                .ToArray();

            for (int i = 0; i < importers.Length; i++)
            {
                PluginImporter importer = importers[i];
                bool is64Folder = importer.assetPath.Contains(x64);
                bool is86Folder = importer.assetPath.Contains(x86) && !is64Folder;

                importer.SetCompatibleWithAnyPlatform(false);
                SetEditorCompatibility(ref importer, enable);

                SetCompatibility(ref importer, enable && (is64Folder || !is86Folder),
                    BuildTarget.StandaloneWindows64,
                    BuildTarget.StandaloneLinux64,
                    BuildTarget.StandaloneOSX,
                    BuildTarget.Android);

                SetCompatibility(ref importer,
                    enable && (!is64Folder || is86Folder),
                    BuildTarget.StandaloneWindows,
                    BuildTarget.Android);

                UpdateAsset(importer);
            }
        }

        private static void ChangeStandaloneMacAsset(string assetName, bool enable)
        {
            PluginImporter[] importers = GetAssets(assetName)
                .Where(x => x.assetPath.Contains(assetName + BundleExtension))
                .ToArray();

            for (int i = 0; i < importers.Length; i++)
            {
                PluginImporter importer = importers[i];
                SetEditorCompatibility(ref importer, enable);
                SetCompatibility(ref importer, enable, BuildTarget.StandaloneOSX);
                importer.isPreloaded = enable;
                UpdateAsset(importer);
            }
        }

        private static void ChangeAndroidAsset(string assetName, bool enable)
        {
            PluginImporter[] importers = GetAssets(assetName)
                .Where(x => x.assetPath.Contains(AndroidRelativePath) && x.assetPath.Contains(assetName + DllExtensionLinux))
                .ToArray();

            for (int i = 0; i < importers.Length; i++)
            {
                PluginImporter importer = importers[i];
                SetCompatibility(ref importer, enable, BuildTarget.Android);
                UpdateAsset(importer);
            }
        }

        private static PluginImporter[] GetAssets(string assetName)
        {
            List<PluginImporter> importers = new List<PluginImporter>();

            string[] searchInFolders = { Utils.CombinePaths(Utils.CohtmlUpmPath, "Plugins") };
            string[] assetsGuids = AssetDatabase.FindAssets(assetName, searchInFolders);
            foreach (string guid in assetsGuids)
            {
                PluginImporter importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as PluginImporter;
                if (importer)
                {
                    importers.Add(importer);
                }
            }

            return importers.ToArray();
        }

        private static void SetEditorCompatibility(ref PluginImporter importer, bool enable)
        {
            importer.SetCompatibleWithEditor(enable);
        }

        private static void SetCompatibility(ref PluginImporter importer, bool enable, params BuildTarget[] targets)
        {
            foreach (BuildTarget target in targets)
            {
                importer.SetCompatibleWithPlatform(target, enable);
            }
        }

        private static void UpdateAsset(PluginImporter importer)
        {
            AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceSynchronousImport);            
        }
    }
}