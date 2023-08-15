using System;
using cohtml.Net;

namespace cohtml
{
public class UnityPluginListener : IUnityPluginListener
{
	public Action<IntPtr> PreloadedTextureReleased;

	/// <summary>
	/// Subscribe to create a custom Cohtml Library initialization.
	/// </summary>
	public Func<ILibrary> OnInitializeLibrary;

	/// <summary>
	/// Subscribe to create a custom Cohtml UISystem initialization.
	/// </summary>
	public Func<ISystemSettings, IUISystem> OnInitializeSystem;

	/// <summary>
	/// Subscribe to provide a Cohtml license check. Return true if the license is empty or invalid.
	/// </summary>
	public Func<bool> OnHaveLicenseCheck;

	public override void OnPreloadedTextureReleased(IntPtr texturePtr)
	{
		PreloadedTextureReleased?.Invoke(texturePtr);
	}

	public override void OnUserImageDropped(IntPtr texturePtr)
	{
		PreloadedTextureReleased?.Invoke(texturePtr);
	}

	public override void OnWorkAvailable(WorkType type)
	{
		Library.ScheduleWork(type);
	}

	public override void Dispose()
	{
		PreloadedTextureReleased = null;
		base.Dispose();
	}
}
}
