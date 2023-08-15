/*
This file is part of Cohtml, Gameface and Prysm - modern user interface technologies.

Copyright (c) 2012-2023 Coherent Labs AD and/or its licensors. All
rights reserved in all media.

The coded instructions, statements, computer programs, and/or related
material (collectively the "Data") in these files contain confidential
and unpublished information proprietary Coherent Labs and/or its
licensors, which is protected by United States of America federal
copyright law and by international treaties.

This software or source code is supplied under the terms of a license
agreement and nondisclosure agreement with Coherent Labs AD and may
not be copied, disclosed, or exploited except in accordance with the
terms of that agreement. The Data may not be disclosed or distributed to
third parties, in whole or in part, without the prior written consent of
Coherent Labs AD.

COHERENT LABS MAKES NO REPRESENTATION ABOUT THE SUITABILITY OF THIS
SOURCE CODE FOR ANY PURPOSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT
HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER, ITS AFFILIATES,
PARENT COMPANIES, LICENSORS, SUPPLIERS, OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OR PERFORMANCE OF THIS SOFTWARE OR SOURCE CODE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using cohtml.Net;
using renoir;
using UnityEngine;

namespace cohtml
{
public class UserImagesManager : IPreloadTextureHandable
{
	public Dictionary<IntPtr, Texture> PreloadedTextures { get; private set; }
	static uint s_NextImageHandle;
	Dictionary<string, CohtmlLiveView> m_LiveViews;

	uint[] m_LiveViewHandles = new uint[128];
	uint m_LiveViewHandlesSize;

	CohtmlUISystem m_System;

	public UserImagesManager(CohtmlUISystem system)
	{
		m_System = system;
		m_LiveViews = new Dictionary<string, CohtmlLiveView>();
		PreloadedTextures = new Dictionary<IntPtr, Texture>();
	}

	private uint NextImageHandle
	{
		get { return ++s_NextImageHandle; }
	}

	public void SubscribePreloadedTextureReleased()
	{
		if (Library.UnityPluginListener.PreloadedTextureReleased == null)
		{
			Library.UnityPluginListener.PreloadedTextureReleased += OnPreloadedTextureReleased;
		}
	}

	public bool AddLiveView(string name, CohtmlLiveView liveViewComponent)
	{
		if (!m_LiveViews.ContainsKey(name))
		{
			m_LiveViews.Add(name, liveViewComponent);
			return true;
		}

		return false;
	}

	public void RemoveLiveView(string url)
	{
		if (m_LiveViews.ContainsKey(url))
		{
			RemoveImageHandle(m_LiveViews[url].ImageHandle);
			m_LiveViews.Remove(url);
		}
	}

	public void RemoveLiveViews()
	{
		foreach (KeyValuePair<string, CohtmlLiveView> liveView in m_LiveViews)
		{
			MonoBehaviour.Destroy(liveView.Value);
		}

		m_LiveViews.Clear();
	}

	private void AddImageHandle(uint imageHandle)
	{
		if (m_LiveViewHandlesSize == m_LiveViewHandles.Length)
		{
			Array.Resize(ref m_LiveViewHandles, (int)m_LiveViewHandlesSize * 2);
		}

		m_LiveViewHandles[m_LiveViewHandlesSize++] = imageHandle;
	}

	private void RemoveImageHandle(uint liveViewHandle)
	{
		if (liveViewHandle != 0)
		{
			var index = Array.FindIndex(m_LiveViewHandles, v => v == liveViewHandle);
			m_LiveViewHandles[index] = m_LiveViewHandles[m_LiveViewHandlesSize - 1];
			m_LiveViewHandlesSize--;
		}
	}

	public ResourceResponse.UserImageData CreateLiveViewImageData(string url)
	{
		if (m_LiveViews[url] && m_LiveViews[url].TargetTexture != null)
		{
			m_LiveViews[url].ImageHandle = NextImageHandle;

			ResourceResponse.UserImageData data = CreateUserImageData(m_LiveViews[url].TargetTexture, m_LiveViews[url].ImageHandle);

			AddImageHandle(m_LiveViews[url].ImageHandle);
			return data;
		}

		LogHandler.LogError("RenderTexture " + url + " not found.");
		return null;
	}

	public ResourceResponse.UserImageData CreateUserImageData(Texture texture, uint imageHandle = 0)
	{
		IntPtr texturePtr = IntPtr.Zero;
		if (LibraryParamsManager.ShouldUseCSharpBackend)
		{
			System.Runtime.InteropServices.GCHandle gch = System.Runtime.InteropServices.GCHandle.Alloc(texture);
			texturePtr = System.Runtime.InteropServices.GCHandle.ToIntPtr(gch);
		}
		else
		{
			texturePtr = texture.GetNativeTexturePtr();
		}

		ResourceResponse.UserImageData data = new ResourceResponse.UserImageData
		{
			Width = (uint)texture.width,
			Height = (uint)texture.height,
			ContentRectX = 0,
			ContentRectY = 0,
			Format = (int)BackendUtilities.UnityToRenoirPixelFormat(texture.graphicsFormat),
			Texture = texturePtr,
			SystemOwnerId = m_System.Id,
			ImageHandle = imageHandle,

			#if UNITY_UV_STARTS_AT_TOP
			Origin = (int)ImageOrigin.TopLeft,
			#else
			Origin = (int)ImageOrigin.BottomLeft,
			#endif
		};

		data.ContentRectWidth = data.Width;
		data.ContentRectHeight = data.Height;

		return data;
	}

	public void UpdateLiveViews()
	{
		if (m_System.IsReady && m_LiveViewHandlesSize > 0)
		{
			m_System.SystemNative.UserImagesChanged(m_LiveViewHandles, m_LiveViewHandlesSize);
		}
	}

	public bool ContainsLiveView(string uri)
	{
		return m_LiveViews.ContainsKey(uri);
	}

	private void OnPreloadedTextureReleased(IntPtr texturePtr)
	{
		ReleasePreloadedTexture(texturePtr);
	}

	public void AddPreloadedTexture(IntPtr texturePtr, Texture texture)
	{
		if (!PreloadedTextures.ContainsKey(texturePtr))
		{
			PreloadedTextures.Add(texturePtr, null);
		}

		PreloadedTextures[texturePtr] = texture;
	}

	public Texture GetPreloadedTexture(IntPtr texturePtr)
	{
		if (PreloadedTextures.ContainsKey(texturePtr))
		{
			return PreloadedTextures[texturePtr];
		}

		return null;
	}

	public bool ReleasePreloadedTexture(IntPtr texturePtr)
	{
		if (!PreloadedTextures.ContainsKey(texturePtr))
		{
			return false;
		}

		Resources.UnloadAsset(PreloadedTextures[texturePtr]);

		return PreloadedTextures.Remove(texturePtr);
	}

	public void ReleaseAllPreloadedTextures()
	{
		foreach (Texture texture in PreloadedTextures.Values)
		{
			Resources.UnloadAsset(texture);
		}

		PreloadedTextures.Clear();
	}

	public void Dispose()
	{
		RemoveLiveViews();
		m_LiveViewHandles = null;

		ReleaseAllPreloadedTextures();
		PreloadedTextures = null;
	}
}
}
