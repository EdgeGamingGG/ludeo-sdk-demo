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

using UnityEngine;
using System;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using System.Threading;
using renoir;

namespace cohtml
{
[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void OnDrawSubLayer(uint viewId, ref renoir.DrawData drawData);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void OnCompositionAdded(uint viewId, string compositionId);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void OnCompositionRemoved(uint viewId, string compositionId);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void OnCompositionNativeTexture(uint viewId, string compositionId, IntPtr tex);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
delegate void OnCompositionVisibilityChanged(uint viewId, string compositionId, bool visible);

public class Compositor
{
	class CompositionData
	{
		public DrawData Data;
		public bool Visible;
		public UnityEngine.Texture2D NativeTexture;
		public bool HasTexture;
	}

	// view -> (composition -> composition data)
	private Dictionary<uint, Dictionary<string, CompositionData>> m_Compositions = new Dictionary<uint, Dictionary<string, CompositionData>>();
	private Mutex m_CompositionsMutex = new Mutex(false);

	[AttributeUsage(AttributeTargets.Method)]
	public class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type t)
		{
		}
	}

	internal delegate void ManagedCallbackAOT();

	[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
	private static void OnDrawSubLayerWrapper(uint viewId, ref renoir.DrawData drawData)
	{
		cohtml.Library.UnityCompositor.OnDrawSubLayer(viewId, ref drawData);
	}

	[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
	private static void OnCompositionAddedWrapper(uint viewId, string compositionId)
	{
		cohtml.Library.UnityCompositor.OnCompositionAdded(viewId, compositionId);
	}

	[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
	private static void OnCompositionRemovedWrapper(uint viewId, string compositionId)
	{
		cohtml.Library.UnityCompositor.OnCompositionRemoved(viewId, compositionId);
	}

	[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
	private static void OnCompositionVisibilityChangedWrapper(uint viewId, string compositionId, bool visible)
	{
		cohtml.Library.UnityCompositor.OnCompositionVisibilityChanged(viewId, compositionId, visible);
	}

	[MonoPInvokeCallback(typeof(ManagedCallbackAOT))]
	private static void OnCompositionNativeTextureWrapper(uint viewId, string compositionId, IntPtr tex)
	{
		cohtml.Library.UnityCompositor.OnCompositionNativeTexture(viewId, compositionId, tex);
	}

	[DllImport(CohtmlDllImport.DLL, CallingConvention = CallingConvention.Cdecl)]
	private static extern void SetUnityCompositorFunctions(
		OnDrawSubLayer onDrawSubLayer,
		OnCompositionAdded onCompositionAdded,
		OnCompositionRemoved onCompositionRemoved,
		OnCompositionVisibilityChanged onCompositionVisibilityChanged,
		OnCompositionNativeTexture onCompositionNativeTexture);

	public void SendManagedFuncitonsToNative()
	{
		SetUnityCompositorFunctions(
			OnDrawSubLayerWrapper,
			OnCompositionAddedWrapper,
			OnCompositionRemovedWrapper,
			OnCompositionVisibilityChangedWrapper,
			OnCompositionNativeTextureWrapper
		);
	}

	public void RegisterView(uint viewId)
	{
		m_CompositionsMutex.WaitOne();
		m_Compositions[viewId] = new Dictionary<string, CompositionData>();
		m_CompositionsMutex.ReleaseMutex();
	}

	public void DeregisterView(uint viewId)
	{
		m_CompositionsMutex.WaitOne();
		m_Compositions.Remove(viewId);
		m_CompositionsMutex.ReleaseMutex();
	}

	public void PaintCompositionsForView(uint viewId, Material renderMaterial)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];
			foreach (var viewData in viewCompositions)
			{
				var key = viewData.Key;
				var data = viewData.Value;
				if (!data.Visible)
				{
					continue;
				}

				renoir.DrawData drawData = data.Data;
				Texture tex;
				if (Library.ShouldUseCSharpBackend
					&& Library.RenderingBackend.Textures.ContainsKey(drawData.Texture))
				{
					tex = Library.RenderingBackend.Textures[drawData.Texture];
				}
				else if (data.HasTexture)
				{
					tex = data.NativeTexture;
				}
				else
				{
					continue;
				}

				GL.PushMatrix();
				GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
				Graphics.DrawTexture(
					new Rect(
						drawData.Untransformed2DTargetRect.Position.x,
						drawData.Untransformed2DTargetRect.Position.y,
						drawData.Untransformed2DTargetRect.Size.x,
						drawData.Untransformed2DTargetRect.Size.y
					),
					tex,
					new Rect(
						drawData.UVOffset.x,
						1.0f - drawData.UVOffset.y - drawData.UVScale.y,
						drawData.UVScale.x,
						drawData.UVScale.y
					),
					0, 0, 0, 0,
					UnityEngine.Color.white, renderMaterial);
				GL.PopMatrix();
			}
		}
		m_CompositionsMutex.ReleaseMutex();
	}

	private void OnDrawSubLayer(uint viewId, ref renoir.DrawData drawData)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];
			string id = Marshal.PtrToStringAnsi(drawData.SubLayerCompositionId);

			if (viewCompositions.ContainsKey(id))
			{
				if (viewCompositions[id].Data.Texture.Id != drawData.Texture.Id)
				{
					viewCompositions[id].HasTexture = false;
				}
				viewCompositions[id].Data = drawData;
			}
		}
		m_CompositionsMutex.ReleaseMutex();
	}

	private void OnCompositionAdded(uint viewId, string compositionId)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];
			viewCompositions[compositionId] = new CompositionData();
		}
		m_CompositionsMutex.ReleaseMutex();
	}

	private void OnCompositionRemoved(uint viewId, string compositionId)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];
			viewCompositions.Remove(compositionId);
		}
		m_CompositionsMutex.ReleaseMutex();
	}

	private void OnCompositionVisibilityChanged(uint viewId, string compositionId, bool visible)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];
			viewCompositions[compositionId].Visible = visible;
		}
		m_CompositionsMutex.ReleaseMutex();
	}

	private void OnCompositionNativeTexture(uint viewId, string compositionId, IntPtr nativeTex)
	{
		m_CompositionsMutex.WaitOne();
		if (m_Compositions.ContainsKey(viewId))
		{
			var viewCompositions = m_Compositions[viewId];

			renoir.DrawData data = viewCompositions[compositionId].Data;
			viewCompositions[compositionId].HasTexture = true;
			viewCompositions[compositionId].NativeTexture = UnityEngine.Texture2D.CreateExternalTexture(
				(int)data.TextureInfo.Width,
				(int)data.TextureInfo.Height,
				BackendUtilities.RenoirToUnityTextureFormat(data.TextureInfo.Format),
				false, false, nativeTex
			);
		}
		m_CompositionsMutex.ReleaseMutex();
	}

}
}
