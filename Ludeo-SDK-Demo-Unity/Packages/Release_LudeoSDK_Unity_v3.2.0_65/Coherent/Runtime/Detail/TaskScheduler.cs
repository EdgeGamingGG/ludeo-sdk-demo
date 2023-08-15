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
using System.Threading;
using cohtml.Net;

namespace cohtml
{
public class TaskScheduler
{
	private readonly AutoResetEvent m_LayoutReset;
	private readonly Thread m_LayoutThread;
	private volatile bool m_IsLayoutThreadRunning;
	private static int s_ThreadCount = -1;
	private readonly CountdownEvent m_ThreadPoolCountdownEvent;

	public TaskScheduler()
	{
		m_LayoutReset = new AutoResetEvent(false);
		m_LayoutThread = new Thread(() =>
			{
				while (m_IsLayoutThreadRunning)
				{
					Library.ExecuteWork(WorkType.WT_Layout);
					m_LayoutReset.WaitOne();
				}
			}
		);

		// the countdown event start with 1 and keep it that way in the game cycle.
		// When it reach 0 the Countdown event will assumed as finished and cannot increment it anymore.
		m_ThreadPoolCountdownEvent = new CountdownEvent(1);
	}

	public static uint ThreadCount
	{
		get
		{
			if (s_ThreadCount == -1)
			{
				ThreadPool.GetMaxThreads(out int _, out s_ThreadCount);
			}

			return (uint)s_ThreadCount;
		}
	}

	public void ScheduleTask(WorkType type)
	{
		switch (type)
		{
			case WorkType.WT_Resources:
				m_ThreadPoolCountdownEvent.AddCount();
				ThreadPool.QueueUserWorkItem(_ =>
				{
					Library.ExecuteWork(WorkType.WT_Resources);
					m_ThreadPoolCountdownEvent.Signal();
				});
				break;
			case WorkType.WT_Layout:
				m_LayoutReset.Set();
				break;
			default:
				throw new NotImplementedException($"{type} is not implemented!");
		}
	}

	public void Start()
	{
		m_IsLayoutThreadRunning = true;
		m_LayoutThread.Start();
	}

	public void Stop()
	{
		m_IsLayoutThreadRunning = false;
		m_LayoutReset.Set();
		m_LayoutThread.Join();

		// The last signal decrement the countdown balance the tasks with initial count.
		m_ThreadPoolCountdownEvent.Signal();
		m_ThreadPoolCountdownEvent.Wait();
	}

	public void Dispose()
	{
		Stop();
	}
}
}
