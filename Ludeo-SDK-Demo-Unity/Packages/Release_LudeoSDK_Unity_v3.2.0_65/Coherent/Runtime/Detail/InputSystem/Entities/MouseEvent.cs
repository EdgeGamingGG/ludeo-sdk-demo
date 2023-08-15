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

using cohtml.Net;
using UnityEngine;

namespace cohtml.InputSystem
{
public class MouseEvent : PointerEvent, IMouseEventData
{
	private MouseEventData m_EventData = new MouseEventData();

	public override EventType PointerEventType
	{
		get
		{
			switch (Type)
			{
				case MouseEventData.EventType.MouseMove:
					return EventType.Move;
				case MouseEventData.EventType.MouseDown:
					return EventType.Down;
				default:
					return EventType.Up;
			}
		}
	}

	public MouseEventData.EventType Type
	{
		get => m_EventData.Type;
		set => m_EventData.Type = value;
	}

	public MouseEventData.MouseButton Button
	{
		get => m_EventData.Button;
		set => m_EventData.Button = value;
	}

	public float WheelX
	{
		get => m_EventData.WheelX;
		set => m_EventData.WheelX = value;
	}

	public float WheelY
	{
		get => m_EventData.WheelY;
		set => m_EventData.WheelY = value;
	}

	public int DeltaX
	{
		get => m_EventData.DeltaX;
		set => m_EventData.DeltaX = value;
	}

	public int DeltaY
	{
		get => m_EventData.DeltaY;
		set => m_EventData.DeltaY = value;
	}

	public EventModifiersState Modifiers
	{
		get => m_EventData.Modifiers;
		set => m_EventData.Modifiers = value;
	}

	public EventMouseModifiersState MouseModifiers
	{
		get => m_EventData.MouseModifiers;
		set => m_EventData.MouseModifiers = value;
	}

	public override void Send(CohtmlView view)
	{
		view.View?.MouseEvent(SetNativePosition(GetViewPosition(view)));
		RemoveView(view);
	}

	public MouseEventData SetNativePosition(Vector2Int position)
	{
		m_EventData.X = position.x;
		m_EventData.Y = position.y;
		return m_EventData;
	}
}
}
