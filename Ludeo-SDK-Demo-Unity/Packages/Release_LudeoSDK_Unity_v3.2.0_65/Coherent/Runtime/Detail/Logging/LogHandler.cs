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

namespace cohtml
{
public class LogHandler : ILogHandler
{
	private static LogHandler s_Instance;
	private static ILogWriter s_Writer;

	public static ILogWriter Writer
	{
		get
		{
			if (s_Writer == null)
			{
				s_Writer = new LogWriter();
				Log("System: Unable to find custom log handler. " +
				    "Falling back to default log handler.");
			}

			return s_Writer;
		}
		set => s_Writer = value;
	}

	public static LogHandler Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new LogHandler();
			}

			return s_Instance;
		}
	}

	public static void Log(string message)
	{
		Instance.WriteLog(Severity.Info, message);
	}

	public static void LogWarning(string message)
	{
		Instance.WriteLog(Severity.Warning, message);
	}

	public static void LogError(string message)
	{
		Instance.WriteLog(Severity.Error, message);
	}

	public override void WriteLog(Severity severity, string message, uint length)
	{
		WriteLog(severity, message);
	}

	public override void Dispose()
	{
		base.Dispose();
		s_Instance = null;
	}

	public void WriteLog(Severity severity, string message)
	{
		Writer.Log(severity, message);
	}
}
}
