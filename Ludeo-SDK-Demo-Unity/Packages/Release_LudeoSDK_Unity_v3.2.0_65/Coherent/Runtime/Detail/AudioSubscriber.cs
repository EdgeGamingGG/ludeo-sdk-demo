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
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace cohtml
{
public class AudioSubscriber : IDisposable
{
	public const int MaxBufferSize = 8192;
	public const int PreBufferSize = 2048;

	public CohtmlView View { get; set; }

	private AudioClip m_AudioClip;
	private float[] m_AudioClipSamples;

	private int m_BufferPosition;
	private float[] m_AudioBuffer;

	private bool m_AutoPlayOnDataReceived;
	private int ptrSize;

	private HashSet<int> StreamIds;

	public AudioSubscriber(CohtmlView view)
	{
		View = view;
		StreamIds = new HashSet<int>();
	}

	public void Subscribe()
	{
		View.Listener.OnAudioStreamCreate += CreateStream;
		View.Listener.OnAudioDataReceive += ReceiveDataForStream;
		View.Listener.OnAudioStreamPlayed += AutoPlayStream;
		View.Listener.OnAudioStreamPaused += StopStream;
		View.Listener.OnAudioStreamVolumeChange += VolumeChanged;
		View.Listener.OnAudioStreamEnds += StopStream;
		View.Listener.OnAudioStreamClose += DestroyStream;
	}

	private void CreateStream(int id, int bitDepth, int channels, float samplingRate)
	{
		m_AudioClip = AudioClip.Create("CohtmlAudioStream_" + id,
			(int)samplingRate,
			channels,
			(int)samplingRate,
			false);

		m_AudioBuffer = new float[channels * PreBufferSize];
		m_AudioClipSamples = new float[channels * (int)samplingRate];

		View.AudioSource.clip = m_AudioClip;
	}

	private void ReceiveDataForStream(int id, int samples, IntPtr pcm, int channels)
	{
		LoadReceivedAudioSamples(samples, pcm, channels);

		SynchronizeAudioAndVideo();

		// Update buffer position
		m_BufferPosition += samples;
		if (m_BufferPosition >= m_AudioClip.samples)
		{
			m_BufferPosition -= m_AudioClip.samples;
		}

		if (!View.AudioSource.isPlaying && m_AutoPlayOnDataReceived && m_BufferPosition > PreBufferSize)
		{
			View.AudioSource.Play();
			m_AutoPlayOnDataReceived = false;
		}
	}

	private void LoadReceivedAudioSamples(int samples, IntPtr pcm, int channels)
	{
		int receivedSamples = channels * samples;
		// Expand the buffer when get more samples than current array of samples
		if (receivedSamples > m_AudioBuffer.Length)
		{
			m_AudioBuffer = new float[receivedSamples];
		}

		int i, j;
		// Copy the samples from unmanaged memory into audio buffer
		for (i = 0; i < channels; i++)
		{
			IntPtr channelData = Marshal.ReadIntPtr(pcm, i * ptrSize);
			Marshal.Copy(channelData, m_AudioBuffer, i * samples, samples);
		}

		// Load new samples into audio clip samples array
		if (m_BufferPosition * channels + receivedSamples > m_AudioClipSamples.Length)
		{
			int currentSamplesLength = m_AudioClipSamples.Length / channels - m_BufferPosition;
			SetAudioClipSamples(currentSamplesLength, 0, m_BufferPosition);

			// Get new part of samples
			SetAudioClipSamples(samples - currentSamplesLength, currentSamplesLength, 0);
		}
		else
		{
			SetAudioClipSamples(samples, 0, m_BufferPosition);
		}

		m_AudioClip.SetData(m_AudioClipSamples, 0);

		void SetAudioClipSamples(int length, int offset, int startPosition)
		{
			for (i = 0; i < length; i++)
			{
				for (j = 0; j < channels; j++)
				{
					m_AudioClipSamples[(startPosition + i) * channels + j] = m_AudioBuffer[offset + i + j * samples];
				}
			}
		}
	}

	private void SynchronizeAudioAndVideo()
	{
		int timeSamples = View.AudioSource.timeSamples;
		bool inSync = m_BufferPosition >= timeSamples &&
		              m_BufferPosition - timeSamples < MaxBufferSize ||
		              m_BufferPosition < timeSamples &&
		              (m_AudioClip.samples - timeSamples) + m_BufferPosition < MaxBufferSize;

		if (!inSync)
		{
			LogHandler.LogWarning("Audio playback was out of sync with the video. " +
			                      "Synchronizing audio now, this may cause a skip.");
			m_BufferPosition = timeSamples + PreBufferSize;
		}
	}

	private void AutoPlayStream(int id)
	{
		View.AudioSource.ignoreListenerPause = true;
		View.AudioSource.loop = true;
		m_AutoPlayOnDataReceived = true;
	}


	private void VolumeChanged(int id, float volume)
	{
		View.AudioSource.volume = volume;
		View.AudioSource.mute = View.AudioSource.volume == 0;
	}

	private void StopStream(int id)
	{
		View.AudioSource.Stop();
		m_BufferPosition = 0;
	}

	private void DestroyStream(int id)
	{
		if (View && View.AudioSource)
		{
			StopStream(id);
			UnityEngine.Object.Destroy(m_AudioClip);
		}
	}

	public void Dispose()
	{
		if (!View)
		{
			return;
		}

		View.Listener.OnAudioStreamCreate -= CreateStream;
		View.Listener.OnAudioDataReceive -= ReceiveDataForStream;
		View.Listener.OnAudioStreamPlayed -= AutoPlayStream;
		View.Listener.OnAudioStreamPaused -= StopStream;
		View.Listener.OnAudioStreamEnds -= StopStream;
		View.Listener.OnAudioStreamClose -= DestroyStream;

		while (StreamIds.Count > 0)
		{
			DestroyStream(StreamIds.First());
		}
	}
}
}
