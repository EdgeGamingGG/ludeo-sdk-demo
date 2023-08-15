using cohtml;
using UnityEngine;
using cohtml.Restricted;

public class CohtmlInitializer : MonoBehaviour
{
	public void Awake()
	{
        Restrictor.SetWhiteListCallback(LudeoSDK.LudeoHelpers.FetchWhiteListedDomains);

        Library.UnityPluginListener = new UnityPluginListener();

        Library.UnityPluginListener.OnInitializeLibrary += () =>
        {
            return Restrictor.InitializeLibrary(LibraryParamsManager.GetCompositeLibraryParams(),
                Library.UnityPluginListener,
                SystemInfo.deviceModel);
        };

        Library.UnityPluginListener.OnInitializeSystem += settings =>
        {
            return Restrictor.InitializeSystem(settings);
        };

        Library.UnityPluginListener.OnHaveLicenseCheck += () =>
        {
            return Restrictor.HaveLicense();
        };
    }
}
