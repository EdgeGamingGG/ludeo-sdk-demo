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
using cohtml.Net;
using UnityEngine;

namespace cohtml.InputSystem
{
public class TouchEventCollection : IViewSendable
{
	private TouchEvent[] m_InputTouches;
	private TouchEventData[] m_ViewTouches;
	private uint m_ViewTouchesCount;

	public int Capacity => cohtmlNative.MAX_TRACKING_TOUCHES;

	public TouchEventCollection()
	{
		m_InputTouches = new TouchEvent[Capacity];
		m_ViewTouches = new TouchEventData[Capacity];

		for (uint i = 0; i < Capacity; i++)
		{
			m_InputTouches[i] = new TouchEvent(i);
		}
	}

	public TouchEvent this[uint key]
	{
		get
		{
			if (key >= Capacity)
			{
				throw new IndexOutOfRangeException();
			}

			return m_InputTouches[key];
		}
		set
		{
			if (key >= Capacity)
			{
				throw new IndexOutOfRangeException();
			}

			m_InputTouches[key] = value;
		}
	}

	public TouchEvent[] ActiveTouches
	{
		get { return Array.FindAll(m_InputTouches, x => x.IsActive); }
	}

	public TouchEvent[] All => m_InputTouches;

	public void Send(CohtmlView view)
	{
		SetPositionByView(view, ref m_ViewTouches);
		view.View.TouchEvent(m_ViewTouches, m_ViewTouchesCount);
		CohtmlInputHandler.Input.UpdateTouches(m_ViewTouches, m_ViewTouchesCount);
	}

	private void SetPositionByView(CohtmlView view, ref TouchEventData[] viewTouches)
	{
		m_ViewTouchesCount = 0;
		for (int i = 0; i < Capacity; i++)
		{
			if (m_InputTouches[i].IsActive)
			{
				Vector2Int ViewPosition = m_InputTouches[i].GetViewPosition(view);
				if (ViewPosition != default)
				{
					viewTouches[m_ViewTouchesCount] = m_InputTouches[i].SetNativePosition(ViewPosition);
					m_ViewTouchesCount++;
					m_InputTouches[i].RemoveView(view);
				}
			}
		}
	}

	public void UpdateEventTypes()
	{
		for (int i = 0; i < Capacity; i++)
		{
			m_InputTouches[i].UpdateEventType();
		}
	}
}
}
