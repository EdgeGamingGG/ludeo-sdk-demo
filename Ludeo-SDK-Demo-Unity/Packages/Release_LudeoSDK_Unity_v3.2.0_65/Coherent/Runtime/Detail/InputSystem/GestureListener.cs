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

namespace cohtml.InputSystem
{
public class GestureListener : IGestureListener
{
	private readonly InputBase m_Input;

	public GestureListener(InputBase input)
	{
		m_Input = input;
	}

	public override void OnPanRecognized(
		float gestureBeginPosX,
		float gestureBeginPosY,
		float currentPosX,
		float currentPosY,
		float deltaX,
		float deltaY)
	{
		GestureEvent gestureEvent = new GestureEvent
		{
			StartLocationX = gestureBeginPosX,
			StartLocationY = gestureBeginPosY,
			CurrentLocationX = currentPosX,
			CurrentLocationY = currentPosY
		};

		gestureEvent.SetDeltaX(deltaX);
		gestureEvent.SetDeltaY(deltaY);
		gestureEvent.Type = GestureEventData.EventType.PanStart;
	}

	public override void OnPanCompleted(float endPosX, float endPosY)
	{
		GestureEvent gestureEvent = new GestureEvent
		{
			CurrentLocationX = endPosX,
			CurrentLocationY = endPosY,
			Type = GestureEventData.EventType.PanEnd
		};
	}

	public override void OnFlingCompleted(float endPosX, float endPosY, float flingStartX, float flingStartY, float flingMotionMs)
	{
		GestureEvent gestureEvent = new GestureEvent
		{
			CurrentLocationX = endPosX,
			CurrentLocationY = endPosY,
			StartLocationX = flingStartX,
			StartLocationY = flingStartY
		};

		gestureEvent.SetDuration(flingMotionMs);
		gestureEvent.Type = GestureEventData.EventType.Fling;
	}

	public override void OnTapRecognized(float positionX, float positionY)
	{
		GestureEvent gestureEvent = new GestureEvent
		{
			CurrentLocationX = positionX,
			CurrentLocationY = positionY,
			Type = GestureEventData.EventType.Tap
		};
	}
}
}
