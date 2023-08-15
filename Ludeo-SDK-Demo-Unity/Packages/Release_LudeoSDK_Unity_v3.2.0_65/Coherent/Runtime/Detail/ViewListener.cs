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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using cohtml.InputSystem;
using cohtml.Net;
using UnityEngine;

namespace cohtml
{
public class ViewListener : IViewListener
{
	public event Action BindingsReleased;

	public Action<int, int, int, float> OnAudioStreamCreate { get; set; }
	public Action<int, int, IntPtr, int> OnAudioDataReceive { get; set; }
	public Action<int> OnAudioStreamPlayed { get; set; }
	public Action<int> OnAudioStreamPaused { get; set; }
	public Action<int, float> OnAudioStreamVolumeChange { get; set; }
	public Action<int> OnAudioStreamEnds { get; set; }
	public Action<int> OnAudioStreamClose { get; set; }

	public override void OnBindingsReleased()
	{
		if (BindingsReleased != null)
		{
			BindingsReleased();
		}
	}

	public event Action<string> FinishLoad;

	public event Action OnWebSocketOpened;

	public override void OnFinishLoad(string url)
	{
		if (FinishLoad != null)
		{
			FinishLoad(url);
		}
	}

	public event Action DOMBuilt;

	public override void OnDOMBuilt()
	{
		if (DOMBuilt != null)
		{
			DOMBuilt();
		}
	}

	public event Action<string, string> LoadFailed;

	public override void OnLoadFailed(string url, string error)
	{
		if (LoadFailed != null)
		{
			LoadFailed(url, error);
		}
	}

	public event Func<string, bool> NavigateTo;

	public override bool OnNavigateTo(string url)
	{
		bool result = true;

		if (NavigateTo != null)
		{
			result = NavigateTo(url);
		}

		return result;
	}

	public event Action ReadyForBindings;

	public override void OnReadyForBindings()
	{
		if (ReadyForBindings != null)
		{
			ReadyForBindings();
		}
	}

	public event Action ScriptContextCreated;

	public override void OnScriptContextCreated()
	{
		if (ScriptContextCreated != null)
		{
			ScriptContextCreated();
		}
	}

	public override Actions OnNodeKeyEvent(INodeProxy node, IKeyEventData eventData, IntPtr userData, PhaseType phase)
	{
		return CohtmlInputHandler.HandleInputEvent(node, phase);
	}

	public override Actions OnNodeMouseEvent(INodeProxy node, IMouseEventData eventData, IntPtr userData, PhaseType phase)
	{
		return CohtmlInputHandler.HandleInputEvent(node, phase);
	}

	public override Actions OnNodeTouched(INodeProxy node, ITouchEventData eventData, IntPtr userData, PhaseType phase)
	{
		return CohtmlInputHandler.HandleInputEvent(node, phase);
	}

	public override IClientSideSocket OnCreateWebSocket(ISocketListener listener, string url, IntPtr protocolsPtr, uint protocolsCount)
	{
		int ptrSize = Marshal.SizeOf(typeof(IntPtr));
		string[] protocols = new string[protocolsCount];

		for (int i = 0; i < protocolsCount; i++)
		{
			IntPtr ptr = Marshal.ReadIntPtr(protocolsPtr, i * ptrSize);
			protocols[i] = Marshal.PtrToStringAnsi(ptr);
		}

		try
		{
				cohtmlWebSocketWrapper = new CohtmlWebSocketWrapper(listener, url, protocols, OnWebSocketOpened);
				return cohtmlWebSocketWrapper;
		}
		catch (Exception e)
		{
			LogHandler.Log(e.Message);
			return null;
		}
	}

	public override void OnClipboardTextSet(string text, uint lengthBytes)
	{
		GUIUtility.systemCopyBuffer = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text), 0, (int)lengthBytes);
	}

	public override void OnClipboardTextGet(IClipboardData setDataObject)
	{
		setDataObject.Set(GUIUtility.systemCopyBuffer, (uint)Utils.EncodeString(GUIUtility.systemCopyBuffer).Length);
	}

	public override void OnAudioStreamCreated(int id, int bitDepth, int channels, float samplingRate)
	{
		OnAudioStreamCreate?.Invoke(id, bitDepth, channels, samplingRate);
	}

	public override void OnAudioDataReceived(int id, int samples, IntPtr pcm, int channels)
	{
		OnAudioDataReceive?.Invoke(id, samples, pcm, channels);
	}

	public override void OnAudioStreamPlay(int id)
	{
		OnAudioStreamPlayed?.Invoke(id);
	}

	public override void OnAudioStreamPause(int id)
	{
		OnAudioStreamPaused?.Invoke(id);
	}

	public override void OnAudioStreamEnded(int id)
	{
		OnAudioStreamEnds?.Invoke(id);
	}

	public override void OnAudioStreamClosed(int id)
	{
		OnAudioStreamClose?.Invoke(id);
	}

	public override void OnAudioStreamVolumeChanged(int id, float volume)
	{
		OnAudioStreamVolumeChange?.Invoke(id, volume);
	}

		private CohtmlWebSocketWrapper cohtmlWebSocketWrapper;
		public void OnApplicationQuit()
        {			
            cohtmlWebSocketWrapper?.OnApplicationQuit();
        }
}
}
