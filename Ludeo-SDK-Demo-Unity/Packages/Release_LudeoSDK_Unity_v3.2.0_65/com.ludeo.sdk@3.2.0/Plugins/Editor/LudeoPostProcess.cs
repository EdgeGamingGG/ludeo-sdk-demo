namespace LudeoSDKUnityEditor
{
    using UnityEditor;
    using cohtml;

    public class LudeoPostProcess : AssetPostprocessor
    {
#if UNITY_2021_2_OR_NEWER
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)        
#endif
        {                     
            string ludeoSdkName = LudeoUnityEditorHelpers.GetLudeoSDKAssemblyName() + ".dll";

            for (int i = 0; i < importedAssets.Length; ++i)
            {
                if (importedAssets[i].EndsWith(ludeoSdkName))
                {
                    LudeoUnityEditorHelpers.SetupLudeoAssets();

                    //ScriptingBackendAdjuster.UpdateCohtmlScriptingBackend();

                    AssetDatabase.Refresh();
                }
            }            
        }     
    }
}